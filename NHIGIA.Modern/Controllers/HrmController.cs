using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

public sealed class HrmController : BaseController
{
    private readonly ILogger<HrmController> _logger;

    public HrmController(HrmDataStore store, HrmUserAccessor userAccessor, ILogger<HrmController> logger)
        : base(store, userAccessor) => _logger = logger;

    private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    private IActionResult Execute(Func<object> action)
    {
        try { return Json(ApiResponse.Ok(action())); }
        catch (Exception exception)
        {
            _logger.LogError(exception, "HRM request failed");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpGet]
    public IActionResult Dashboard() => Execute(() => Store.GetDashboard(CurrentHrmUser));

    [HttpGet]
    public IActionResult Users() => Execute(() => Store.GetVisibleUsers(CurrentHrmUser).Select(x => new
    {
        x.Id, x.Username, x.DisplayName, x.RoleCode, x.RoleLabel, x.DepartmentId, x.DepartmentName
    }));

    [HttpGet]
    public IActionResult ShiftTemplates() => Execute(() => Store.GetShiftTemplates());

    [HttpGet]
    public IActionResult Schedules() => Execute(() => Store.GetSchedules(CurrentHrmUser));

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult SaveSchedule(SaveScheduleRequest request) => Execute(() =>
    {
        if (request == null || request.UserId <= 0) throw new InvalidOperationException("Vui lòng chọn nhân viên.");
        if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == request.UserId)) throw new InvalidOperationException("Bạn không có quyền phân lịch cho nhân viên này.");
        if (!TimeSpan.TryParse(request.StartTime, out _) || !TimeSpan.TryParse(request.EndTime, out _)) throw new InvalidOperationException("Giờ bắt đầu hoặc kết thúc không hợp lệ.");
        if (request.EffectiveFrom == default) throw new InvalidOperationException("Vui lòng chọn ngày áp dụng.");
        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value.Date < request.EffectiveFrom.Date) throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");
        request.ShiftName = string.IsNullOrWhiteSpace(request.ShiftName) ? "Ca cá nhân" : request.ShiftName.Trim();
        return new { Id = Store.SaveSchedule(request, CurrentHrmUser, ClientIp) };
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult DeleteSchedule(int id) => Execute(() =>
    {
        if (!Store.DeleteSchedule(id, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Không tìm thấy lịch hoặc bạn không có quyền xóa.");
        return new { Id = id };
    });

    [HttpGet]
    public IActionResult LeaveRequests() => Execute(() => Store.GetLeaveRequests(CurrentHrmUser));

    [HttpGet]
    public IActionResult LeaveStats() => Execute(() => Store.GetLeaveStats(CurrentHrmUser));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateLeave(CreateLeaveRequest request) => Execute(() =>
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LeaveType)) throw new InvalidOperationException("Vui lòng chọn loại nghỉ.");
        if (request.StartDate == default || request.EndDate == default || request.EndDate.Date < request.StartDate.Date) throw new InvalidOperationException("Khoảng ngày nghỉ không hợp lệ.");
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("Vui lòng nhập lý do nghỉ.");
        return Store.CreateLeave(request, CurrentHrmUser, ClientIp);
    });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CancelLeave(int id) => Execute(() =>
    {
        if (!Store.CancelLeave(id, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Chỉ có thể hủy đơn của bạn khi đang chờ duyệt.");
        return new { Id = id };
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult ApproveLeave(ApprovalRequest request) => Execute(() =>
    {
        if (request == null || request.Id <= 0) throw new InvalidOperationException("Đơn nghỉ không hợp lệ.");
        if (!Store.ApproveLeave(request, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Đơn đã được xử lý hoặc không thuộc phạm vi của bạn.");
        return new { request.Id, request.Approve };
    });

    [HttpGet]
    public IActionResult Communications(string keyword = "", string category = "") =>
        Execute(() => Store.GetCommunications(CurrentHrmUser, keyword, category));

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> CreateCommunication(CreateCommunicationRequest request, IFormFile attachment)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) throw new InvalidOperationException("Vui lòng nhập tiêu đề và nội dung.");
            request.ScopeCode = (request.ScopeCode ?? "DEPARTMENT").ToUpperInvariant();
            if (!new[] { "ALL", "DEPARTMENT", "MANAGER" }.Contains(request.ScopeCode)) throw new InvalidOperationException("Phạm vi đăng tin không hợp lệ.");
            if (request.ScopeCode == "ALL" && !HrmRoles.CanPublishCompanyWide(CurrentHrmUser.RoleCode)) throw new InvalidOperationException("Trưởng phòng chỉ được đăng trong phòng ban hoặc nhóm quản lý.");
            request.Category = string.IsNullOrWhiteSpace(request.Category) ? "Thông báo" : request.Category.Trim();
            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Ảnh đính kèm không được vượt quá 5 MB.");
                var extension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => throw new InvalidOperationException("Chỉ hỗ trợ ảnh JPG, PNG, GIF hoặc WebP.")
                };
                await using var stream = new MemoryStream();
                await attachment.CopyToAsync(stream);
                var content = stream.ToArray();
                if (!IsSupportedImage(content, extension)) throw new InvalidOperationException("Nội dung tệp ảnh không hợp lệ.");
                request.AttachmentName = Path.GetFileName(attachment.FileName);
                request.AttachmentContentType = contentType;
                request.AttachmentContent = content;
            }
            return Json(ApiResponse.Ok(Store.CreateCommunication(request, CurrentHrmUser, ClientIp)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot publish internal communication");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpGet]
    public IActionResult CommunicationAttachment(int id)
    {
        var attachment = Store.GetCommunicationAttachment(id, CurrentHrmUser);
        return attachment?.Content == null ? NotFound() : File(attachment.Content, attachment.ContentType ?? "application/octet-stream");
    }

    private static bool IsSupportedImage(byte[] content, string extension)
    {
        if (content == null || content.Length < 12) return false;
        return extension switch
        {
            ".jpg" or ".jpeg" => content[0] == 0xff && content[1] == 0xd8 && content[2] == 0xff,
            ".png" => content.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            ".gif" => System.Text.Encoding.ASCII.GetString(content, 0, 6) is "GIF87a" or "GIF89a",
            ".webp" => System.Text.Encoding.ASCII.GetString(content, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(content, 8, 4) == "WEBP",
            _ => false
        };
    }

    [HttpGet]
    public IActionResult Attendance(DateTime? fromDate, DateTime? toDate) => Execute(() =>
    {
        var to = (toDate ?? DateTime.Today).Date;
        var from = (fromDate ?? to.AddDays(-30)).Date;
        if (to < from || (to - from).TotalDays > 366) throw new InvalidOperationException("Khoảng lọc tối đa là 366 ngày.");
        return Store.GetAttendance(CurrentHrmUser, from, to);
    });

    [HttpGet]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult HanetSettings() => Execute(() => Store.GetHanetSettings(false));

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult SaveHanetSettings(HanetSettingsModel settings) => Execute(() =>
    {
        if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps || !apiUri.Host.EndsWith("hanet.ai", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("API URL phải là địa chỉ HTTPS thuộc hanet.ai.");
        if (!Uri.TryCreate(settings.OAuthTokenUrl, UriKind.Absolute, out var tokenUri) || tokenUri.Scheme != Uri.UriSchemeHttps || !tokenUri.Host.EndsWith("hanet.com", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("OAuth URL phải là địa chỉ HTTPS thuộc hanet.com.");
        Store.SaveHanetSettings(settings, CurrentHrmUser, ClientIp);
        return Store.GetHanetSettings(false);
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult SaveHanetPersonMap(HanetPersonMapRequest request) => Execute(() =>
    {
        if (request == null || request.UserId <= 0 || (string.IsNullOrWhiteSpace(request.PersonId) && string.IsNullOrWhiteSpace(request.AliasId))) throw new InvalidOperationException("Cần chọn nhân viên và nhập Person ID hoặc Alias ID.");
        if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == request.UserId)) throw new InvalidOperationException("Nhân viên không hợp lệ.");
        Store.SaveHanetPersonMap(request, CurrentHrmUser, ClientIp);
        return new { request.UserId };
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public async Task<IActionResult> TestHanet()
    {
        try
        {
            var settings = Store.GetHanetSettings(true);
            if (string.IsNullOrWhiteSpace(settings.AccessToken)) throw new InvalidOperationException("Chưa có access token HANET.");
            var endpoint = settings.ApiBaseUrl.TrimEnd('/') + "/place/getPlaces";
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            using var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string> { ["token"] = settings.AccessToken }));
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
            var code = json["returnCode"]?.ToString() ?? json["code"]?.ToString();
            var ok = response.IsSuccessStatusCode && (code == "1" || code == "200" || string.IsNullOrEmpty(code));
            var message = ok ? "Kết nối HANET thành công." : "HANET từ chối yêu cầu: " + (json["returnMessage"]?.ToString() ?? json["message"]?.ToString() ?? response.ReasonPhrase);
            Store.UpdateHanetSyncStatus(ok ? "SUCCESS" : "FAILED", message);
            return ok ? Json(ApiResponse.Ok(json["data"], message)) : BadRequest(ApiResponse.Fail(message));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "HANET connection test failed");
            Store.UpdateHanetSyncStatus("FAILED", exception.Message);
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpGet]
    public FileContentResult AttendanceCsv(DateTime? fromDate, DateTime? toDate)
    {
        var to = (toDate ?? DateTime.Today).Date;
        var from = (fromDate ?? to.AddDays(-30)).Date;
        var rows = Store.GetAttendance(CurrentHrmUser, from, to);
        var csv = new StringBuilder("Ngay,Nhan vien,Phong ban,Ca,Check-in,Check-out,Phut cong,Di muon,Ve som,Trang thai\r\n");
        foreach (var row in rows)
        {
            static string Q(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
            csv.AppendLine(string.Join(",", Q(row.WorkDate.ToString("dd/MM/yyyy")), Q(row.DisplayName), Q(row.DepartmentName), Q(row.ShiftName), Q(row.CheckIn?.ToString("HH:mm")), Q(row.CheckOut?.ToString("HH:mm")), row.WorkedMinutes, row.LateMinutes, row.EarlyMinutes, Q(row.StatusCode)));
        }
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", "cham-cong.csv");
    }
}
