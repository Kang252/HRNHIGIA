using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models;

public sealed class WorkItem
{
    public int Id { get; set; }
    [Required] public string Kind { get; set; }
    [Required, StringLength(200)] public string Title { get; set; }
    [StringLength(2000)] public string Description { get; set; }
    [StringLength(100)] public string Category { get; set; }
    [StringLength(100)] public string Reference { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    [StringLength(250)] public string Location { get; set; }
    [StringLength(250)] public string Destination { get; set; }
    [Range(0, 1000000000)] public decimal? Target { get; set; }
    [Range(0, 1000000000)] public decimal? Actual { get; set; }
    [Range(1, 100)] public decimal? Weight { get; set; }
    public int AssetInUse { get; set; }
    public int AssetMaintenance { get; set; }
    public int AssetLost { get; set; }
    public int AssetDisposed { get; set; }
    public int AssetDamaged { get; set; }
    [StringLength(20)] public string Priority { get; set; } = "NORMAL";
    public string Status { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string LastActionNote { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public string ParticipantNames { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
    [StringLength(250)] public string WorkLocation { get; set; }
    [StringLength(100)] public string JobLevel { get; set; }
    [StringLength(100)] public string SalaryRange { get; set; }
    [StringLength(150)] public string ContactEmail { get; set; }
    [StringLength(2000)] public string Keywords { get; set; }
    public DateTime? StartDate { get; set; }
    public string Quarter { get; set; }

    public string RecordCode => FormatCode(Kind, Id);

    public static string FormatCode(string kind, int id)
    {
        var prefix = kind?.ToLowerInvariant() switch
        {
            "kpi" => "KPI", "payroll" => "BL", "recruitment" => "TD",
            "training" => "DT", "overtime" => "TC", "resignation" => "NV",
            "transfer" => "DC", "assets" => "TS", "helpdesk" => "HD",
            "vehicle" => "XE", "meeting" => "PH", "business-trip" => "CT", "offboarding" => "TV",
            _ => "HS"
        };
        return $"{prefix}{id:D8}";
    }
}

public sealed class WorkPage
{
    public string Kind { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public bool CanCreate { get; set; }
    public bool CanManage { get; set; }
    public bool Available { get; set; }
    public string Query { get; set; }
    public int? EditId { get; set; }
    public WorkItem Draft { get; set; } = new();
    public WorkItem OffboardingDraft { get; set; } = new();
    public string NextOffboardingReference { get; set; }
    public List<WorkItem> Items { get; set; } = new();
    public List<WorkPerson> People { get; set; } = new();
    public List<WorkDepartment> Departments { get; set; } = new();

    // Payroll Management
    public string PayrollTab { get; set; } = "dashboard";
    public string PayrollPeriod { get; set; } = "2026-02";
    public string PayrollDeptFilter { get; set; } = "ALL";
    public string PayrollStatusFilter { get; set; } = "ALL";
    public int TotalPayrollEmployees { get; set; }
    public decimal TotalPayrollNetSalary { get; set; }
    public decimal TotalPayrollSocialInsurance { get; set; }
    public decimal TotalPayrollPersonalIncomeTax { get; set; }
    public List<PayrollDepartmentStat> PayrollDepartmentStats { get; set; } = new();
    public List<PayrollItemDetail> PayrollItems { get; set; } = new();
    public List<PayrollAllowanceItem> PayrollAllowances { get; set; } = new();
    public List<PayrollInsuranceItem> PayrollInsurances { get; set; } = new();
    public List<PayrollTaxItem> PayrollTaxes { get; set; } = new();
    public List<PayrollDeductionItem> PayrollDeductions { get; set; } = new();
    public List<PayrollAdvanceItem> PayrollAdvances { get; set; } = new();
}
public sealed class WorkPerson { public int Id { get; set; } public string DisplayName { get; set; } public string DepartmentName { get; set; } }
public sealed class WorkDepartment { public int Id { get; set; } public string Name { get; set; } }

public sealed class PayrollDepartmentStat
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; }
    public int BarHeightPercent { get; set; }
}

public sealed class PayrollItemDetail
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public string EmployeeEmail { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string Branch { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal KpiSalary { get; set; }
    public decimal SalesSalary { get; set; }
    public decimal OtSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossIncome => BaseSalary + KpiSalary + SalesSalary + OtSalary + TotalAllowance + Bonus;
    public decimal SocialInsurance { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal UnemploymentInsurance { get; set; }
    public decimal TotalInsurance => SocialInsurance + HealthInsurance + UnemploymentInsurance;
    public decimal PersonalIncomeTax { get; set; }
    public decimal Advance { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string StatusText { get; set; }
    public string StatusBadge { get; set; }
    public string LastActionNote { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class PayrollAllowanceItem
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public string AllowanceType { get; set; }
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsInsuranceSubject { get; set; }
    public string Note { get; set; }
}

public sealed class PayrollInsuranceItem
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public decimal InsuranceSalary { get; set; }
    public decimal BhxhEmp { get; set; }
    public decimal BhytEmp { get; set; }
    public decimal BhtnEmp { get; set; }
    public decimal TotalEmp => BhxhEmp + BhytEmp + BhtnEmp;
    public decimal BhxhComp { get; set; }
    public decimal BhytComp { get; set; }
    public decimal BhtnComp { get; set; }
    public decimal TotalComp => BhxhComp + BhytComp + BhtnComp;
    public decimal TotalContribution => TotalEmp + TotalComp;
}

public sealed class PayrollTaxItem
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal NonTaxableIncome { get; set; }
    public decimal TaxableIncome => Math.Max(0, TotalIncome - NonTaxableIncome);
    public decimal PersonalDeduction { get; set; } = 11000000;
    public int DependentCount { get; set; }
    public decimal DependentDeduction => DependentCount * 4400000;
    public decimal InsuranceDeduction { get; set; }
    public decimal AssessedIncome { get; set; }
    public string TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class PayrollDeductionItem
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public string DeductionType { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class PayrollAdvanceItem
{
    public int Id { get; set; }
    public string Period { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public decimal Amount { get; set; }
    public DateTime AdvanceDate { get; set; }
    public string Reason { get; set; }
    public string RepaymentPeriod { get; set; }
    public string Status { get; set; } = "APPROVED";
}

public sealed class WorkNotification
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
