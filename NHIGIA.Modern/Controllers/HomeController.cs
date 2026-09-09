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

    [HttpGet]
    [HrmAuthorize(HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult EditEmployeeProfile(int id)
    {
        if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == id)) return Forbid();
        var profile = Store.GetEmployeeProfile(id);
        if (profile == null) return NotFound();
        PrepareEmployeeProfileForm(id);
        ViewBag.Title = "Cập nhật hồ sơ nhân viên";
        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HrmAuthorize(HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult EditEmployeeProfile(EmployeeProfileModel profile)
    {
        if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == profile.UserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(profile.DisplayName)) ModelState.AddModelError(nameof(profile.DisplayName), "Vui lòng nhập họ và tên.");
        if (string.IsNullOrWhiteSpace(profile.EmployeeCode)) ModelState.AddModelError(nameof(profile.EmployeeCode), "Vui lòng nhập mã nhân viên.");
        if (profile.SupervisorUserId == profile.UserId) ModelState.AddModelError(nameof(profile.SupervisorUserId), "Nhân viên không thể tự quản lý chính mình.");
        if (profile.AnnualLeaveDays is < 0 or > 365) ModelState.AddModelError(nameof(profile.AnnualLeaveDays), "Số ngày phép phải từ 0 đến 365.");
        if (profile.ContractStartDate.HasValue && profile.ContractEndDate < profile.ContractStartDate)
            ModelState.AddModelError(nameof(profile.ContractEndDate), "Ngày kết thúc hợp đồng phải sau ngày bắt đầu.");
        foreach (var email in new[] { profile.PersonalEmail, profile.CompanyEmail, profile.EmergencyContactEmail }.Where(x => !string.IsNullOrWhiteSpace(x)))
            if (!System.Net.Mail.MailAddress.TryCreate(email, out _)) ModelState.AddModelError("", $"Email '{email}' không hợp lệ.");

        var departments = Store.GetDepartments();
        if (profile.DepartmentId.HasValue && !departments.Any(x => x.Id == profile.DepartmentId))
            ModelState.AddModelError(nameof(profile.DepartmentId), "Phòng ban không hợp lệ.");
        var supervisors = Store.GetVisibleUsers(CurrentHrmUser).Where(x => x.Id != profile.UserId).ToList();
        if (profile.SupervisorUserId.HasValue && !supervisors.Any(x => x.Id == profile.SupervisorUserId))
            ModelState.AddModelError(nameof(profile.SupervisorUserId), "Quản lý trực tiếp không hợp lệ.");
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = departments;
            ViewBag.Supervisors = supervisors;
            ViewBag.Title = "Cập nhật hồ sơ nhân viên";
            return View(profile);
        }

        try
        {
            profile.DisplayName = profile.DisplayName.Trim();
            profile.EmployeeCode = profile.EmployeeCode.Trim();
            Store.UpdateEmployeeProfile(profile, CurrentHrmUser, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["ProfileSuccess"] = $"Đã cập nhật hồ sơ {profile.DisplayName}.";
            return RedirectToAction(nameof(EmployeeInformation));
        }
        catch (Exception exception)
        {
            ModelState.AddModelError("", exception is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 }
                ? "Mã nhân viên đã được sử dụng."
                : "Không thể lưu hồ sơ. Vui lòng kiểm tra dữ liệu và thử lại.");
            ViewBag.Departments = departments;
            ViewBag.Supervisors = supervisors;
            ViewBag.Title = "Cập nhật hồ sơ nhân viên";
            return View(profile);
        }
    }

    private void PrepareEmployeeProfileForm(int employeeId)
    {
        ViewBag.Departments = Store.GetDepartments();
        ViewBag.Supervisors = Store.GetVisibleUsers(CurrentHrmUser).Where(x => x.Id != employeeId).ToList();
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
