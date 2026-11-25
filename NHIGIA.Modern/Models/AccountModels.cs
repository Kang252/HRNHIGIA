using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models;

public sealed class AccountListItem
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string EmployeeCode { get; set; }
    public string RoleCode { get; set; }
    public string RoleLabel => HrmRoles.Label(RoleCode);
    public string DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public sealed class CreateAccountRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên."), StringLength(150)] public string DisplayName { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tài khoản."), StringLength(50)] public string Username { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mã nhân viên."), StringLength(50)] public string EmployeeCode { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn vai trò.")] public string RoleCode { get; set; }
    public int? DepartmentId { get; set; }
    [StringLength(150)] public string JobTitle { get; set; }
    [EmailAddress(ErrorMessage = "Email công ty không hợp lệ."), StringLength(150)] public string CompanyEmail { get; set; }
    public DateTime? HireDate { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn hình thức lao động.")] public string EmploymentStatus { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại."), DataType(DataType.Password)] public string CurrentPassword { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), DataType(DataType.Password)] public string NewPassword { get; set; }
    [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại chưa khớp.")] public string ConfirmPassword { get; set; }
}
