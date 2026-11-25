using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccountsController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountsController> _logger;
    public AccountsController(HrmDataStore store, HrmUserAccessor users, IConfiguration configuration, ILogger<AccountsController> logger)
        : base(store, users) { _configuration = configuration; _logger = logger; }

    private string Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private HrmUserAccountModel Actor() => Store.FindUser(CurrentHrmUser?.Id ?? 0);

    [HttpGet]
    public IActionResult Index() => Page(new CreateAccountRequest());

    private IActionResult Page(CreateAccountRequest draft)
    {
        ViewBag.Title = "Quản lý tài khoản";
        ViewBag.Draft = draft;
        ViewBag.Departments = Store.GetDepartments();
        ViewBag.UsesKeycloak = HrmAuthenticationMode.UsesKeycloak(_configuration);
        ViewBag.Actor = Actor();
        return View("Index", Store.GetAccounts(Actor()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(CreateAccountRequest request)
    {
        if (ModelState.IsValid)
        {
            try
            {
                TempData["AccountPassword"] = Store.CreateAccount(request, Actor(), Ip);
                TempData["AccountUsername"] = request.Username.Trim().ToLowerInvariant();
                TempData["AccountSuccess"] = $"Đã tạo tài khoản cho {request.DisplayName.Trim()}.";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException exception) { ModelState.AddModelError("", exception.Message); }
        }
        return Page(request);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ResetPassword(int id) => Run(() =>
    {
        var (username, password) = Store.ResetPassword(id, Actor(), Ip);
        TempData["AccountPassword"] = password;
        TempData["AccountUsername"] = username;
        return $"Đã cấp lại mật khẩu cho {username}. Các phiên đăng nhập cũ đã bị đăng xuất.";
    });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SetActive(int id, bool active) => Run(() =>
    {
        Store.SetAccountActive(id, active, Actor(), Ip);
        return active ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
    });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SetRole(int id, string roleCode) => Run(() =>
    {
        Store.SetAccountRole(id, roleCode, Actor(), Ip);
        return "Đã đổi vai trò. Người dùng cần đăng nhập lại.";
    });

    private IActionResult Run(Func<string> action)
    {
        try { TempData["AccountSuccess"] = action(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Account action failed");
            TempData["AccountError"] = "Không thể cập nhật tài khoản. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index));
    }
}
