using System;
using System.Collections.Generic;

namespace NHIGIA.Modern.Models
{
    public class MobileLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceToken { get; set; } // Firebase FCM Token
        public string DeviceModel { get; set; }
        public string Platform { get; set; } // iOS / Android
    }

    public class MobileAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public MobileUserProfile User { get; set; }
    }

    public class MobileUserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string RoleCode { get; set; }
        public string RoleLabel { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitle { get; set; }
        public string EmployeeCode { get; set; }
        public string AvatarUrl { get; set; }
        public string CompanyEmail { get; set; }
        public string MobilePhone { get; set; }
    }

    public class MobileGpsCheckInRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? AccuracyMeters { get; set; }
        public string LocationAddress { get; set; }
        public string CheckType { get; set; } // IN, OUT
        public string SelfieImageBase64 { get; set; }
        public bool IsMockLocation { get; set; }
        public string DeviceInfo { get; set; }
    }

    public class MobilePayslipResponse
    {
        public int Id { get; set; }
        public string Period { get; set; } // e.g. "08/2026"
        public string Title { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal SocialInsuranceDeduction { get; set; }
        public decimal PersonalTaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string StatusCode { get; set; } // PUBLISHED, PAID
        public DateTime? PaidDate { get; set; }
        public bool IsConfirmedByEmployee { get; set; }
        public string Note { get; set; }
    }

    public class MobileUpdateProfileRequest
    {
        public string MobilePhone { get; set; }
        public string PersonalEmail { get; set; }
        public string CurrentAddress { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelationship { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
    }

    public class AiChatRequest
    {
        public string Message { get; set; }
        public string SessionId { get; set; }
    }

    public class AiChatResponse
    {
        public bool Success { get; set; }
        public string Reply { get; set; }
        public List<string> SuggestedActions { get; set; } = new List<string>();
        public List<string> ReferencePolicies { get; set; } = new List<string>();
    }

    public class CvParseResult
    {
        public bool Success { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Education { get; set; }
        public int YearsOfExperience { get; set; }
        public List<string> KeySkills { get; set; } = new List<string>();
        public int MatchScorePercentage { get; set; }
        public string MatchAnalysis { get; set; }
        public List<string> InterviewQuestions { get; set; } = new List<string>();
        public string OverallRecommendation { get; set; }
    }
}

