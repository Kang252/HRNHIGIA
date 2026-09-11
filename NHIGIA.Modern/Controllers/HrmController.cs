using System.Text;
using System.Text.Json.Nodes;
using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

public sealed class HrmController : BaseController
{
    private readonly ILogger<HrmController> _logger;
    private readonly WorkItemStore _workStore;

    public HrmController(HrmDataStore store, HrmUserAccessor userAccessor, ILogger<HrmController> logger, WorkItemStore workStore)
        : base(store, userAccessor)
    {
        _logger = logger;
        _workStore = workStore;
    }

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
        x.Id, x.Username, x.EmployeeCode, x.DisplayName, x.RoleCode, x.RoleLabel, x.DepartmentId, x.DepartmentName
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
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> CreateLeave(CreateLeaveRequest request, IFormFile attachment)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LeaveType)) throw new InvalidOperationException("Vui lòng chọn loại nghỉ.");
            if (request.StartDate == default || request.EndDate == default || request.EndDate.Date < request.StartDate.Date) throw new InvalidOperationException("Khoảng ngày nghỉ không hợp lệ.");
            if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("Vui lòng nhập lý do nghỉ.");
            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > 10 * 1024 * 1024) throw new InvalidOperationException("Tệp đính kèm không được vượt quá 10 MB.");
                var safeName = Path.GetFileName(attachment.FileName);
                if (safeName.Length > 255) throw new InvalidOperationException("Tên tệp đính kèm không được vượt quá 255 ký tự.");
                var extension = Path.GetExtension(safeName).ToLowerInvariant();
                request.AttachmentContentType = extension switch
                {
                    ".pdf" => "application/pdf", ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png",
                    ".doc" => "application/msword", ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => throw new InvalidOperationException("Chỉ hỗ trợ tệp PDF, JPG, PNG, DOC hoặc DOCX.")
                };
                await using var stream = new MemoryStream();
                await attachment.CopyToAsync(stream);
                request.AttachmentContent = stream.ToArray();
                if (!IsSupportedLeaveAttachment(request.AttachmentContent, extension)) throw new InvalidOperationException("Nội dung tệp đính kèm không hợp lệ.");
                request.AttachmentName = safeName;
            }
            return Json(ApiResponse.Ok(Store.CreateLeave(request, CurrentHrmUser, ClientIp)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot create leave request");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpGet]
    public IActionResult LeaveAttachment(int id)
    {
        var attachment = Store.GetLeaveAttachment(id, CurrentHrmUser);
        return attachment?.Content == null
            ? NotFound()
            : File(attachment.Content, attachment.ContentType ?? "application/octet-stream", attachment.FileName ?? $"dinh-kem-{id}");
    }

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

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult ApproveAllLeaves(string note) => Execute(() =>
    {
        var count = Store.ApproveAllLeaves(CurrentHrmUser, note, ClientIp);
        return new { Count = count };
    });

    [HttpGet]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult ApprovalInbox() => Execute(() =>
    {
        var managerStage = CurrentHrmUser.RoleCode == HrmRoles.Manager;
        var leaves = Store.GetLeaveRequests(CurrentHrmUser)
            .Where(item => managerStage ? item.StatusCode == "PENDING_MANAGER" : item.StatusCode is "PENDING_MANAGER" or "PENDING_HR")
            .Select(item => new
            {
                Source = "leave", Kind = "leave", item.Id, Code = item.RequestCode,
                RequestType = "Nghỉ phép", Title = item.LeaveType, EmployeeName = item.DisplayName,
                item.DepartmentName, StartDate = (DateTime?)item.StartDate, EndDate = (DateTime?)item.EndDate, DueDate = (DateTime?)null,
                Description = item.Reason, item.StatusCode, item.CreatedAt
            });
        var work = _workStore.GetPendingApprovals(CurrentHrmUser).Select(item => new
        {
            Source = "work", item.Kind, item.Id, Code = item.RecordCode,
            RequestType = WorkKindLabel(item.Kind), item.Title,
            EmployeeName = item.Kind is "meeting" or "vehicle" ? item.ParticipantNames : item.EmployeeName,
            item.DepartmentName, StartDate = item.StartAt, EndDate = item.EndAt, item.DueDate,
            item.Description, StatusCode = item.Status, item.CreatedAt
        });
        return leaves.Concat(work).OrderBy(item => item.CreatedAt).ToList();
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult DecideApproval(ApprovalInboxRequest request) => Execute(() =>
    {
        if (request == null || request.Id <= 0) throw new InvalidOperationException("Yêu cầu không hợp lệ.");
        if (!request.Approve && string.IsNullOrWhiteSpace(request.Note)) throw new InvalidOperationException("Vui lòng nhập lý do từ chối.");
        if (request.Source == "leave")
        {
            if (!Store.ApproveLeave(new ApprovalRequest { Id = request.Id, Approve = request.Approve, Note = request.Note }, CurrentHrmUser, ClientIp))
                throw new InvalidOperationException("Đơn đã được xử lý hoặc không thuộc phạm vi của bạn.");
        }
        else
        {
            var item = _workStore.GetPendingApprovals(CurrentHrmUser).FirstOrDefault(x => x.Id == request.Id && x.Kind == request.Kind)
                ?? throw new InvalidOperationException("Yêu cầu đã được xử lý hoặc không thuộc phạm vi của bạn.");
            var changed = item.Kind == "payroll"
                ? _workStore.TransitionPayroll(item.Id, "PENDING_APPROVAL", request.Approve ? "APPROVED" : "REJECTED", request.Approve ? "APPROVE" : "REJECT", request.Note, CurrentHrmUser.Id, null, ClientIp)
                : _workStore.TransitionBooking(item.Id, item.Kind, "PENDING", request.Approve ? "APPROVED" : "REJECTED", request.Note, CurrentHrmUser.Id, ClientIp);
            if (!changed) throw new InvalidOperationException("Yêu cầu vừa được người khác xử lý.");
        }
        return new { request.Id, request.Approve };
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult ApproveAllRequests(string note) => Execute(() =>
    {
        var count = Store.ApproveAllLeaves(CurrentHrmUser, note, ClientIp);
        foreach (var item in _workStore.GetPendingApprovals(CurrentHrmUser))
        {
            var changed = item.Kind == "payroll"
                ? _workStore.TransitionPayroll(item.Id, "PENDING_APPROVAL", "APPROVED", "APPROVE_ALL", note, CurrentHrmUser.Id, null, ClientIp)
                : _workStore.TransitionBooking(item.Id, item.Kind, "PENDING", "APPROVED", note, CurrentHrmUser.Id, ClientIp);
            if (changed) count++;
        }
        return new { Count = count };
    });

    private static string WorkKindLabel(string kind) => kind switch
    {
        "overtime" => "Tăng ca", "resignation" => "Nghỉ việc", "vehicle" => "Đặt xe",
        "meeting" => "Đặt phòng họp", "business-trip" => "Công tác", "offboarding" => "Thôi việc",
        "transfer" => "Điều chuyển", "payroll" => "Bảng lương", _ => "Yêu cầu"
    };

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

    private static bool IsSupportedLeaveAttachment(byte[] content, string extension)
    {
        if (content == null || content.Length < 4) return false;
        return extension switch
        {
            ".pdf" => content.Take(4).SequenceEqual("%PDF"u8.ToArray()),
            ".jpg" or ".jpeg" => content[0] == 0xff && content[1] == 0xd8 && content[2] == 0xff,
            ".png" => content.Length >= 8 && content.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            ".doc" => content.Length >= 8 && content.Take(8).SequenceEqual(new byte[] { 0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1 }),
            ".docx" => content[0] == 0x50 && content[1] == 0x4b && content[2] == 0x03 && content[3] == 0x04,
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
    [HrmAuthorize(HrmRoles.Admin)]
    public IActionResult HanetSettings() => Execute(() => Store.GetHanetSettings(false));

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin)]
    public IActionResult SaveHanetSettings(HanetSettingsModel settings) => Execute(() =>
    {
        if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps || !apiUri.Host.EndsWith("hanet.ai", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("API URL phải là địa chỉ HTTPS thuộc hanet.ai.");
        if (!Uri.TryCreate(settings.OAuthTokenUrl, UriKind.Absolute, out var tokenUri) || tokenUri.Scheme != Uri.UriSchemeHttps || !tokenUri.Host.EndsWith("hanet.com", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("OAuth URL phải là địa chỉ HTTPS thuộc hanet.com.");
        Store.SaveHanetSettings(settings, CurrentHrmUser, ClientIp);
        return Store.GetHanetSettings(false);
    });

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin)]
    public IActionResult SaveHanetPersonMap(HanetPersonMapRequest request) => Execute(() =>
    {
        if (request == null || request.UserId <= 0 || (string.IsNullOrWhiteSpace(request.PersonId) && string.IsNullOrWhiteSpace(request.AliasId))) throw new InvalidOperationException("Cần chọn nhân viên và nhập Person ID hoặc Alias ID.");
        if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == request.UserId)) throw new InvalidOperationException("Nhân viên không hợp lệ.");
        Store.SaveHanetPersonMap(request, CurrentHrmUser, ClientIp);
        return new { request.UserId };
    });

    [HttpGet]
    [HrmAuthorize(HrmRoles.Admin)]
    public FileContentResult HanetMappingTemplate()
    {
        var csv = "MaNhanVien,TaiKhoan,PersonID,AliasID,PlaceID\r\nNG001,hradmin,19599634311402042324,HR001,4628\r\n";
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray(), "text/csv; charset=utf-8", "mau-anh-xa-hanet.csv");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin)]
    public async Task<IActionResult> ImportHanetMappings(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse.Fail("Vui lòng chọn file CSV."));
        if (file.Length > 2 * 1024 * 1024) return BadRequest(ApiResponse.Fail("File ánh xạ không được lớn hơn 2 MB."));
        if (!string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase)) return BadRequest(ApiResponse.Fail("Chỉ hỗ trợ file CSV mở được bằng Excel."));

        try
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
            var content = await reader.ReadToEndAsync();
            var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) throw new InvalidOperationException("File chưa có dữ liệu ánh xạ.");
            var separator = lines[0].Count(x => x == ';') > lines[0].Count(x => x == ',') ? ';' : ',';
            var headers = ParseCsvLine(lines[0].TrimStart('\uFEFF'), separator).Select((name, index) => new { Name = NormalizeHeader(name), Index = index }).ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);
            foreach (var required in new[] { "MANHANVIEN", "TAIKHOAN", "PERSONID", "ALIASID", "PLACEID" })
                if (!headers.ContainsKey(required)) throw new InvalidOperationException("Thiếu cột bắt buộc: " + required + ". Hãy tải và sử dụng file mẫu.");

            var users = Store.GetVisibleUsers(CurrentHrmUser);
            var imported = 0;
            var errors = new List<string>();
            for (var lineNumber = 2; lineNumber <= lines.Length; lineNumber++)
            {
                var cells = ParseCsvLine(lines[lineNumber - 1], separator);
                string Cell(string name) => headers[name] < cells.Count ? cells[headers[name]].Trim() : string.Empty;
                var employeeCode = Cell("MANHANVIEN");
                var username = Cell("TAIKHOAN");
                var personId = Cell("PERSONID");
                var aliasId = Cell("ALIASID");
                var placeId = Cell("PLACEID");
                if (string.IsNullOrWhiteSpace(employeeCode) && string.IsNullOrWhiteSpace(username)) { errors.Add($"Dòng {lineNumber}: thiếu mã nhân viên hoặc tài khoản."); continue; }
                if (string.IsNullOrWhiteSpace(personId) && string.IsNullOrWhiteSpace(aliasId)) { errors.Add($"Dòng {lineNumber}: thiếu Person ID hoặc Alias ID."); continue; }
                var matches = users.Where(x => (!string.IsNullOrWhiteSpace(employeeCode) && string.Equals(x.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase)) || (!string.IsNullOrWhiteSpace(username) && string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase))).ToList();
                if (matches.Count != 1) { errors.Add($"Dòng {lineNumber}: không tìm thấy duy nhất một nhân viên phù hợp."); continue; }
                try
                {
                    Store.SaveHanetPersonMap(new HanetPersonMapRequest { UserId = matches[0].Id, PersonId = personId, AliasId = aliasId, PlaceId = placeId }, CurrentHrmUser, ClientIp);
                    imported++;
                }
                catch (Exception exception) { errors.Add($"Dòng {lineNumber}: {exception.Message}"); }
            }
            return Json(ApiResponse.Ok(new { Imported = imported, Errors = errors }, $"Đã ánh xạ {imported} nhân viên; {errors.Count} dòng cần kiểm tra."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "HANET mapping import failed");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    private static List<string> ParseCsvLine(string line, char separator)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < (line ?? string.Empty).Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"') { current.Append('"'); index++; }
                else quoted = !quoted;
            }
            else if (character == separator && !quoted) { values.Add(current.ToString()); current.Clear(); }
            else current.Append(character);
        }
        values.Add(current.ToString());
        return values;
    }

    private static string NormalizeHeader(string value) => new string((value ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin)]
    public async Task<IActionResult> HanetPersons()
    {
        try
        {
            var settings = Store.GetHanetSettings(true);
            if (string.IsNullOrWhiteSpace(settings.AccessToken)) throw new InvalidOperationException("Chưa có access token HANET.");
            if (string.IsNullOrWhiteSpace(settings.PlaceId)) throw new InvalidOperationException("Chưa cấu hình Place ID.");

            var endpoint = settings.ApiBaseUrl.TrimEnd('/') + "/person/getListByPlace";
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            using var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = settings.AccessToken,
                ["placeID"] = settings.PlaceId,
                ["type"] = "0",
                ["page"] = "0",
                ["size"] = "500"
            }));
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
            var code = json["returnCode"]?.ToString() ?? json["code"]?.ToString();
            var ok = response.IsSuccessStatusCode && (code == "1" || code == "200" || string.IsNullOrEmpty(code));
            if (!ok) throw new InvalidOperationException("HANET từ chối yêu cầu: " + (json["returnMessage"]?.ToString() ?? json["message"]?.ToString() ?? response.ReasonPhrase));
            return Json(ApiResponse.Ok(json["data"], "Đã tải danh sách nhân viên từ HANET."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "HANET person list failed");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Admin)]
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

    [HttpGet]
    public IActionResult AttendanceXlsx(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var to = (toDate ?? DateTime.Today).Date;
            var from = (fromDate ?? to.AddDays(-30)).Date;
            if (to < from || (to - from).TotalDays > 62) throw new InvalidOperationException("Khoảng xuất bảng chấm công tối đa là 63 ngày.");
            var rows = Store.GetAttendance(CurrentHrmUser, from, to);
            var employees = rows.GroupBy(x => x.UserId).Select(group => group.OrderByDescending(x => x.WorkDate).First()).OrderBy(x => x.DisplayName).ToList();
            var byEmployeeDate = rows.ToDictionary(x => (x.UserId, x.WorkDate.Date));
            var dates = Enumerable.Range(0, (to - from).Days + 1).Select(offset => from.AddDays(offset)).ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Bảng chấm công");
            sheet.ShowGridLines = false;
            sheet.Cell("A1").Value = "Bảng chấm công";
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 24;
            sheet.Cell("A4").Value = "Tài khoản";
            sheet.Cell("B4").Value = CurrentHrmUser.Username;
            sheet.Cell("A5").Value = "Địa điểm";
            sheet.Cell("B5").Value = "NHIGIA";
            sheet.Cell("A6").Value = "Thời gian";
            sheet.Cell("B6").Value = $"{from:yyyy-MM-dd} đến {to:yyyy-MM-dd}";
            sheet.Cell("A7").Value = "Số lượng nhân viên";
            sheet.Cell("B7").Value = employees.Count;

            var headers = new[] { "ID", "Tên", "Chức vụ", "Phòng ban", "MSNV" };
            for (var index = 0; index < headers.Length; index++) sheet.Cell(11, index + 1).Value = headers[index];
            for (var index = 0; index < dates.Count; index++)
            {
                var column = 6 + index * 2;
                sheet.Range(11, column, 11, column + 1).Merge();
                sheet.Cell(11, column).Value = dates[index].ToString("yyyy-MM-dd");
            }
            var lastColumn = 5 + dates.Count * 2;
            var header = sheet.Range(11, 1, 11, lastColumn);
            header.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#6FA8DC"));
            header.Style.Font.SetBold();
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            for (var employeeIndex = 0; employeeIndex < employees.Count; employeeIndex++)
            {
                var employee = employees[employeeIndex];
                var rowNumber = 12 + employeeIndex;
                sheet.Cell(rowNumber, 1).Value = employee.PersonId ?? employee.UserId.ToString();
                sheet.Cell(rowNumber, 2).Value = employee.DisplayName ?? string.Empty;
                sheet.Cell(rowNumber, 3).Value = employee.JobTitle ?? string.Empty;
                sheet.Cell(rowNumber, 4).Value = employee.DepartmentName ?? string.Empty;
                sheet.Cell(rowNumber, 5).Value = employee.EmployeeCode ?? string.Empty;
                for (var dateIndex = 0; dateIndex < dates.Count; dateIndex++)
                {
                    var column = 6 + dateIndex * 2;
                    if (!byEmployeeDate.TryGetValue((employee.UserId, dates[dateIndex]), out var attendance))
                    {
                        sheet.Cell(rowNumber, column).Value = "-";
                        sheet.Cell(rowNumber, column + 1).Value = "-";
                        continue;
                    }
                    sheet.Cell(rowNumber, column).Value = attendance.CheckIn?.ToString("HH:mm") ?? "-";
                    sheet.Cell(rowNumber, column + 1).Value = attendance.CheckOut?.ToString("HH:mm") ?? "-";
                    if (attendance.LateMinutes > 0) sheet.Cell(rowNumber, column).Style.Font.SetFontColor(XLColor.Red);
                    if (attendance.EarlyMinutes > 0 || attendance.StatusCode == "MISSING_CHECK") sheet.Cell(rowNumber, column + 1).Style.Font.SetFontColor(XLColor.Red);
                }
            }
            if (employees.Count > 0)
            {
                var data = sheet.Range(12, 1, 11 + employees.Count, lastColumn);
                data.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                data.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                data.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                sheet.Range(12, 6, 11 + employees.Count, lastColumn).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
            sheet.Column(1).Width = 23;
            sheet.Column(2).Width = 25;
            sheet.Column(3).Width = 22;
            sheet.Column(4).Width = 24;
            sheet.Column(5).Width = 13;
            for (var column = 6; column <= lastColumn; column++) sheet.Column(column).Width = 8;
            sheet.Row(11).Height = 24;
            sheet.SheetView.FreezeRows(11);
            sheet.SheetView.FreezeColumns(5);
            sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            sheet.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"bang-cham-cong-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.xlsx");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Attendance XLSX export failed");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    [HttpGet]
    [HrmAuthorize(HrmRoles.Admin)]
    public async Task<IActionResult> AttendanceImagesToday()
    {
        try
        {
            var settings = Store.GetHanetSettings(true);
            if (string.IsNullOrWhiteSpace(settings.AccessToken)) throw new InvalidOperationException("Chưa có access token HANET.");
            if (string.IsNullOrWhiteSpace(settings.PlaceId)) throw new InvalidOperationException("Chưa cấu hình Place ID HANET.");

            var vietnam = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");
            var today = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnam).Date;
            var endpoint = settings.ApiBaseUrl.TrimEnd('/') + "/person/getCheckinByPlaceIdInDay";
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
            using var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = settings.AccessToken,
                ["placeID"] = settings.PlaceId,
                ["date"] = today.ToString("yyyy-MM-dd"),
                ["type"] = "0",
                ["exType"] = "1,2",
                ["page"] = "0",
                ["size"] = "500"
            }));
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
            var code = json["returnCode"]?.ToString() ?? json["code"]?.ToString();
            if (!response.IsSuccessStatusCode || (code != "1" && code != "200" && !string.IsNullOrEmpty(code)))
                throw new InvalidOperationException("HANET từ chối yêu cầu: " + (json["returnMessage"]?.ToString() ?? json["message"]?.ToString() ?? response.ReasonPhrase));

            var rows = json["data"] as JsonArray ?? new JsonArray();
            if (rows.Count == 0) throw new InvalidOperationException("Hôm nay chưa có ảnh chấm công trên HANET.");
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                var manifest = new StringBuilder("Nhan vien,Thoi gian,Dia diem,Thiet bi,Person ID,Alias ID,Ten file\r\n");
                var failures = new List<string>();
                var sequence = 0;
                foreach (var node in rows.OfType<JsonObject>())
                {
                    var imageUrl = HanetValue(node, "avatar", "image", "imageUrl", "faceImage", "faceImageUrl");
                    if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri) || imageUri.Scheme != Uri.UriSchemeHttps || !IsTrustedHanetImageHost(imageUri.Host)) continue;
                    var personName = HanetValue(node, "personName", "name") ?? "Nhan-vien";
                    var checkTime = HanetCheckTime(HanetValue(node, "checkinTime", "time", "timestamp"), vietnam);
                    var extension = Path.GetExtension(imageUri.AbsolutePath).ToLowerInvariant();
                    if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp")) extension = ".jpg";
                    var fileName = $"{SafeFileName(personName)}_{checkTime:HH-mm-ss}_{++sequence:000}{extension}";
                    try
                    {
                        var bytes = await client.GetByteArrayAsync(imageUri);
                        if (bytes.Length == 0 || bytes.Length > 15 * 1024 * 1024) throw new InvalidOperationException("Kích thước ảnh không hợp lệ.");
                        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        await using var stream = entry.Open();
                        await stream.WriteAsync(bytes);
                        manifest.AppendLine(string.Join(',', CsvValue(personName), CsvValue(checkTime.ToString("dd/MM/yyyy HH:mm:ss")), CsvValue(HanetValue(node, "place", "placeName")), CsvValue(HanetValue(node, "deviceName", "deviceID")), CsvValue(HanetValue(node, "personID")), CsvValue(HanetValue(node, "aliasID")), CsvValue(fileName)));
                    }
                    catch (Exception exception) { failures.Add($"{personName} - {checkTime:HH:mm:ss}: {exception.Message}"); }
                }
                if (!archive.Entries.Any(x => !x.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("HANET chưa trả về ảnh chấm công có thể tải trong hôm nay.");
                var csvEntry = archive.CreateEntry("danh-sach-cham-cong.csv", CompressionLevel.Fastest);
                await using (var stream = csvEntry.Open()) await stream.WriteAsync(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(manifest.ToString())).ToArray());
                if (failures.Count > 0)
                {
                    var errorEntry = archive.CreateEntry("anh-khong-tai-duoc.txt", CompressionLevel.Fastest);
                    await using var stream = new StreamWriter(errorEntry.Open(), new UTF8Encoding(true));
                    await stream.WriteAsync(string.Join(Environment.NewLine, failures));
                }
            }
            return File(output.ToArray(), "application/zip", $"anh-cham-cong-{today:yyyy-MM-dd}.zip");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "HANET attendance image export failed");
            return BadRequest(ApiResponse.Fail(exception.Message));
        }
    }

    private static string HanetValue(JsonObject source, params string[] names)
    {
        foreach (var property in source)
            if (names.Any(name => string.Equals(name, property.Key, StringComparison.OrdinalIgnoreCase))) return property.Value?.ToString();
        return null;
    }

    private static DateTime HanetCheckTime(string value, TimeZoneInfo timeZone)
    {
        if (long.TryParse(value, out var epoch))
        {
            var instant = epoch > 9999999999 ? DateTimeOffset.FromUnixTimeMilliseconds(epoch) : DateTimeOffset.FromUnixTimeSeconds(epoch);
            return TimeZoneInfo.ConvertTime(instant, timeZone).DateTime;
        }
        return DateTime.TryParse(value, out var parsed) ? parsed : DateTime.Now;
    }

    private static bool IsTrustedHanetImageHost(string host) => host.Equals("hanet.ai", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".hanet.ai", StringComparison.OrdinalIgnoreCase)
        || host.Equals("wasabisys.com", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".wasabisys.com", StringComparison.OrdinalIgnoreCase);

    private static string SafeFileName(string value)
    {
        var result = new string((value ?? string.Empty).Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(result) ? "Nhan-vien" : result;
    }

    private static string CsvValue(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
}
