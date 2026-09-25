using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class LeaveRequestPolicy
{
    public static bool CanCreateFor(HrmUserAccountModel actor, HrmUserAccountModel target) =>
        actor != null && target?.IsActive == true &&
        (actor.Id == target.Id || HrmRoles.CanPublishCompanyWide(actor.RoleCode) ||
         (actor.RoleCode == HrmRoles.Manager && actor.DepartmentId.HasValue && actor.DepartmentId == target.DepartmentId));

    public static bool CanApprove(HrmUserAccountModel actor, LeaveRequestModel request) =>
        actor != null && request != null && actor.Id != request.UserId &&
        request.StatusCode is "PENDING_MANAGER" or "PENDING_HR" &&
        (HrmRoles.CanPublishCompanyWide(actor.RoleCode) ||
         (actor.RoleCode == HrmRoles.Manager && request.StatusCode == "PENDING_MANAGER" &&
          request.RoleCode == HrmRoles.Employee && actor.DepartmentId.HasValue && actor.DepartmentId == request.DepartmentId));

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
        var subtype = (request.Reason ?? "").Split('\n').FirstOrDefault(x => x.TrimStart().StartsWith("Loại: "))?
            .Trim()[6..].Split('|')[0].Trim();
        return string.IsNullOrEmpty(subtype) || subtype is "Nghỉ phép" or "Phép năm" or "Nghỉ phép năm";
    }

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
