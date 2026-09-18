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
    public IActionResult CompanyInformation(string section = "introduction", string brand = "nhigia")
    {
        section = section?.Trim().ToLowerInvariant() switch
        {
            "rules" => "rules",
            "brand" => "brand",
            _ => "introduction"
        };

        ViewBag.CompanyBrand = brand?.Trim().ToLowerInvariant() switch
        {
            "gotravel" => "gotravel",
            "ttp" => "ttp",
            _ => "nhigia"
        };
        ViewBag.CompanySection = section;
        ViewBag.Title = section switch
        {
            "rules" => "Nội quy công ty",
            "brand" => "Logo mẫu",
            _ => "Giới thiệu công ty"
        };
        return View();
    }
    public IActionResult Attendance() { ViewBag.Title = "Chấm công"; return View(); }
    public IActionResult WorkSchedules() { ViewBag.Title = "Lịch làm việc"; return View(); }
    public IActionResult LeaveRequests() { ViewBag.Title = "Yêu cầu của tôi"; return View(); }
    public IActionResult InternalCommunications()
    {
        ViewBag.Title = "Truyền thông nội bộ";
        try
        {
            var profile = Store.GetEmployeeProfile(CurrentHrmUser.Id);
            ViewBag.CurrentAvatarUrl = profile?.AvatarUrl ?? string.Empty;
            ViewBag.CurrentJobTitle = profile?.JobTitle ?? string.Empty;
        }
        catch
        {
            ViewBag.CurrentAvatarUrl = string.Empty;
            ViewBag.CurrentJobTitle = string.Empty;
        }
        return View();
    }

    public IActionResult MyProfile(int? id = null)
    {
        int targetId = id.HasValue && id.Value > 0 ? id.Value : CurrentHrmUser.Id;
        if (targetId != CurrentHrmUser.Id)
        {
            bool canView = CurrentHrmUser.RoleCode is HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin
                || Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == targetId);
            if (!canView) return Forbid();
            ViewBag.Title = "Thông tin nhân viên";
        }
        else
        {
            ViewBag.Title = "Hồ sơ của tôi";
        }

        var profile = Store.GetEmployeeProfile(targetId);
        return profile == null ? NotFound() : View(profile);
    }

    [HttpGet]
    public IActionResult Avatar(int id)
    {
        var avatar = Store.GetAvatarContent(id, CurrentHrmUser);
        if (avatar?.Content == null || avatar.Content.Length == 0) return NotFound();
        Response.Headers.CacheControl = "private, max-age=86400";
        return File(avatar.Content, avatar.ContentType ?? "image/jpeg");
    }

    [HttpPost]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar, int? targetUserId = null)
    {
        if (CurrentHrmUser.RoleCode is not (HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin))
        {
            return Json(new { success = false, message = "Chỉ có Quản lý nhân sự mới có quyền cập nhật ảnh nhân viên." });
        }

        if (avatar == null || avatar.Length == 0)
        {
            return Json(new { success = false, message = "Vui lòng chọn file hình ảnh." });
        }
        if (avatar.Length > 5 * 1024 * 1024)
        {
            return Json(new { success = false, message = "Dung lượng ảnh không được vượt quá 5MB." });
        }

        try
        {
            int userIdToUpdate = targetUserId.HasValue && targetUserId.Value > 0 ? targetUserId.Value : CurrentHrmUser.Id;
            if (!Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == userIdToUpdate) && userIdToUpdate != CurrentHrmUser.Id)
            {
                return Json(new { success = false, message = "Bạn không có quyền thay đổi ảnh của nhân viên này." });
            }

            var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowed.Contains(ext))
            {
                return Json(new { success = false, message = "Định dạng file không hỗ trợ. Vui lòng chọn ảnh JPG, PNG hoặc WEBP." });
            }

            await using var stream = new MemoryStream();
            await avatar.CopyToAsync(stream);
            var avatarUrl = Store.UpdateAvatarContent(userIdToUpdate, stream.ToArray(), GetAvatarContentType(ext));

            return Json(new { success = true, avatarUrl });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi lưu ảnh: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult SaveAvatarUrl(string avatarUrl, int? targetUserId = null)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return Json(new { success = false, message = "Vui lòng nhập hoặc chọn ảnh hợp lệ." });
        }

        try
        {
            int userIdToUpdate = CurrentHrmUser.Id;
            if (targetUserId.HasValue && targetUserId.Value != CurrentHrmUser.Id)
            {
                bool canEdit = CurrentHrmUser.RoleCode is HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin
                    || Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == targetUserId.Value);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "Bạn không có quyền cập nhật ảnh của nhân viên này." });
                }
                userIdToUpdate = targetUserId.Value;
            }

            Store.UpdateAvatarUrl(userIdToUpdate, avatarUrl.Trim());
            return Json(new { success = true, avatarUrl = avatarUrl.Trim() });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi cập nhật avatar: " + ex.Message });
        }
    }

    [HttpPost]
    public IActionResult RemoveAvatar(int? targetUserId = null)
    {
        try
        {
            int userIdToUpdate = CurrentHrmUser.Id;
            if (targetUserId.HasValue && targetUserId.Value != CurrentHrmUser.Id)
            {
                bool canEdit = CurrentHrmUser.RoleCode is HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin
                    || Store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == targetUserId.Value);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa ảnh của nhân viên này." });
                }
                userIdToUpdate = targetUserId.Value;
            }

            Store.UpdateAvatarUrl(userIdToUpdate, "");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi xóa ảnh: " + ex.Message });
        }
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
    public async Task<IActionResult> EditEmployeeProfile(EmployeeProfileModel profile, IFormFile avatarFile = null)
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
            byte[] uploadedAvatarContent = null;
            string uploadedAvatarContentType = null;
            if (avatarFile != null && avatarFile.Length > 0)
            {
                if (avatarFile.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("Dung lượng ảnh không được vượt quá 5MB.");
                var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (allowed.Contains(ext))
                {
                    await using var stream = new MemoryStream();
                    await avatarFile.CopyToAsync(stream);
                    uploadedAvatarContent = stream.ToArray();
                    uploadedAvatarContentType = GetAvatarContentType(ext);
                }
                else throw new InvalidOperationException("Định dạng ảnh không được hỗ trợ.");
            }

            profile.DisplayName = profile.DisplayName.Trim();
            profile.EmployeeCode = profile.EmployeeCode.Trim();
            Store.UpdateEmployeeProfile(profile, CurrentHrmUser, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (uploadedAvatarContent != null)
                Store.UpdateAvatarContent(profile.UserId, uploadedAvatarContent, uploadedAvatarContentType);
            TempData["ProfileSuccess"] = $"Đã cập nhật hồ sơ {profile.DisplayName}.";
            return RedirectToAction(nameof(EditEmployeeProfile), new { id = profile.UserId });
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

    private static string GetAvatarContentType(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
    public IActionResult Approvals() { ViewBag.Title = "Phê duyệt yêu cầu"; return View(); }

    [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
    public IActionResult Reports() { ViewBag.Title = "Báo cáo nhân sự"; return View(); }

    [HrmAuthorize(HrmRoles.Admin)]
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
