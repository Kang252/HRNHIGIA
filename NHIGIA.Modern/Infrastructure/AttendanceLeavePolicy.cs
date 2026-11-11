using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

// Approved leave is an exception to the recurring shift, never a fabricated camera event.
public static class AttendanceLeavePolicy
{
    public static readonly string[] LeaveTypes =
    [
        "Phép năm", "Nghỉ phép", "Nghỉ ốm", "Nghỉ không lương", "Nghỉ kết hôn",
        "Nghỉ thai sản", "Nghỉ việc riêng", "Nghỉ bù", "Khác"
    ];

    public static int SessionMask(string value) => value?.Trim().ToUpperInvariant() switch
    {
        "CẢ NGÀY" or "FULL_DAY" or "FULL" => 3,
        "BUỔI SÁNG" or "MORNING" or "AM" => 1,
        "BUỔI CHIỀU" or "AFTERNOON" or "PM" => 2,
        _ => 0
    };

    public static bool IsApprovedLeave(LeaveRequestModel leave) =>
        leave.StatusCode == "APPROVED" &&
        LeaveTypes.Contains(leave.LeaveType?.Trim(), StringComparer.OrdinalIgnoreCase) &&
        SessionMask(leave.SessionCode) != 0;

    public static IList<AttendanceRecordModel> Project(
        IReadOnlyList<AttendanceRecordModel> attendance, IReadOnlyList<ScheduleModel> schedules,
        IReadOnlyList<LeaveRequestModel> leaves, DateTime fromDate, DateTime toDate, DateTime now,
        IReadOnlyList<ApprovedOvertimeModel> overtime)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        if (to < from || (to - from).TotalDays > 366)
            throw new InvalidOperationException("Khoảng lọc tối đa là 366 ngày.");

        var approved = leaves.Where(x => IsApprovedLeave(x) && x.StartDate.Date <= to && x.EndDate.Date >= from)
            .ToLookup(x => x.UserId);
        var approvedOvertime = overtime.Where(x =>
                string.Equals(x.StatusCode, "APPROVED", StringComparison.OrdinalIgnoreCase) &&
                x.WorkDate.Date >= from && x.WorkDate.Date <= to)
            .ToLookup(x => x.UserId);
        var shifts = schedules.Where(x => x.StatusCode == "ACTIVE").ToLookup(x => x.UserId);
        var rows = attendance.Where(x => x.WorkDate.Date >= from && x.WorkDate.Date <= to)
            .GroupBy(x => (x.UserId, x.WorkDate.Date))
            .ToDictionary(x => x.Key, x => Copy(x.OrderByDescending(r => r.EventCount).First()));

