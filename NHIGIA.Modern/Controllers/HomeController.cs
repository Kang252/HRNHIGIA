using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

public sealed class HomeController : BaseController
{
    private readonly IConfiguration _configuration;

    public HomeController(HrmDataStore store, HrmUserAccessor userAccessor, IConfiguration configuration)
        : base(store, userAccessor) => _configuration = configuration;

    public IActionResult Index() { ViewBag.Title = "Trang chủ"; return View(); }
    public IActionResult Attendance() { ViewBag.Title = "Chấm công"; return View(); }
    public IActionResult WorkSchedules() { ViewBag.Title = "Lịch làm việc"; return View(); }
    public IActionResult LeaveRequests() { ViewBag.Title = "Yêu cầu nghỉ phép"; return View(); }
    public IActionResult InternalCommunications() { ViewBag.Title = "Truyền thông nội bộ"; return View(); }

    public IActionResult MyProfile()
    {
        ViewBag.Title = "Hồ sơ của tôi";
        var profile = Store.GetEmployeeProfile(CurrentHrmUser.Id);
        return profile == null ? NotFound() : View(profile);
    }

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult EmployeeInformation()
    {
        ViewBag.Title = CurrentHrmUser.RoleCode == HrmRoles.Manager ? "Nhân sự phòng ban" : "Quản lý nhân sự";
        return View(Store.GetVisibleUsers(CurrentHrmUser));
    }

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult Approvals() { ViewBag.Title = "Phê duyệt nghỉ phép"; return View(); }

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult Reports() { ViewBag.Title = "Báo cáo nhân sự"; return View(); }

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult HanetIntegration()
    {
        if (!_configuration.GetValue<bool>("Features:HanetIntegrationUiEnabled")) return NotFound();
        ViewBag.Title = "Tích hợp HANET";
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
