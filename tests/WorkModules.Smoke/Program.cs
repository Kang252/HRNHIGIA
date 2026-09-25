using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

// Local-only integration check: synthetic authentication tickets never leave loopback.
var root = Path.GetFullPath(args.Length > 0 ? args[0] : "NHIGIA.Modern");
var assistantOnly = args.Skip(1).Contains("--assistant-only");
var communicationsOnly = args.Skip(1).Contains("--communications-only");
var attendanceOnly = args.Skip(1).Contains("--attendance-only");
var keys = Path.Combine(Path.GetTempPath(), "nhigia-smoke-" + Guid.NewGuid());
Directory.CreateDirectory(keys);
var start = new ProcessStartInfo("dotnet") { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
start.ArgumentList.Add(Path.Combine(root, "bin/Release/net8.0/NHIGIA.Modern.dll"));
start.ArgumentList.Add("--urls"); start.ArgumentList.Add("http://127.0.0.1:5182");
start.Environment["ASPNETCORE_ENVIRONMENT"] = communicationsOnly || attendanceOnly ? "Development" : "Production";
start.Environment["HRM_DATA_PROTECTION_PATH"] = keys;
start.Environment["HRM_CONNECTION_STRING"] = "Server=tcp:127.0.0.1,1;Database=unavailable;User Id=smoke;Password=unused;Connect Timeout=1;Encrypt=True";
using var process = Process.Start(start)!;
process.OutputDataReceived += (_, _) => { }; process.ErrorDataReceived += (_, _) => { };
process.BeginOutputReadLine(); process.BeginErrorReadLine();
try
{
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri("http://127.0.0.1:5182"), Timeout = TimeSpan.FromSeconds(10) };
    var ready = false;
    for (int i = 0; i < 30; i++)
    {
        try { ready = (await client.GetAsync("/Health")).IsSuccessStatusCode; } catch (HttpRequestException) { }
        if (ready) break;
        await Task.Delay(500);
    }
    if (!ready) throw new Exception("Local test host failed to start.");
    using (var proxyRequest = new HttpRequestMessage(HttpMethod.Get, "/Work?kind=kpi"))
    {
        proxyRequest.Headers.Add("X-Forwarded-Proto", "https");
        var proxyResponse = await client.SendAsync(proxyRequest);
        if (proxyResponse.Headers.Location?.Scheme != "https")
            throw new Exception("Forwarded HTTPS scheme was not preserved in the login redirect.");
        Console.WriteLine("PASS forwarded HTTPS redirect");
    }
    var provider = DataProtectionProvider.Create(new DirectoryInfo(keys), b => b.SetApplicationName("NHIGIA.Modern.v1"));
    var format = new TicketDataFormat(provider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", "Cookies", "v2"));
    string Cookie(string role)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Name, "local-smoke"), new Claim(ClaimTypes.Role, role), new Claim("display_name", "Local smoke") }, "Cookies");
        return "NHIGIA.Auth.v3=" + format.Protect(new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) }, "Cookies"));
    }
    var failures = new List<string>();
    async Task Check(string path, string role, HttpStatusCode expected, params string[] text)
    {
        var before = failures.Count;
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (role != null) request.Headers.Add("Cookie", Cookie(role));
        var response = await client.SendAsync(request);
        var rawHtml = await response.Content.ReadAsStringAsync();
        var html = WebUtility.HtmlDecode(rawHtml);
        if (args.Contains("--snapshots") && response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "text/html")
        {
            var snapshots = Path.GetFullPath(Path.Combine(root, "..", ".tmp-audit-ui"));
            Directory.CreateDirectory(snapshots);
            var filename = (role ?? "anonymous") + "-" + Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(path)) + ".html";
            await File.WriteAllTextAsync(Path.Combine(snapshots, filename), rawHtml);
        }
        if (response.StatusCode != expected) failures.Add($"{path}: expected {expected}, got {response.StatusCode}");
        foreach (var value in text) if (!html.Contains(value)) failures.Add($"{path}: missing {value}");
        Console.WriteLine($"{(failures.Count==before ? "PASS" : "FAIL")} {role ?? "anonymous"} {path}");
    }
    async Task CheckMissing(string path, string role, params string[] text)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Cookie", Cookie(role));
        var response = await client.SendAsync(request);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        if (!response.IsSuccessStatusCode) throw new Exception($"{path}: failed negative-content check with {response.StatusCode}");
        foreach (var value in text) if (html.Contains(value)) failures.Add($"{path}: unexpected {value}");
        Console.WriteLine($"PASS removed content {path}");
    }
    if (communicationsOnly)
    {
        await Check("/Home/InternalCommunications", "ADMIN", HttpStatusCode.OK, "Ảnh / tệp", "id=\"preview\"", ".webp", "FormData", "AuthorAvatarUrl", "AuthorJobTitle", "wall-avatar", "Ảnh đại diện");
        if (failures.Count>0) throw new Exception(string.Join("\n",failures));
        return;
    }
    if (attendanceOnly)
    {
        await Check("/Home/Attendance", "ADMIN", HttpStatusCode.OK, "Đang cập nhật", "Lần quét gần nhất", "Chờ kết thúc ca", "IsProvisional", "Đồng bộ hôm nay", "SyncHanetAttendanceToday", "Nghỉ đã duyệt", "ExpectedStartAt", "Đối chiếu và chốt công tháng", "Xác nhận dữ liệu của tôi", "Khóa kỳ", "AttendanceAdjustments");
        await Check("/Home/Attendance", "EMPLOYEE", HttpStatusCode.OK, "Nghỉ đã duyệt", "APPROVED_LEAVE");
        await Check("/Home/WorkSchedules", "EMPLOYEE", HttpStatusCode.OK, "Lịch nghỉ đã duyệt", "scheduleLeaveMonth", "/Hrm/ScheduleLeaves");
        await Check("/Hrm/ScheduleLeaves", null, HttpStatusCode.Redirect);
        await Check("/Hrm/ScheduleLeaves?fromDate=2026-10-10&toDate=2026-09-01", "EMPLOYEE", HttpStatusCode.BadRequest, "366");
        if (failures.Count>0) throw new Exception(string.Join("\n",failures));
        return;
    }
    await Check("/Work?kind=kpi", null, HttpStatusCode.Redirect);
    await Check("/Hrm/LeaveAttachment?id=1", null, HttpStatusCode.Redirect);
    await Check("/Account/Login", null, HttpStatusCode.OK, "images/nhigia-header-logo.png", "hrm-login-logo");
    using (var antiforgeryClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = client.BaseAddress })
    using (var antiforgeryPage = await antiforgeryClient.GetAsync("/Account/Login"))
    {
        var cookies = string.Join(";", antiforgeryPage.Headers.TryGetValues("Set-Cookie", out var values) ? values : []);
        if (!cookies.Contains("NHIGIA.Antiforgery.v2=")) throw new Exception("Stable antiforgery cookie was not issued.");
        Console.WriteLine("PASS stable antiforgery cookie");
    }
    await Check("/", "ADMIN", HttpStatusCode.OK, "Tổng quan hệ thống", "Tài khoản nhân sự", "Helpdesk IT", "images/nhigia-header-logo.png", "Mở thông báo", "/Work/Notifications", "hrmNotificationBadge", "hrmRowPreview", "Trợ lý Nhị Gia", "Trò chuyện tự do", "Chỉ dữ liệu SQL bị giới hạn", "/Assistant/Ask", "hrm-assistant.js");
    await Check("/js/hrm-assistant.js", null, HttpStatusCode.OK, "readJsonResponse", "response.text()", "trang lỗi thay vì dữ liệu trợ lý");
    await CheckMissing("/", "ADMIN", "Ứng dụng eHRM", "hrm-app-grid", "inventory_2");
    await Check("/", "EMPLOYEE", HttpStatusCode.OK, "Tổng quan của tôi", "KPI của tôi", "Phiếu lương", "Yêu cầu IT");
    await CheckMissing("/", "EMPLOYEE", "Quản lý nhân sự", "Tuyển dụng", "Điều chuyển nhân sự");
    await Check("/", "MANAGER", HttpStatusCode.OK, "Tổng quan phòng ban", "Nhân sự phòng ban", "KPI phòng ban", "Đào tạo");
    await CheckMissing("/", "MANAGER", "Tuyển dụng", "Điều chuyển nhân sự");
    await Check("/", "HR", HttpStatusCode.OK, "Điều hành nhân sự", "Tính lương", "Tuyển dụng", "Điều chuyển");
    await Check("/", "DIRECTOR", HttpStatusCode.OK, "Tổng quan điều hành", "Duyệt bảng lương", "Báo cáo điều hành");
    using (var assistantPost = new HttpRequestMessage(HttpMethod.Post, "/Assistant/Ask"))
    {
        assistantPost.Headers.Add("Cookie", Cookie("EMPLOYEE"));
        assistantPost.Content = new StringContent("{\"Question\":\"Chấm công hôm nay\"}", System.Text.Encoding.UTF8, "application/json");
        if ((await client.SendAsync(assistantPost)).StatusCode != HttpStatusCode.BadRequest) throw new Exception("Assistant accepted a request without anti-forgery token.");
        Console.WriteLine("PASS assistant anti-forgery protection");
    }
    using (var anonymousAssistantPost = new HttpRequestMessage(HttpMethod.Post, "/Assistant/Ask"))
    {
        anonymousAssistantPost.Content = new StringContent("{\"Question\":\"Xin chào\"}", System.Text.Encoding.UTF8, "application/json");
        var anonymousResponse = await client.SendAsync(anonymousAssistantPost);
        var anonymousBody = await anonymousResponse.Content.ReadAsStringAsync();
        if (anonymousResponse.StatusCode != HttpStatusCode.Unauthorized || !anonymousBody.Contains("Phiên đăng nhập đã hết hạn"))
            throw new Exception("Assistant did not return a JSON 401 when the login session was missing.");
        Console.WriteLine("PASS assistant JSON unauthorized response");
    }
    foreach (var role in new[] { "EMPLOYEE", "MANAGER", "HR", "DIRECTOR" })
        await Check("/Hrm/HanetDevices", role, HttpStatusCode.Redirect);
    foreach (var endpoint in new[] { "SaveHanetDevice", "DeleteHanetDevice", "SyncHanetAttendanceToday" })
    {
        using var devicePost = new HttpRequestMessage(HttpMethod.Post, "/Hrm/" + endpoint);
        devicePost.Headers.Add("Cookie", Cookie("ADMIN"));
        devicePost.Content = new FormUrlEncodedContent(new Dictionary<string,string> { ["Id"]="1" });
        if ((await client.SendAsync(devicePost)).StatusCode != HttpStatusCode.BadRequest)
            throw new Exception(endpoint + " accepted a request without anti-forgery token.");
        Console.WriteLine("PASS device anti-forgery " + endpoint);
    }
    await Check("/Home/HanetIntegration", "ADMIN", HttpStatusCode.OK, "Danh sách thiết bị đã lưu", "deviceSearch", "SaveHanetDevice", "DeleteHanetDevice");
    foreach (var brand in new[] { "gotravel", "ttp", "nhigia" })
        await Check("/Home/CompanyInformation?section=brand&brand=" + brand, "EMPLOYEE", HttpStatusCode.OK,
            "GoTravel", "TTP", "Nhị Gia", "download=\"" + brand + "-logo.png\"", "company-brand-tabs");
    if (assistantOnly) return;
    await Check("/Home/HanetIntegration", "ADMIN", HttpStatusCode.OK, "Tích hợp camera HANET", "hanetWebhookUrl", "Nhập hàng loạt bằng file", "HanetMappingTemplate", "ImportHanetMappings");
    await Check("/Hrm/HanetMappingTemplate", "ADMIN", HttpStatusCode.OK, "MaNhanVien,TaiKhoan,PersonID,AliasID,PlaceID");
    await Check("/Home/HanetIntegration", "HR", HttpStatusCode.Redirect);
    await Check("/Home/HanetIntegration", "DIRECTOR", HttpStatusCode.Redirect);
    await CheckMissing("/", "HR", "Tích hợp HANET");
    await CheckMissing("/", "DIRECTOR", "Tích hợp HANET");
    foreach (var kind in new[] { "kpi", "payroll", "recruitment", "training", "overtime", "resignation", "transfer", "helpdesk", "vehicle", "meeting", "business-trip" })
        await Check("/Work?kind=" + kind, "ADMIN", HttpStatusCode.OK, "Chưa kết nối", "disabled", "href=\"/Home/Attendance\"");
    await Check("/Work?kind=kpi", "ADMIN", HttpStatusCode.OK, "Danh sách chỉ định KPI", "Cách đo / Nguồn dữ liệu", "kpiFilterForm", "SubmitKpiProof", "EvaluateKpiProof");
    await Check("/Home/InternalCommunications", "ADMIN", HttpStatusCode.OK, "Ảnh / tệp", "id=\"preview\"", ".webp", "FormData", "AuthorAvatarUrl", "AuthorJobTitle", "wall-avatar", "Ảnh đại diện");
    await Check("/Home/LeaveRequests", "EMPLOYEE", HttpStatusCode.OK, "name=\"attachment\"", "FormData(this)", "LeaveAttachment", "Tệp cũ chưa lưu nội dung");
    await Check("/Work?kind=helpdesk", "EMPLOYEE", HttpStatusCode.OK, "Tạo yêu cầu Helpdesk IT");
    await Check("/Work?kind=overtime", "EMPLOYEE", HttpStatusCode.OK, "Số giờ tăng ca", "Lý do tăng ca");
    await Check("/Work?kind=resignation", "EMPLOYEE", HttpStatusCode.OK, "Ngày làm việc cuối cùng", "Lý do thôi việc", "Yêu cầu nghỉ việc");
    await Check("/Work?kind=training", "EMPLOYEE", HttpStatusCode.OK, "Đào tạo của tôi", "Chưa kết nối");
    await Check("/Work?kind=vehicle", "EMPLOYEE", HttpStatusCode.OK, "Đặt xe", "Mục đích chuyến đi", "Điểm đón", "Điểm đến", "Số người đi", "work-layout-vehicle", "Chọn nhân viên đi xe", "meetingEmployeeSearch");
    await Check("/Work?kind=meeting", "EMPLOYEE", HttpStatusCode.OK, "Đặt phòng họp", "Chủ đề cuộc họp", "Phòng họp 1 · 8 người", "Số người tham dự", "meetingEmployeeSearch", "Tìm theo tên hoặc phòng ban", "đã chọn", "work-layout-editor-primary", "Danh sách và phê duyệt");
    await Check("/Work?kind=meeting", "MANAGER", HttpStatusCode.OK, "Tự động xác nhận", "không cần chờ duyệt", "Lưu yêu cầu", "cách nhau ít nhất 10 phút");
    await Check("/Work/Notifications", "EMPLOYEE", HttpStatusCode.OK, "Thông báo của tôi", "Hộp thông báo", "Không thể tải thông báo");
    await Check("/Work/NotificationCount", "EMPLOYEE", HttpStatusCode.ServiceUnavailable, "Message");
    await Check("/Home/Approvals", "MANAGER", HttpStatusCode.OK, "Phê duyệt yêu cầu", "ApprovalInbox", "DecideApproval", "ApproveAllRequests", "Phê duyệt tất cả", "done_all");
    await Check("/Work?kind=business-trip", "EMPLOYEE", HttpStatusCode.OK, "Công tác của tôi");
    await Check("/Work?kind=business-trip", "MANAGER", HttpStatusCode.OK, "Tạo phân công công tác", "Nơi công tác", "Số quyết định", "work-layout-editor-primary", "Danh sách và phê duyệt");
    await Check("/Work?kind=resignation", "HR", HttpStatusCode.OK, "Quản lý nghỉ việc và thôi việc", "Mở thủ tục thôi việc", "Ngày thôi việc", "Nội dung bàn giao");
    await Check("/Work?kind=offboarding", "HR", HttpStatusCode.Redirect);
    await Check("/Work?kind=kpi", "MANAGER", HttpStatusCode.OK, "KPI phòng ban", "Chưa kết nối");
    await Check("/Work?kind=assets&asTab=list", "MANAGER", HttpStatusCode.OK, "Tài sản phòng ban", "Chưa kết nối", "Danh sách tài sản trong hệ thống", "Cấp phát", "Thu hồi", "readonly", "Hệ thống tự cấp mã kế tiếp");
    // Failed saves must return the editor with user-entered fields; no SQL can be reached by this host.
    using (var assetGet = new HttpRequestMessage(HttpMethod.Get, "/Work?kind=assets&asTab=list"))
    {
        var auth = Cookie("ADMIN");
        assetGet.Headers.Add("Cookie", auth);
        var editor = await client.SendAsync(assetGet);
        var editorHtml = await editor.Content.ReadAsStringAsync();
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(editorHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if (!tokenMatch.Success) throw new Exception("Asset editor missing anti-forgery field.");
        using var save = new HttpRequestMessage(HttpMethod.Post, "/Work/SaveAssetItem");
        save.Headers.Add("Cookie", auth);
        save.Content = new FormUrlEncodedContent(new Dictionary<string,string> {
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(tokenMatch.Groups[1].Value),
            ["Title"]="Laptop audit retained", ["Target"]="-10", ["WorkLocation"]="Office", ["Category"]="Laptop",
            ["Description"]="Keep my draft", ["StartDate"]="2026-09-25"
        });
        var saved = await client.SendAsync(save);
        var failedHtml = await saved.Content.ReadAsStringAsync();
        if (saved.StatusCode != HttpStatusCode.OK || !failedHtml.Contains("Laptop audit retained") ||
            !failedHtml.Contains("Keep my draft") || !WebUtility.HtmlDecode(failedHtml).Contains("Nguyên giá tài sản không được âm"))
            throw new Exception("Failed asset save lost draft or validation messages.");
        if (failedHtml.Contains("Kind field is required")) throw new Exception("Server-owned asset Kind failed validation.");
        if (args.Contains("--snapshots")) await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(root)!, ".tmp-audit-ui", "asset-save-failed.html"),failedHtml);
        Console.WriteLine("PASS failed asset save retains fields and validation; Kind inferred by endpoint");
    }
    await Check("/Work?kind=transfer", "EMPLOYEE", HttpStatusCode.Redirect);
    await Check("/Work?kind=payroll", "EMPLOYEE", HttpStatusCode.OK, "Phiếu lương", "Chưa kết nối");
    await Check("/Work?kind=recruitment", "EMPLOYEE", HttpStatusCode.Redirect);
    await Check("/Work?kind=recruitment", "MANAGER", HttpStatusCode.Redirect);
    await Check("/Home/EditEmployeeProfile/1", "EMPLOYEE", HttpStatusCode.Redirect);
    await Check("/Home/EditEmployeeProfile/1", "MANAGER", HttpStatusCode.Redirect);
    await Check("/Work?kind=unknown", "ADMIN", HttpStatusCode.NotFound);
    using var post = new HttpRequestMessage(HttpMethod.Post, "/Work/Create");
    post.Headers.Add("Cookie", Cookie("ADMIN"));
    post.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["Kind"] = "helpdesk", ["Title"] = "test" });
    if ((await client.SendAsync(post)).StatusCode != HttpStatusCode.BadRequest) throw new Exception("Missing anti-forgery token was accepted.");
    Console.WriteLine("PASS anti-forgery protection");
    using (var ajax = new HttpRequestMessage(HttpMethod.Get, "/Hrm/Users"))
    {
        ajax.Headers.Add("X-Requested-With", "XMLHttpRequest");
        var response = await client.SendAsync(ajax);
        if (response.StatusCode != HttpStatusCode.Unauthorized || response.Content.Headers.ContentType?.MediaType != "application/json")
            failures.Add("AJAX session expiry did not return JSON 401");
        else Console.WriteLine("PASS AJAX expiry JSON response");
    }
    if (failures.Count>0) throw new Exception(string.Join("\n",failures));
}
finally
{
    if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
    Directory.Delete(keys, recursive: true);
}
