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
    var provider = DataProtectionProvider.Create(new DirectoryInfo(keys), b => b.SetApplicationName(root + Path.DirectorySeparatorChar));
    var format = new TicketDataFormat(provider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", "Cookies", "v2"));
    string Cookie(string role)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Name, "local-smoke"), new Claim(ClaimTypes.Role, role), new Claim("display_name", "Local smoke") }, "Cookies");
        return "NHIGIA.Auth=" + format.Protect(new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) }, "Cookies"));
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
    await Check("/Work?kind=kpi", null, HttpStatusCode.Redirect);
    await Check("/", "ADMIN", HttpStatusCode.OK, "Ứng dụng eHRM", "Tính lương", "Tuyển dụng", "Quản lý đào tạo", "Tăng ca", "Quản lý nghỉ việc");
    foreach (var kind in new[] { "kpi", "payroll", "recruitment", "training", "overtime", "resignation", "transfer", "assets", "helpdesk" })
        await Check("/Work?kind=" + kind, "ADMIN", HttpStatusCode.OK, "Chưa kết nối", "disabled", "href=\"/Home/Attendance\"");
    await Check("/Work?kind=helpdesk", "EMPLOYEE", HttpStatusCode.OK, "Tạo yêu cầu Helpdesk IT");
    await Check("/Work?kind=overtime", "EMPLOYEE", HttpStatusCode.OK, "Số giờ tăng ca", "Lý do tăng ca");
    await Check("/Work?kind=resignation", "EMPLOYEE", HttpStatusCode.OK, "Ngày làm việc cuối cùng", "Lý do nghỉ việc");
    await Check("/Work?kind=transfer", "EMPLOYEE", HttpStatusCode.Redirect);
    await Check("/Work?kind=payroll", "EMPLOYEE", HttpStatusCode.Redirect);
    await Check("/Work?kind=recruitment", "EMPLOYEE", HttpStatusCode.Redirect);
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
