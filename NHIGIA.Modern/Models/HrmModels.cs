using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models
{
    public static class HrmRoles
    {
        public const string Admin = "ADMIN";
        public const string Hr = "HR";
        public const string Director = "DIRECTOR";
        public const string Manager = "MANAGER";
        public const string Employee = "EMPLOYEE";

        public static string Label(string roleCode)
        {
            switch ((roleCode ?? string.Empty).ToUpperInvariant())
            {
                case Admin: return "Quản trị hệ thống";
                case Hr: return "Quản trị nhân sự";
                case Director: return "Giám đốc";
                case Manager: return "Trưởng phòng";
                default: return "Nhân viên";
            }
        }

        public static bool CanManagePeople(string roleCode)
        {
            return roleCode == Admin || roleCode == Hr || roleCode == Director || roleCode == Manager;
        }

        public static bool CanPublishCompanyWide(string roleCode)
        {
            return roleCode == Admin || roleCode == Hr || roleCode == Director;
        }
    }

    public class HrmUserAccountModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string DisplayName { get; set; }
        public string RoleCode { get; set; }
        public string RoleLabel { get { return HrmRoles.Label(RoleCode); } }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? SupervisorUserId { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeProfileModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string RoleCode { get; set; }
        public string RoleLabel { get { return HrmRoles.Label(RoleCode); } }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? SupervisorUserId { get; set; }
        public string SupervisorName { get; set; }
        public bool IsActive { get; set; }

        public string EmployeeCode { get; set; }
        public string AvatarUrl { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Nationality { get; set; }
        public string Ethnicity { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }

        public string MobilePhone { get; set; }
        public string OfficePhone { get; set; }
        public string HomePhone { get; set; }
        public string PersonalEmail { get; set; }
        public string CompanyEmail { get; set; }
        public string PermanentAddress { get; set; }
        public string CurrentAddress { get; set; }

        public string IdentityNumber { get; set; }
        public DateTime? IdentityIssuedDate { get; set; }
        public string IdentityIssuedPlace { get; set; }
        public DateTime? IdentityExpiryDate { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportIssuedDate { get; set; }
        public string PassportIssuedPlace { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string PersonalTaxCode { get; set; }

        public string JobTitle { get; set; }
        public string EmploymentStatus { get; set; }
        public string WorkLocation { get; set; }
        public string TimekeepingCode { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? ProbationDate { get; set; }
        public DateTime? OfficialDate { get; set; }
        public string ContractType { get; set; }
        public string ContractNumber { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public decimal? AnnualLeaveDays { get; set; }

        public string EducationLevel { get; set; }
        public string Degree { get; set; }
        public string SchoolName { get; set; }
        public string Faculty { get; set; }
        public string Major { get; set; }
        public string GraduationYear { get; set; }
        public string GraduationClassification { get; set; }

        public string BasicSalary { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string SocialInsuranceNumber { get; set; }
        public DateTime? SocialInsuranceStartDate { get; set; }
        public string HealthInsuranceNumber { get; set; }
        public DateTime? HealthInsuranceExpiryDate { get; set; }
        public string RegisteredHealthFacility { get; set; }

        public string EmergencyContactName { get; set; }
        public string EmergencyContactRelationship { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactEmail { get; set; }
        public string EmergencyContactAddress { get; set; }
        public string Notes { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ScheduleModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string DepartmentName { get; set; }
        public int? ShiftTemplateId { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public int GraceMinutes { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string StatusCode { get; set; }
    }

    public class SaveScheduleRequest
    {
        public int UserId { get; set; }
        public int? ShiftTemplateId { get; set; }
        public string ShiftName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public int GraceMinutes { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class ShiftTemplateModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public int GraceMinutes { get; set; }
        public bool IsOvernight { get; set; }
    }

    public class LeaveRequestModel
    {
        public int Id { get; set; }
        public string RequestCode { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string DepartmentName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionCode { get; set; }
        public string HandoverTo { get; set; }
        public string Reason { get; set; }
        public string AttachmentName { get; set; }
        public bool HasAttachment { get; set; }
        public string StatusCode { get; set; }
        public string ManagerNote { get; set; }
        public string HrNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLeaveRequest
    {
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionCode { get; set; }
        public string HandoverTo { get; set; }
        public string Reason { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentContentType { get; set; }
        public byte[] AttachmentContent { get; set; }
    }

    public class ApprovalRequest
    {
        public int Id { get; set; }
        public bool Approve { get; set; }
        public string Note { get; set; }
    }

    public class ApprovalInboxRequest
    {
        public string Source { get; set; }
        public string Kind { get; set; }
        public int Id { get; set; }
        public bool Approve { get; set; }
        public string Note { get; set; }
    }

    public class CommunicationModel
    {
        public int Id { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorName { get; set; }
        public string Category { get; set; }
        public string ScopeCode { get; set; }
        public int? DepartmentId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentContentType { get; set; }
        public bool IsPinned { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class CreateCommunicationRequest
    {
        public string Category { get; set; }
        public string ScopeCode { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentContentType { get; set; }
        public byte[] AttachmentContent { get; set; }
        public bool IsPinned { get; set; }
    }

    public class CommunicationAttachmentModel
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }

    public class AttendanceRecordModel
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime WorkDate { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan? ScheduledStart { get; set; }
        public TimeSpan? ScheduledEnd { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public string StatusCode { get; set; }
        public string Source { get; set; }
    }

    public class HanetSettingsModel
    {
        public string ApiBaseUrl { get; set; }
        public string OAuthTokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string PlaceId { get; set; }
        public string WebhookSecret { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public string LastSyncStatus { get; set; }
        public string LastSyncMessage { get; set; }
    }

    public class HanetPersonMapRequest
    {
        public int UserId { get; set; }
        public string AliasId { get; set; }
        public string PersonId { get; set; }
        public string PlaceId { get; set; }
    }

    public class HanetWebhookEvent
    {
        public string EventKey { get; set; }
        public string PersonId { get; set; }
        public string AliasId { get; set; }
        public string PlaceId { get; set; }
        public string DeviceId { get; set; }
        public DateTime CheckTime { get; set; }
        public string EventType { get; set; }
        public string PayloadJson { get; set; }
    }

    public class DashboardModel
    {
        public int TotalEmployees { get; set; }
        public int PresentToday { get; set; }
        public int LateOrEarlyToday { get; set; }
        public int PendingApprovals { get; set; }
        public int UnmappedHanetUsers { get; set; }
        public List<AttendanceRecordModel> AttendanceToday { get; set; }
        public List<CommunicationModel> Communications { get; set; }
    }

    public class LeaveStatsModel
    {
        public decimal AnnualAllowance { get; set; }
        public decimal UsedDays { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static ApiResponse Ok(object data, string message = null)
        {
            return new ApiResponse { Success = true, Data = data, Message = message };
        }

        public static ApiResponse Fail(string message)
        {
            return new ApiResponse { Success = false, Message = message };
        }
    }
}
