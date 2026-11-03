namespace NHIGIA.Modern.Models;

public sealed class HrReportSummaryModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int EmployeeCount { get; set; }
    public int RecordedWorkdays { get; set; }
    public int WorkedMinutes { get; set; }
    public decimal OnLeaveDays { get; set; }
    public int LateOrEarlyCount { get; set; }
    public int MissingAttendanceCount { get; set; }
    public int PendingLeaveCount { get; set; }
    public int RecruitmentCandidateCount { get; set; }
    public int CompletedResignationCount { get; set; }
    public List<EmploymentStatusStatModel> EmploymentStatuses { get; set; } = new();
}

public sealed class EmploymentStatusStatModel
{
    public string Status { get; set; }
    public int Count { get; set; }
}