        foreach (var employee in approved)
        {
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (rows.ContainsKey((employee.Key, date))) continue;
                var dayLeaves = employee.Where(x => x.StartDate.Date <= date && x.EndDate.Date >= date).ToList();
                if (dayLeaves.Count == 0) continue;
                var shift = shifts[employee.Key].Where(x => x.EffectiveFrom.Date <= date &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= date) &&
                        (x.WorkDaysMask & (1 << (int)date.DayOfWeek)) != 0)
                    .OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Id).FirstOrDefault();
                // Non-working days in a multi-day request do not become attendance days.
                if (shift == null) continue;
                var owner = dayLeaves[0];
                var row = new AttendanceRecordModel
                {
                    UserId = owner.UserId, PersonId = owner.PersonId, EmployeeCode = owner.EmployeeCode, DisplayName = owner.DisplayName,
                    JobTitle = owner.JobTitle, DepartmentName = owner.DepartmentName, WorkDate = date,
                    ShiftName = shift.ShiftName, ScheduledStart = shift.StartTime, ScheduledEnd = shift.EndTime,
                    GraceMinutes = shift.GraceMinutes, BreakMinutes = shift.BreakMinutes, Source = "Nghỉ đã duyệt"
                };
                Calculate(row, dayLeaves, approvedOvertime[employee.Key]
                    .Where(x => x.WorkDate.Date == date).ToList(), now);
                if (row.ApprovedLeave) rows.Add((employee.Key, date), row);
            }
        }

        foreach (var row in rows.Values)
            Calculate(row,
                approved[row.UserId].Where(x => x.StartDate.Date <= row.WorkDate.Date && x.EndDate.Date >= row.WorkDate.Date).ToList(),
                approvedOvertime[row.UserId].Where(x => x.WorkDate.Date == row.WorkDate.Date).ToList(), now);

        return rows.Values.OrderByDescending(x => x.WorkDate).ThenBy(x => x.DisplayName).ToList();
    }

    private static void Calculate(AttendanceRecordModel row, IReadOnlyList<LeaveRequestModel> leaves,
        IReadOnlyList<ApprovedOvertimeModel> overtime, DateTime now)
    {
        row.LateMinutes = row.EarlyMinutes = row.WorkedMinutes = 0;
        row.IsProvisional = row.ApprovedLeave = false;
        row.LeaveSession = row.LeaveDescription = null;
        row.ExpectedStartAt = row.ExpectedEndAt = null;

        var mask = leaves.Aggregate(0, (current, leave) => current | SessionMask(leave.SessionCode));
        var authorizedOvertime = ResolveOvertimeWindows(overtime);
        if (!row.ScheduledStart.HasValue || !row.ScheduledEnd.HasValue)
        {
            if (authorizedOvertime.Count > 0)
            {
                CalculateOvertimeOnly(row, authorizedOvertime, now);
                return;
            }
            DescribeLeave(row, leaves, mask);
            row.StatusCode = mask == 3 ? "ON_LEAVE" : "MISSING_SCHEDULE";
            return;
        }

        var start = row.WorkDate.Date.Add(row.ScheduledStart.Value);
        var end = row.WorkDate.Date.Add(row.ScheduledEnd.Value);
        if (end <= start) end = end.AddDays(1);
        var expectedStart = start;
        var expectedEnd = end;
        var noon = row.WorkDate.Date.AddHours(12);
        // Company day shifts: morning ends at 12:00, afternoon begins at 13:30.
        // Older schedules may still store BreakMinutes=60; do not infer the return time from it.
        // For overnight shifts AM/PM represent the first/second half of the shift.
        var overnight = end.Date > start.Date;
        var split = overnight ? start.AddTicks((end - start - TimeSpan.FromMinutes(row.BreakMinutes)).Ticks / 2) : noon;
        var afternoonStart = overnight ? split.AddMinutes(row.BreakMinutes) : row.WorkDate.Date.AddHours(13.5);

        if ((mask & 1) != 0 && start < split) expectedStart = end < afternoonStart ? end : afternoonStart;
        if ((mask & 2) != 0 && end > split) expectedEnd = start > split ? start : split;
        if (mask == 3) expectedStart = end;
        if (expectedStart != start || expectedEnd != end)
            DescribeLeave(row, leaves, mask);

        if (expectedStart >= expectedEnd)
        {
            row.StatusCode = "ON_LEAVE";
            // Preserve all original timestamps for the device view; do not create attendance punches.
            return;
        }

        row.ExpectedStartAt = expectedStart;
        row.ExpectedEndAt = expectedEnd;
        var authorizedEnd = expectedEnd;
        if (authorizedOvertime.Count > 0)
        {
            var overtimeEnd = authorizedOvertime.Max(x => x.End);
            if (overtimeEnd > authorizedEnd) authorizedEnd = overtimeEnd;
        }
        if (row.IsFlexible)
        {
            var actualStart = row.CheckIn > expectedStart ? row.CheckIn : expectedStart;
            var actualEnd = row.CheckOut < expectedEnd ? row.CheckOut : expectedEnd;
            if (row.CheckIn.HasValue && row.CheckOut.HasValue)
            {
                var baseMinutes = actualEnd > actualStart ? (int)(actualEnd.Value - actualStart.Value).TotalMinutes : 0;
                var allAuthorizedMinutes = AuthorizedMinutes(row.CheckIn.Value, row.CheckOut.Value,
                    [new(expectedStart, expectedEnd), .. authorizedOvertime]);
                var overtimeMinutes = Math.Max(0, allAuthorizedMinutes - baseMinutes);
                row.WorkedMinutes = Math.Max(0, baseMinutes - (row.ApprovedLeave ? 0 : row.BreakMinutes)) + overtimeMinutes;
            }
            var target = row.ApprovedLeave ? (int)Math.Ceiling(row.RequiredMinutes / 2d) : row.RequiredMinutes;
            row.IsProvisional = now < authorizedEnd;
            row.StatusCode = row.IsProvisional ? "IN_PROGRESS" :
                !row.CheckIn.HasValue || !row.CheckOut.HasValue ? "MISSING_CHECK" :
                row.WorkedMinutes >= target ? "ON_TIME" : "INSUFFICIENT_HOURS";
            return;
        }
        row.LateMinutes = row.CheckIn.HasValue
            ? Math.Max(0, (int)(row.CheckIn.Value - expectedStart.AddMinutes(row.GraceMinutes)).TotalMinutes) : 0;
        if (now < authorizedEnd)
        {
            row.IsProvisional = true;
            row.CheckOut = null;
            row.StatusCode = "IN_PROGRESS";
            return;
        }

        // LastSeen remains raw; it also restores checkout if this projection is refreshed after shift end.
        row.CheckOut = !row.HasExplicitPunches && row.EventCount > 1 && row.LastSeen.HasValue && row.LastSeen != row.CheckIn ? row.LastSeen : row.CheckOut;
        if (row.CheckIn.HasValue && row.CheckOut.HasValue)
        {
            // Keep the raw punch timestamps for audit, but count only the assigned shift
            // and approved overtime windows. Gaps between those windows are not work time.
            row.WorkedMinutes = AuthorizedMinutes(row.CheckIn.Value, row.CheckOut.Value,
                [new(expectedStart, expectedEnd), .. authorizedOvertime],
                overnight ? null : noon, overnight ? null : row.WorkDate.Date.AddHours(13.5));
        }
        row.EarlyMinutes = row.CheckOut.HasValue ? Math.Max(0, (int)(expectedEnd - row.CheckOut.Value).TotalMinutes) : 0;
        if (!row.CheckIn.HasValue || !row.CheckOut.HasValue) row.StatusCode = "MISSING_CHECK";
        else if (row.LateMinutes > 0 && row.EarlyMinutes > 0) row.StatusCode = "LATE_EARLY";
        else if (row.LateMinutes > 0) row.StatusCode = "LATE";
        else if (row.EarlyMinutes > 0) row.StatusCode = "EARLY";
        else row.StatusCode = "ON_TIME";
    }

    private static void CalculateOvertimeOnly(AttendanceRecordModel row, IReadOnlyList<WorkInterval> authorized, DateTime now)
    {
        var expectedStart = authorized.Min(x => x.Start);
        var expectedEnd = authorized.Max(x => x.End);
        row.ShiftName = "Tăng ca đã duyệt";
        row.ExpectedStartAt = expectedStart;
        row.ExpectedEndAt = expectedEnd;
        if (now < expectedEnd)
        {
            row.IsProvisional = true;
            row.CheckOut = null;
            row.StatusCode = "IN_PROGRESS";
            return;
        }
        if (row.CheckIn.HasValue && row.CheckOut.HasValue)
        {
            row.WorkedMinutes = AuthorizedMinutes(row.CheckIn.Value, row.CheckOut.Value, authorized,
                row.WorkDate.Date.AddHours(12), row.WorkDate.Date.AddHours(13.5));
        }
        row.StatusCode = row.CheckIn.HasValue && row.CheckOut.HasValue ? "ON_TIME" : "MISSING_CHECK";
    }

    private static List<WorkInterval> ResolveOvertimeWindows(IReadOnlyList<ApprovedOvertimeModel> overtime)
    {
        var windows = new List<WorkInterval>();
        foreach (var item in overtime)
        {
            if (TryResolveOvertimeWindow(item.WorkDate, item.StartAt, item.EndAt, item.TimeRange, out var start, out var end))
                windows.Add(new WorkInterval(start, end));
        }
        return windows;
    }

    public static bool TryResolveOvertimeWindow(DateTime workDate, DateTime? startAt, DateTime? endAt,
        string timeRange, out DateTime start, out DateTime end)
    {
        start = end = default;
        if (startAt.HasValue && endAt.HasValue && startAt.Value.Date == workDate.Date)
        {
            start = startAt.Value;
            end = endAt.Value <= start ? endAt.Value.AddDays(1) : endAt.Value;
            return end > start && end - start <= TimeSpan.FromDays(1);
        }

        var parts = (timeRange ?? string.Empty).Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !TimeSpan.TryParse(parts[0], out var startTime) || !TimeSpan.TryParse(parts[1], out var endTime) ||
            startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1) || endTime < TimeSpan.Zero || endTime >= TimeSpan.FromDays(1))
            return false;
        start = workDate.Date.Add(startTime);
        end = workDate.Date.Add(endTime);
        if (end <= start) end = end.AddDays(1);
        return true;
    }

    private static int AuthorizedMinutes(DateTime actualStart, DateTime actualEnd,
        IReadOnlyList<WorkInterval> authorized, DateTime? lunchStart = null, DateTime? lunchEnd = null)
    {
        if (actualEnd <= actualStart) return 0;
        var intervals = authorized
            .Select(x => new WorkInterval(x.Start > actualStart ? x.Start : actualStart,
                x.End < actualEnd ? x.End : actualEnd))
            .Where(x => x.End > x.Start)
            .OrderBy(x => x.Start)
            .ThenBy(x => x.End)
            .ToList();
        if (intervals.Count == 0) return 0;

        var merged = new List<WorkInterval>();
        foreach (var interval in intervals)
        {
            if (merged.Count == 0 || interval.Start > merged[^1].End)
            {
                merged.Add(interval);
                continue;
            }
            if (interval.End > merged[^1].End)
                merged[^1] = new WorkInterval(merged[^1].Start, interval.End);
        }

        var total = 0;
        foreach (var interval in merged)
        {
            var minutes = (int)(interval.End - interval.Start).TotalMinutes;
            if (lunchStart.HasValue && lunchEnd.HasValue)
            {
                var overlapStart = interval.Start > lunchStart ? interval.Start : lunchStart.Value;
                var overlapEnd = interval.End < lunchEnd ? interval.End : lunchEnd.Value;
                if (overlapEnd > overlapStart) minutes -= (int)(overlapEnd - overlapStart).TotalMinutes;
            }
            total += Math.Max(0, minutes);
        }
        return total;
    }

    private readonly record struct WorkInterval(DateTime Start, DateTime End);

    private static void DescribeLeave(AttendanceRecordModel row, IReadOnlyList<LeaveRequestModel> leaves, int mask)
    {
        if (mask == 0) return;
        row.ApprovedLeave = true;
        row.LeaveSession = mask == 3 ? "Cả ngày" : mask == 1 ? "Buổi sáng" : "Buổi chiều";
        row.LeaveDescription = string.Join("; ", leaves.Select(x => $"{x.LeaveType} ({x.RequestCode})").Distinct());
    }

    private static AttendanceRecordModel Copy(AttendanceRecordModel row) => new()
    {
        UserId = row.UserId, PersonId = row.PersonId, EmployeeCode = row.EmployeeCode, DisplayName = row.DisplayName,
        JobTitle = row.JobTitle, DepartmentName = row.DepartmentName, WorkDate = row.WorkDate,
        ShiftName = row.ShiftName, ScheduledStart = row.ScheduledStart, ScheduledEnd = row.ScheduledEnd,
        GraceMinutes = row.GraceMinutes, BreakMinutes = row.BreakMinutes,
        CheckIn = row.CheckIn, CheckOut = row.CheckOut, LastSeen = row.LastSeen, EventCount = row.EventCount, Source = row.Source,
        HasExplicitPunches = row.HasExplicitPunches, IsFlexible = row.IsFlexible, RequiredMinutes = row.RequiredMinutes
    };
}
