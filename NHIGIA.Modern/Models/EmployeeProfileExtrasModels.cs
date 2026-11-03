using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models;

public sealed class EmployeeAssignedAssetModel
{
    public int Id { get; set; }
    public string Reference { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Status { get; set; }
    public string Condition { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string Notes { get; set; }
}

public sealed class EmployeeUniformModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required, StringLength(150)] public string ItemName { get; set; }
    [StringLength(50)] public string Size { get; set; }
    [Range(1, 1000)] public int Quantity { get; set; } = 1;
    public DateTime IssuedDate { get; set; } = DateTime.Today;
    public DateTime? ReturnedDate { get; set; }
    [Required, StringLength(30)] public string StatusCode { get; set; } = "ISSUED";
    [StringLength(500)] public string Notes { get; set; }
}

public sealed class EmployeeSoftwareAccountModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required, StringLength(150)] public string SoftwareName { get; set; }
    [StringLength(500), Url] public string LoginUrl { get; set; }
    [StringLength(100)] public string LoginMethod { get; set; }
    [StringLength(200)] public string Username { get; set; }
    [StringLength(500)] public string Password { get; set; }
    [StringLength(500)] public string PermissionScope { get; set; }
    public bool IsActive { get; set; } = true;
    public bool HasPassword { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class EmployeeProfileExtrasModel
{
    public int UserId { get; set; }
    public bool CanManage { get; set; }
    public bool CanRevealPasswords { get; set; }
    public List<EmployeeAssignedAssetModel> Assets { get; set; } = new();
    public List<EmployeeUniformModel> Uniforms { get; set; } = new();
    public List<EmployeeSoftwareAccountModel> SoftwareAccounts { get; set; } = new();
}
