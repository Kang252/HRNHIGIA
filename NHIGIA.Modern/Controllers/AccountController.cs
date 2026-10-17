using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

public sealed class AccountController : Controller
{
    private readonly HrmDataStore _store;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly KeycloakAvailabilityProbe _keycloakAvailability;

    public AccountController(
        HrmDataStore store,
        ILogger<AccountController> logger,
        IConfiguration configuration,
        KeycloakAvailabilityProbe keycloakAvailability)
    {
        _store = store;
        _logger = logger;
        _configuration = configuration;
        _keycloakAvailability = keycloakAvailability;
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Login(
        string returnUrl = null,
        string error = null,
        CancellationToken cancellationToken = default)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        if (KeycloakEnabled)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                UseSingleSignOn = true
            };

            if (string.Equals(error, "keycloak", StringComparison.OrdinalIgnoreCase))
            {
                model.AuthenticationMessage = "Phiên đăng nhập tập trung không thành công hoặc tài khoản chưa được cấp quyền vào HRM.";
                return View(model);
            }

            if (!await _keycloakAvailability.IsAvailableAsync(cancellationToken))
            {
                _logger.LogWarning(
                    "Tạm dừng chuyển hướng đăng nhập vì Keycloak chưa sẵn sàng. RequestId: {RequestId}",
                    HttpContext.TraceIdentifier);
                return KeycloakUnavailable(model);
            }

            var redirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            try
            {
                await HttpContext.ChallengeAsync(
                    OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties { RedirectUri = redirectUri });
                return new EmptyResult();
            }
            catch (Exception exception) when (!Response.HasStarted && IsKeycloakMetadataFailure(exception))
            {
                _logger.LogWarning(
                    exception,
                    "Không thể tạo yêu cầu đăng nhập Keycloak. RequestId: {RequestId}",
                    HttpContext.TraceIdentifier);
                Response.Clear();
                return KeycloakUnavailable(model);
            }
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (KeycloakEnabled)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }
        if (!ModelState.IsValid) return View(model);

        HrmUserAccountModel user;
        try
        {
            user = _store.FindUser(model.Username);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Không thể kết nối cơ sở dữ liệu khi đăng nhập");
            ModelState.AddModelError(
                string.Empty,
                "Hệ thống chưa kết nối được cơ sở dữ liệu. Vui lòng liên hệ quản trị viên.");
            return View(model);
        }

        if (user == null || !user.IsActive || !HrmPasswordHasher.Verify(model.Password, user.PasswordSalt, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.RoleCode),
            new("display_name", user.DisplayName ?? user.Username),
            new("department_name", user.DepartmentName ?? string.Empty)
        };
        if (user.DepartmentId.HasValue) claims.Add(new Claim("department_id", user.DepartmentId.Value.ToString()));
        if (user.SupervisorUserId.HasValue) claims.Add(new Claim("supervisor_id", user.SupervisorUserId.Value.ToString()));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 168 : 8)
            });

        _store.MarkLogin(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl) : RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (KeycloakEnabled)
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = Url.Action(nameof(Login), "Account") },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private bool KeycloakEnabled => _configuration.GetValue<bool>("Authentication:Keycloak:Enabled");

    private IActionResult KeycloakUnavailable(LoginViewModel model)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        Response.Headers.CacheControl = "no-store, no-cache";
        Response.Headers["Retry-After"] = "5";
        model.AuthenticationUnavailable = true;
        model.AuthenticationMessage = "Dịch vụ đăng nhập tập trung đang khởi động hoặc tạm thời mất kết nối. Vui lòng chờ ít phút rồi thử lại.";
        return View("Login", model);
    }

    private static bool IsKeycloakMetadataFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is HttpRequestException or IOException or TaskCanceledException) return true;
            if (current.Message.Contains("IDX20803", StringComparison.Ordinal) ||
                current.Message.Contains("IDX20804", StringComparison.Ordinal) ||
                current.Message.Contains("IDX20807", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
