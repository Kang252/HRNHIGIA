using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

// Local-only integration check: synthetic authentication tickets never leave loopback.
var root = Path.GetFullPath(args.Length > 0 ? args[0] : "NHIGIA.Modern");
var keys = Path.Combine(Path.GetTempPath(), "nhigia-smoke-" + Guid.NewGuid());
Directory.CreateDirectory(keys);
var start = new ProcessStartInfo("dotnet") { WorkingDirectory = root, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
start.ArgumentList.Add(Path.Combine(root, "bin/Release/net8.0/NHIGIA.Modern.dll"));
start.ArgumentList.Add("--urls"); start.ArgumentList.Add("http://127.0.0.1:5182");
start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
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
        return "NHIGIA.Auth.v2=" + format.Protect(new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) }, "Cookies"));
    }
    async Task Check(string path, string role, HttpStatusCode expected, params string[] text)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (role != null) request.Headers.Add("Cookie", Cookie(role));
        var response = await client.SendAsync(request);
        if (response.StatusCode != expected) throw new Exception($"{path}: expected {expected}, got {response.StatusCode}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        foreach (var value in text) if (!html.Contains(value)) throw new Exception($"{path}: missing {value}");
        Console.WriteLine($"PASS {role ?? "anonymous"} {path}");
    }
    async Task CheckMissing(string path, string role, params string[] text)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Cookie", Cookie(role));
        var response = await client.SendAsync(request);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        foreach (var value in text) if (html.Contains(value)) throw new Exception($"{path}: unexpected {value}");
        Console.WriteLine($"PASS removed content {path}");
    }
    await Check("/Work?kind=kpi", null, HttpStatusCode.Redirect);
    await Check("/Hrm/LeaveAttachment?id=1", null, HttpStatusCode.Redirect);
    await Check("/Account/Login", null, HttpStatusCode.OK, "images/nhigia-logo.png", "hrm-login-logo");
    using (var antiforgeryClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = client.BaseAddress })
    using (var antiforgeryPage = await antiforgeryClient.GetAsync("/Account/Login"))
    {
        var cookies = string.Join(";", antiforgeryPage.Headers.TryGetValues("Set-Cookie", out var values) ? values : []);
        if (!cookies.Contains("NHIGIA.Antiforgery.v2=")) throw new Exception("Stable antiforgery cookie was not issued.");
        Console.WriteLine("PASS stable antiforgery cookie");
    }
    await Check("/", "ADMIN", HttpStatusCode.OK, "Tổng quan hệ thống", "Tài khoản nhân sự", "Helpdesk IT", "images/nhigia-logo.png", "Mở thông báo", "/Work/Notifications", "hrmNotificationBadge", "hrmRowPreview");
    await CheckMissing("/", "ADMIN", "Ứng dụng eHRM", "hrm-app-grid", "inventory_2");
    await Check("/", "EMPLOYEE", HttpStatusCode.OK, "Tổng quan của tôi", "KPI của tôi", "Phiếu lương", "Yêu cầu IT");
    await CheckMissing("/", "EMPLOYEE", "Quản lý nhân sự", "Tuyển dụng", "Điều chuyển nhân sự");
    await Check("/", "MANAGER", HttpStatusCode.OK, "Tổng quan phòng ban", "Nhân sự phòng ban", "KPI phòng ban", "Đào tạo");
    await CheckMissing("/", "MANAGER", "Tuyển dụng", "Điều chuyển nhân sự");
    await Check("/", "HR", HttpStatusCode.OK, "Điều hành nhân sự", "Tính lương", "Tuyển dụng", "Điều chuyển");
    await Check("/", "DIRECTOR", HttpStatusCode.OK, "Tổng quan điều hành", "Duyệt bảng lương", "Báo cáo điều hành");
    await Check("/Home/HanetIntegration", "ADMIN", HttpStatusCode.OK, "Tích hợp camera HANET", "hanetWebhookUrl");
    await Check("/Home/HanetIntegration", "HR", HttpStatusCode.Redirect);
    await Check("/Home/HanetIntegration", "DIRECTOR", HttpStatusCode.Redirect);
    await CheckMissing("/", "HR", "Tích hợp HANET");
    await CheckMissing("/", "DIRECTOR", "Tích hợp HANET");
    foreach (var kind in new[] { "kpi", "payroll", "recruitment", "training", "overtime", "resignation", "transfer", "helpdesk", "vehicle", "meeting", "business-trip" })
        await Check("/Work?kind=" + kind, "ADMIN", HttpStatusCode.OK, "Chưa kết nối", "disabled", "href=\"/Home/Attendance\"");
    await Check("/Work?kind=kpi", "ADMIN", HttpStatusCode.OK, "Tổng tỷ trọng", "Tên tiêu chí KPI", "Mã KPI / Từ khóa", "Phòng ban nhận KPI", "Chọn một nhân viên hoặc một phòng ban", "Cách đo / Nguồn dữ liệu");
    await Check("/Home/InternalCommunications", "ADMIN", HttpStatusCode.OK, "Ảnh đính kèm", "communicationImagePreview", "image/webp", "FormData");
    await Check("/Home/LeaveRequests", "EMPLOYEE", HttpStatusCode.OK, "name=\"attachment\"", "FormData(this)", "LeaveAttachment", "Tệp cũ chưa lưu nội dung");
    await Check("/Work?kind=helpdesk", "EMPLOYEE", HttpStatusCode.OK, "Tạo yêu cầu Helpdesk IT");
    await Check("/Work?kind=overtime", "EMPLOYEE", HttpStatusCode.OK, "Số giờ tăng ca", "Lý do tăng ca");
    await Check("/Work?kind=resignation", "EMPLOYEE", HttpStatusCode.OK, "Ngày làm việc cuối cùng", "Lý do nghỉ việc", "Yêu cầu nghỉ việc");
    await Check("/Work?kind=training", "EMPLOYEE", HttpStatusCode.OK, "Đào tạo của tôi", "Chưa kết nối");
    await Check("/Work?kind=vehicle", "EMPLOYEE", HttpStatusCode.OK, "Đặt xe", "Mục đích chuyến đi", "Điểm đón", "Điểm đến", "Số người đi", "work-layout-vehicle", "Chọn nhân viên đi xe", "meetingEmployeeSearch");
    await Check("/Work?kind=meeting", "EMPLOYEE", HttpStatusCode.OK, "Đặt phòng họp", "Chủ đề cuộc họp", "Phòng họp 1 · 8 người", "Số người tham dự", "meetingEmployeeSearch", "Tìm theo tên hoặc phòng ban", "đã chọn", "work-layout-editor-primary", "Danh sách và phê duyệt");
    await Check("/Work?kind=meeting", "MANAGER", HttpStatusCode.OK, "Tự động xác nhận", "không cần chờ duyệt", "Đặt và xác nhận", "cách nhau ít nhất 10 phút");
    await Check("/Work/Notifications", "EMPLOYEE", HttpStatusCode.OK, "Thông báo của tôi", "Lời mời họp");
    await Check("/Work/NotificationCount", "EMPLOYEE", HttpStatusCode.OK, "Count");
    await Check("/Home/Approvals", "MANAGER", HttpStatusCode.OK, "Phê duyệt yêu cầu", "ApprovalInbox", "DecideApproval", "ApproveAllRequests", "Phê duyệt tất cả", "done_all");
    await Check("/Work?kind=business-trip", "EMPLOYEE", HttpStatusCode.OK, "Công tác của tôi");
    await Check("/Work?kind=business-trip", "MANAGER", HttpStatusCode.OK, "Tạo phân công công tác", "Nơi công tác", "Số quyết định", "work-layout-editor-primary", "Danh sách và phê duyệt");
    await Check("/Work?kind=resignation", "HR", HttpStatusCode.OK, "Quản lý nghỉ việc và thôi việc", "Mở thủ tục thôi việc", "Ngày thôi việc", "Nội dung bàn giao", "Nghỉ việc & thôi việc");
    await Check("/Work?kind=offboarding", "HR", HttpStatusCode.Redirect);
    await Check("/Work?kind=kpi", "MANAGER", HttpStatusCode.OK, "KPI phòng ban", "Chưa kết nối");
    await Check("/Work?kind=assets", "MANAGER", HttpStatusCode.OK, "Tài sản phòng ban", "Chưa kết nối", "assetSearch", "Cấp phát", "Thu hồi", "In QR code", "Export", "readonly", "Hệ thống tự cấp mã kế tiếp");
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
}
finally
{
    if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
    Directory.Delete(keys, recursive: true);
}
