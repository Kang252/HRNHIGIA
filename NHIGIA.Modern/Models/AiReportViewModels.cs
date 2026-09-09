using System;
using System.Collections.Generic;

namespace NHIGIA.Modern.Models
{
    public class AiReportDashboardViewModel
    {
        public string Period { get; set; } = "Tháng hiện tại";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int HrHealthScore { get; set; } = 85;
        public string HealthScoreAssessment { get; set; } = "Ổn định - Cần tối ưu chi phí OT";

        // Tóm tắt điều hành nhanh
        public int TotalEmployees { get; set; }
        public int ResignedThisPeriod { get; set; }
        public decimal TurnoverRate { get; set; }
        public decimal TotalPayrollCost { get; set; }
        public decimal TotalOtCost { get; set; }
        public decimal TotalOtHours { get; set; }
        public int AbnormalOtCount { get; set; }
        public decimal CompanyKpiRate { get; set; }
        public int OpenRecruitmentCount { get; set; }

        // Nhận định từ AI
        public AiExecutiveInsight Insight { get; set; } = new();

        // 5 Nhóm chi tiết
        public TurnoverAnalyticsModel Turnover { get; set; } = new();
        public PersonnelCostAnalyticsModel PersonnelCost { get; set; } = new();
        public OvertimeAnomalyModel OvertimeAnomaly { get; set; } = new();
        public DepartmentPerformanceModel DepartmentPerformance { get; set; } = new();
        public RecruitmentPipelineModel Recruitment { get; set; } = new();
    }

    public class AiExecutiveInsight
    {
        public string ExecutiveSummary { get; set; }
        public List<string> Highlights { get; set; } = new();
        public List<string> KeyRisks { get; set; } = new();
        public List<string> StrategicRecommendations { get; set; } = new();
    }

    public class TurnoverAnalyticsModel
    {
        public decimal RatePercent { get; set; }
        public int TotalResignations { get; set; }
        public int HighRiskCount { get; set; }
        public string RetentionAssessment { get; set; }
        public List<DepartmentTurnoverItem> DepartmentBreakdown { get; set; } = new();
        public List<ResignationRecordItem> RecentResignations { get; set; } = new();
        public List<FlightRiskItem> FlightRisks { get; set; } = new();
    }

    public class DepartmentTurnoverItem
    {
        public string DepartmentName { get; set; }
        public int TotalHeadcount { get; set; }
        public int ResignCount { get; set; }
        public decimal TurnoverRate { get; set; }
        public string RiskLevel { get; set; } // THẤP, TRUNG BÌNH, CAO
    }

    public class ResignationRecordItem
    {
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitle { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
    }

    public class FlightRiskItem
    {
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string RiskFactor { get; set; }
        public string WarningLevel { get; set; } // VÀNG, ĐỎ
        public string SuggestedAction { get; set; }
    }

    public class PersonnelCostAnalyticsModel
    {
        public decimal TotalEstimatedPayroll { get; set; }
        public decimal TotalOvertimeCost { get; set; }
        public decimal AvgSalaryPerHead { get; set; }
        public decimal OtCostRatioPercent { get; set; }
        public string CostEfficiencyVerdict { get; set; }
        public List<DepartmentCostItem> DepartmentCosts { get; set; } = new();
        public List<MonthlyCostTrendItem> MonthlyTrends { get; set; } = new();
    }

    public class DepartmentCostItem
    {
        public string DepartmentName { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal OvertimeCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class MonthlyCostTrendItem
    {
        public string MonthLabel { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal OvertimeCost { get; set; }
        public decimal Total { get; set; }
    }

    public class OvertimeAnomalyModel
    {
        public decimal TotalHours { get; set; }
        public int OverLimitEmployeesCount { get; set; }
        public decimal LegalMonthlyLimit { get; set; } = 40m; // Luật lao động: 40h/tháng
        public string ComplianceStatus { get; set; }
        public List<AnomalousEmployeeItem> AnomalousEmployees { get; set; } = new();
        public List<DepartmentOtItem> DepartmentOtList { get; set; } = new();
    }

    public class AnomalousEmployeeItem
    {
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public decimal OtHours { get; set; }
        public string AnomalyType { get; set; } // VƯỢT TRẦN LUẬT, TĂNG ĐỘT BIẾN, KHÔNG HIỆU QUẢ
        public string KpiStatus { get; set; }
        public string Recommendation { get; set; }
    }

    public class DepartmentOtItem
    {
        public string DepartmentName { get; set; }
        public decimal TotalHours { get; set; }
        public decimal AvgHoursPerPerson { get; set; }
        public int ExceedingLimitCount { get; set; }
        public string AlertColor { get; set; }
    }

    public class DepartmentPerformanceModel
    {
        public decimal CompanyAverageCompletionRate { get; set; }
        public int TotalKpisTracked { get; set; }
        public int CompletedKpisCount { get; set; }
        public string TopPerformingDepartment { get; set; }
        public string LowestPerformingDepartment { get; set; }
        public List<DepartmentKpiSummaryItem> DepartmentSummaries { get; set; } = new();
        public List<KpiDetailItem> HighPriorityKpis { get; set; } = new();
    }

    public class DepartmentKpiSummaryItem
    {
        public string DepartmentName { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public string PerformanceRank { get; set; } // XUẤT SẮC, ĐẠT, CẦN CẢI THIỆN
    }

    public class KpiDetailItem
    {
        public string Title { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public decimal Target { get; set; }
        public decimal Actual { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal Weight { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
    }

    public class RecruitmentPipelineModel
    {
        public int ActiveCampaigns { get; set; }
        public int TotalTargetHeadcount { get; set; }
        public int HiredCount { get; set; }
        public decimal FillRatePercent { get; set; }
        public int AverageDaysToHire { get; set; } = 21;
        public string PipelineBottleneck { get; set; }
        public List<RecruitmentCampaignItem> Campaigns { get; set; } = new();
    }

    public class RecruitmentCampaignItem
    {
        public string PositionTitle { get; set; }
        public string DepartmentName { get; set; }
        public int TargetHeadcount { get; set; }
        public int CurrentHired { get; set; }
        public decimal ProgressPercent { get; set; }
        public string DueDateString { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
    }

    public class AiAskRequest
    {
        public string Question { get; set; }
        public string Period { get; set; }
    }

    public class AiAskResponse
    {
        public bool Success { get; set; }
        public string Answer { get; set; }
        public List<string> SuggestedFollowUps { get; set; } = new();
    }
}

