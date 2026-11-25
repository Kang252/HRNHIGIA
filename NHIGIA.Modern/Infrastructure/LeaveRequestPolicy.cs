using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class LeaveRequestPolicy
{
    public static bool CanCreateFor(HrmUserAccountModel actor, HrmUserAccountModel target) =>
        actor != null && target?.IsActive == true &&
        (actor.Id == target.Id || HrmRoles.CanPublishCompanyWide(actor.RoleCode) ||
         (actor.RoleCode == HrmRoles.Manager && actor.DepartmentId.HasValue && actor.DepartmentId == target.DepartmentId));

    public static bool CanApprove(HrmUserAccountModel actor, LeaveRequestModel request) =>
        actor != null && request != null && actor.Id != request.UserId && (request.StatusCode switch
        {
            "PENDING_MANAGER" => actor.RoleCode == HrmRoles.Admin ||
                actor.RoleCode == HrmRoles.Manager && request.RoleCode == HrmRoles.Employee &&
                actor.DepartmentId.HasValue && actor.DepartmentId == request.DepartmentId,
            "PENDING_HR" => actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director,
            _ => false
        });

    public static decimal RequestedDays(CreateLeaveRequest request, IEnumerable<ScheduleModel> schedules, int userId)
    {
        if (request == null || request.StartDate == default || request.EndDate < request.StartDate) return 0;
        var assigned = (schedules ?? []).Where(x => x.UserId == userId && x.StatusCode == "ACTIVE").ToList();
        var hasAssignedSchedule = assigned.Any(x => x.EffectiveFrom.Date <= request.EndDate.Date &&
            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= request.StartDate.Date));
        var leaveMask = AttendanceLeavePolicy.SessionMask(request.SessionCode);
        decimal total = 0;
        for (var day = request.StartDate.Date; day <= request.EndDate.Date; day = day.AddDays(1))
        {
            var shift = assigned.Where(x => x.EffectiveFrom.Date <= day &&
                    (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= day) &&
                    (x.WorkDaysMask & (1 << (int)day.DayOfWeek)) != 0)
                .OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Id).FirstOrDefault();
            if (hasAssignedSchedule && shift == null) continue;
            if (!hasAssignedSchedule && day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            var workMask = shift == null ? 3 :
                shift.EndTime > shift.StartTime && shift.EndTime <= TimeSpan.FromHours(12) ? 1 :
                shift.EndTime > shift.StartTime && shift.StartTime >= TimeSpan.FromHours(13.5) ? 2 : 3;
            var mask = leaveMask & workMask;
            total += mask == 3 ? 1m : mask == 0 ? 0m : .5m;
        }
        return total;
    }

    public static string InitialStatus(CreateLeaveRequest request, IEnumerable<ScheduleModel> schedules, int userId, string ownerRole)
    {
        var days = RequestedDays(request, schedules, userId);
        return ownerRole == HrmRoles.Employee && days <= 3m ? "PENDING_MANAGER" : "PENDING_HR";
    }

    /// <summary>
    /// The leave form is opened as "Nghỉ phép"; what payroll and the annual balance need is the kind chosen inside it.
    /// Older rows keep that choice only in the first lines of the reason.
    /// </summary>
    public static string NormalizeLeaveType(string leaveType, string subtype, string reason = null)
    {
        var type = leaveType?.Trim();
        if (type != "Nghỉ phép") return type;
        if (string.IsNullOrWhiteSpace(subtype)) subtype = ReasonSubtype(reason);
        return subtype?.Trim().ToLowerInvariant() switch
        {
            { } value when value.Contains("không hưởng lương") || value.Contains("không lương") => "Nghỉ không lương",
            { } value when value.Contains("ốm") => "Nghỉ ốm",
            { } value when value.Contains("thai sản") => "Nghỉ thai sản",
            { } value when value.Contains("việc riêng") => "Nghỉ việc riêng",
            { } value when value.Contains("kết hôn") => "Nghỉ kết hôn",
            { } value when value.Contains("nghỉ bù") => "Nghỉ bù",
            _ => type
        };
    }

    private static string ReasonSubtype(string reason) =>
        (reason ?? "").Split('\n').FirstOrDefault(x => x.TrimStart().StartsWith("Loại: "))?.TrimStart()[6..].Split('|')[0].Trim();

    public static string ApproverLabel(string statusCode) =>
        statusCode == "PENDING_MANAGER" ? "Trưởng phòng" : "HR/Giám đốc";

    /// <summary>Workdays of a stored request, or null when its session is not a leave session.</summary>
    public static decimal? RequestedDays(LeaveRequestModel request, IEnumerable<ScheduleModel> schedules) =>
        AttendanceLeavePolicy.SessionMask(request.SessionCode) == 0
            ? null
            : RequestedDays(new CreateLeaveRequest
            {
                StartDate = request.StartDate, EndDate = request.EndDate, SessionCode = request.SessionCode
            }, schedules, request.UserId);

    public static string Validate(CreateLeaveRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LeaveType) || request.LeaveType.Length > 80)
            return "Vui lòng chọn loại yêu cầu hợp lệ (tối đa 80 ký tự).";
        if (request.StartDate == default || request.EndDate == default || request.EndDate.Date < request.StartDate.Date ||
            (request.EndDate.Date - request.StartDate.Date).TotalDays > 366)
            return "Khoảng thời gian yêu cầu không hợp lệ hoặc vượt quá 366 ngày.";
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 16000)
            return "Vui lòng nhập nội dung yêu cầu, tối đa 16.000 ký tự.";
        if (string.IsNullOrWhiteSpace(request.SessionCode) || request.SessionCode.Length > 30)
            return "Vui lòng chọn thời gian hoặc hình thức hợp lệ (tối đa 30 ký tự).";
        if (AttendanceLeavePolicy.LeaveTypes.Contains(request.LeaveType, StringComparer.OrdinalIgnoreCase) &&
            AttendanceLeavePolicy.SessionMask(request.SessionCode) == 0)
            return "Vui lòng chọn nghỉ cả ngày, buổi sáng hoặc buổi chiều.";
        if (request.HandoverTo?.Length > 150) return "Thông tin bàn giao không được vượt quá 150 ký tự.";
        if (request.EmployeeId.HasValue && request.EmployeeId <= 0) return "Nhân viên được chọn không hợp lệ.";
        return null;
    }

    public static bool UsesAnnualAllowance(LeaveRequestModel request)
    {
        var type = request.LeaveType?.Trim();
        if (type is "Phép năm" or "Nghỉ phép năm") return true;
        if (type != "Nghỉ phép") return false;
        // Older forms stored the selected subtype inside the formatted reason.
        var subtype = ReasonSubtype(request.Reason);
        return string.IsNullOrEmpty(subtype) || subtype is "Nghỉ phép" or "Phép năm" or "Nghỉ phép năm";
    }

    /// <summary>Business checks that need the employee's schedule, balance and other requests.</summary>
    public static string ValidateAgainst(CreateLeaveRequest request, int userId, decimal annualAllowance,
        IReadOnlyCollection<LeaveRequestModel> openRequests, IReadOnlyCollection<ScheduleModel> schedules)
    {
        var mask = AttendanceLeavePolicy.SessionMask(request.SessionCode);
        if (!AttendanceLeavePolicy.LeaveTypes.Contains(request.LeaveType, StringComparer.OrdinalIgnoreCase) || mask == 0) return null;
        if (RequestedDays(request, schedules, userId) <= 0)
            return "Khoảng thời gian đã chọn không có ngày làm việc theo lịch của nhân viên.";
        var overlap = openRequests.FirstOrDefault(x => x.StartDate.Date <= request.EndDate.Date && x.EndDate.Date >= request.StartDate.Date &&
            (AttendanceLeavePolicy.SessionMask(x.SessionCode) & mask) != 0 &&
            AttendanceLeavePolicy.LeaveTypes.Contains(x.LeaveType, StringComparer.OrdinalIgnoreCase));
        if (overlap != null)
            return $"Đã có đơn {overlap.RequestCode} trùng thời gian ({overlap.StartDate:dd/MM/yyyy}–{overlap.EndDate:dd/MM/yyyy}).";
        if (!UsesAnnualAllowance(new LeaveRequestModel { LeaveType = request.LeaveType, Reason = request.Reason })) return null;
        for (var year = request.StartDate.Year; year <= request.EndDate.Year; year++)
        {
            var from = new DateTime(year, 1, 1);
            var requested = RequestedDays(new CreateLeaveRequest
            {
                StartDate = request.StartDate.Date < from ? from : request.StartDate.Date,
                EndDate = request.EndDate.Date > from.AddYears(1).AddDays(-1) ? from.AddYears(1).AddDays(-1) : request.EndDate.Date,
                SessionCode = request.SessionCode
            }, schedules, userId);
            // Requests still waiting hold their days, otherwise several of them could each pass the check.
            var committed = UsedAnnualDays(openRequests.Select(x => new LeaveRequestModel
            {
                UserId = x.UserId, LeaveType = x.LeaveType, Reason = x.Reason, StartDate = x.StartDate, EndDate = x.EndDate,
                SessionCode = x.SessionCode, StatusCode = "APPROVED"
            }), schedules, year);
            var remaining = Math.Max(0, annualAllowance - committed);
            if (requested > remaining)
                return $"Quỹ phép năm {year} còn {Days(remaining)} ngày (đã trừ đơn đang chờ), không đủ cho {Days(requested)} ngày công. Hãy giảm số ngày hoặc chọn nghỉ không hưởng lương.";
        }
        return null;
    }

    private static string Days(decimal value) => value.ToString("0.#", VietnamFormat.Numbers);

    public static decimal UsedAnnualDays(IEnumerable<LeaveRequestModel> requests, IEnumerable<ScheduleModel> schedules, int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = from.AddYears(1).AddDays(-1);
        var masks = new Dictionary<(int UserId, DateTime Day), int>();
        foreach (var request in requests.Where(x => x.StatusCode == "APPROVED" && UsesAnnualAllowance(x)))
        {
            var start = request.StartDate.Date < from ? from : request.StartDate.Date;
            var end = request.EndDate.Date > to ? to : request.EndDate.Date;
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                var key = (request.UserId, day);
                masks[key] = masks.GetValueOrDefault(key) | AttendanceLeavePolicy.SessionMask(request.SessionCode);
            }
        }
        var active = schedules.Where(x => x.StatusCode == "ACTIVE").ToLookup(x => x.UserId);
        decimal total = 0;
        foreach (var (key, leaveMask) in masks)
        {
            var shift = active[key.UserId].Where(x => x.EffectiveFrom.Date <= key.Day &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= key.Day) &&
                (x.WorkDaysMask & (1 << (int)key.Day.DayOfWeek)) != 0)
                .OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Id).FirstOrDefault();
            // With no assigned schedules retain calendar-day accounting for legacy records.
            var workMask = shift == null ? (active[key.UserId].Any() ? 0 : 3) :
                shift.EndTime > shift.StartTime && shift.EndTime <= TimeSpan.FromHours(12) ? 1 :
                shift.EndTime > shift.StartTime && shift.StartTime >= TimeSpan.FromHours(13.5) ? 2 : 3;
            var mask = leaveMask & workMask;
            total += mask == 3 ? 1m : mask == 0 ? 0m : .5m;
        }
        return total;
    }
}
