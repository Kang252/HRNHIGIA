using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class RemoteAttendancePolicy
{
    public static bool CanReview(HrmUserAccountModel actor, int ownerId, int? departmentId, string ownerRole) =>
        actor != null && actor.IsActive && actor.Id != ownerId &&
        (actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director ||
         actor.RoleCode == HrmRoles.Manager && ownerRole == HrmRoles.Employee &&
         actor.DepartmentId.HasValue && actor.DepartmentId == departmentId);

    public static void ValidateCoordinates(double? latitude, double? longitude)
    {
        if (latitude.HasValue != longitude.HasValue ||
            latitude.HasValue && (!double.IsFinite(latitude.Value) || !double.IsFinite(longitude.Value) ||
                Math.Abs(latitude.Value) > 90 || Math.Abs(longitude.Value) > 180))
            throw new InvalidOperationException("Tọa độ không hợp lệ.");
    }

    public static void ValidatePlan(RemoteWorkPlan plan)
    {
        if (plan.Mode is not ("FIELD" or "HOME")) throw new InvalidOperationException("Loại lịch không hợp lệ.");
        if (plan.FromDate == default || plan.ToDate < plan.FromDate || (plan.ToDate - plan.FromDate).TotalDays > 92)
            throw new InvalidOperationException("Chọn khoảng đăng ký tối đa 93 ngày.");
        if (plan.WorkDaysMask < 1 || plan.WorkDaysMask > 127) throw new InvalidOperationException("Chọn ngày làm việc.");
        if (plan.WindowStart < TimeSpan.Zero || plan.WindowEnd >= TimeSpan.FromDays(1) || plan.WindowEnd <= plan.WindowStart)
            throw new InvalidOperationException("Khung giờ phải bắt đầu và kết thúc trong cùng ngày.");
        var minutes = (plan.WindowEnd - plan.WindowStart).TotalMinutes;
        if (plan.BreakMinutes < 0 || plan.BreakMinutes >= minutes || plan.RequiredMinutes < 1 || plan.RequiredMinutes > minutes - plan.BreakMinutes)
            throw new InvalidOperationException("Số phút làm việc/nghỉ không phù hợp khung giờ.");
        ValidateCoordinates(plan.Latitude, plan.Longitude);
        if (plan.Mode == "HOME" && !plan.Latitude.HasValue) throw new InvalidOperationException("Lịch làm tại nhà cần tọa độ đăng ký.");
        if (plan.RadiusMeters is < 50 or > 2000) throw new InvalidOperationException("Bán kính từ 50 đến 2.000 m.");
        if (string.IsNullOrWhiteSpace(plan.PlaceName) || plan.PlaceName.Length > 200 || (plan.Note?.Length ?? 0) > 2000)
            throw new InvalidOperationException("Nhập địa điểm/tuyến tối đa 200 ký tự và ghi chú tối đa 2.000 ký tự.");
    }

    public static double Distance(double latitude, double longitude, double targetLatitude, double targetLongitude)
    {
        const double radians = Math.PI / 180;
        var a = Math.Pow(Math.Sin((targetLatitude - latitude) * radians / 2), 2) +
            Math.Cos(latitude * radians) * Math.Cos(targetLatitude * radians) *
            Math.Pow(Math.Sin((targetLongitude - longitude) * radians / 2), 2);
        return 6371000 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }

    public static (DateTime CheckTime, double? Distance, string Reason) Evaluate(RemoteWorkPlan plan, RemotePunchRequest punch, DateTimeOffset now)
    {
        if (punch.ClientId == Guid.Empty || punch.Kind is not ("IN" or "OUT" or "VISIT"))
            throw new InvalidOperationException("Lượt chấm không hợp lệ.");
        if (plan.Mode == "HOME" && punch.Kind == "VISIT") throw new InvalidOperationException("Lịch tại nhà không có lượt ghé khách hàng.");
        if (punch.CapturedAt == default || punch.CapturedAt > now.AddMinutes(5) || punch.CapturedAt < now.AddDays(-7))
            throw new InvalidOperationException("Giờ thiết bị không hợp lệ hoặc bản ghi đã quá 7 ngày.");
        ValidateCoordinates(punch.Latitude, punch.Longitude);
        if (punch.AccuracyMeters.HasValue && (!double.IsFinite(punch.AccuracyMeters.Value) || punch.AccuracyMeters < 0))
            throw new InvalidOperationException("Độ chính xác GPS không hợp lệ.");
        if (string.IsNullOrWhiteSpace(punch.PlaceName) || punch.PlaceName.Length > 200 || (punch.Note?.Length ?? 0) > 2000)
            throw new InvalidOperationException("Nhập tên điểm thực tế tối đa 200 ký tự; ghi chú tối đa 2.000 ký tự.");
        // Client timestamps are evidence only. Delayed/offline records require a human decision.
        var delayed = punch.WasOffline || Math.Abs((now - punch.CapturedAt).TotalSeconds) > 120;
        var checkTime = (delayed ? punch.CapturedAt : now).ToOffset(TimeSpan.FromHours(7)).DateTime;
        var issues = new List<string>();
        if (plan.StatusCode != "APPROVED") issues.Add("Lịch chưa được duyệt hoặc đã hủy");
        if (checkTime.Date < plan.FromDate.Date || checkTime.Date > plan.ToDate.Date || (plan.WorkDaysMask & (1 << (int)checkTime.DayOfWeek)) == 0)
            issues.Add("Ngoài ngày đăng ký");
        if (checkTime.TimeOfDay < plan.WindowStart || checkTime.TimeOfDay > plan.WindowEnd)
            issues.Add("Ngoài khung giờ đăng ký");
        if (delayed) issues.Add("Đồng bộ trễ/offline; cần xác minh thời điểm thiết bị");
        double? distance = null;
        if (!punch.Latitude.HasValue || !punch.AccuracyMeters.HasValue) issues.Add("Thiếu vị trí hoặc độ chính xác GPS");
        else
        {
            if (punch.AccuracyMeters > 100) issues.Add("GPS chưa đủ chính xác");
            if (plan.Latitude.HasValue)
            {
                distance = Distance(punch.Latitude.Value, punch.Longitude.Value, plan.Latitude.Value, plan.Longitude.Value);
                if (distance + punch.AccuracyMeters > plan.RadiusMeters) issues.Add("Ngoài vùng hoặc GPS giao ranh giới vùng đăng ký");
            }
        }
        if (issues.Count > 0 && string.IsNullOrWhiteSpace(punch.Note))
            throw new InvalidOperationException("Vui lòng bổ sung ghi chú để quản lý xác minh: " + string.Join("; ", issues));
        return (checkTime, distance, string.Join("; ", issues));
    }
}
