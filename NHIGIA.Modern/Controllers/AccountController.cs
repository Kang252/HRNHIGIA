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

    public AccountController(HrmDataStore store, ILogger<AccountController> logger, IConfiguration configuration)
    {
        _store = store;
        _logger = logger;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string returnUrl = null, string error = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        if (KeycloakEnabled)
        {
            if (string.Equals(error, "keycloak", StringComparison.OrdinalIgnoreCase))
            {
                TempData["LoginError"] = "Không thể đăng nhập hoặc tài khoản chưa được cấp quyền vào HRM.";
                return View(new LoginViewModel { ReturnUrl = returnUrl });
            }

            var redirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectDefaults.AuthenticationScheme);
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
            var redirectUri = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Action("Index", "Home");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectDefaults.AuthenticationScheme);
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
}
