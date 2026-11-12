using System.Text.Json;
using System.Text.Json.Nodes;

namespace NHIGIA.Modern.Models;

/// <summary>Optional payroll details: missing metadata is unknown, not a zero amount or an assumed rate.</summary>
public sealed class PayrollStoredValues
{
    public static string UpdateManualAmounts(string json, PayrollManualEditRequest request) =>
        UpdateManualAmounts(json, request, 0, DateTime.UtcNow);

    public static string UpdateManualAmounts(string json, PayrollManualEditRequest request, int actorId, DateTime changedAt)
    {
        ArgumentNullException.ThrowIfNull(request);
        var values = string.IsNullOrWhiteSpace(json) ? new JsonObject() :
            JsonNode.Parse(json) as JsonObject ?? throw new JsonException("Payroll metadata must be an object.");
        values["baseSalary"] = request.BaseSalary;
        values["productSalary"] = request.ProductSalary;
        values["salesSalary"] = request.SalesSalary;
        values["kpiSalary"] = request.KpiSalary;
        values["otSalary"] = request.OtSalary;
        values["totalAllowance"] = request.TotalAllowance;
        values["bonus"] = request.Bonus;
        values["socialInsurance"] = request.SocialInsurance;
        values["healthInsurance"] = request.HealthInsurance;
        values["unemploymentInsurance"] = request.UnemploymentInsurance;
        values["personalIncomeTax"] = request.PersonalIncomeTax;
        values["advance"] = request.Advance;
        values["deduction"] = request.Deduction;
        values["unpaidLeaveDeduction"] = 0;
        values["lateDeduction"] = 0;
        values["unionDeduction"] = 0;
        values["phoneDeduction"] = 0;
        values["otherDeduction"] = request.Deduction;
        values["netSalary"] = request.NetSalary;
        values["manualOverride"] = true;
        values["manualOverrideBy"] = actorId;
        values["manualOverrideAt"] = changedAt.ToString("O");
        values["calculationStatus"] = "MANUAL";
        values["calculationNote"] = "Kế toán điều chỉnh thủ công";
        return values.ToJsonString();
    }

    public static string UpdateGeneratedAmounts(string json, PayrollGeneratedValues generated)
    {
        ArgumentNullException.ThrowIfNull(generated);
        var values = string.IsNullOrWhiteSpace(json) ? new JsonObject() :
            JsonNode.Parse(json) as JsonObject ?? throw new JsonException("Payroll metadata must be an object.");
        values["isGenerated"] = true;
        values["calculationVersion"] = "1";
        values["manualOverride"] = false;
        values["generatedAt"] = generated.GeneratedAt.ToString("O");
        values["generatedBy"] = generated.GeneratedBy;
        values["periodFrom"] = generated.PeriodFrom.ToString("yyyy-MM-dd");
        values["periodTo"] = generated.PeriodTo.ToString("yyyy-MM-dd");
        values["calculationNote"] = generated.CalculationNote;
        values["calculationStatus"] = generated.CalculationStatus;
        values["contractBaseSalary"] = generated.ContractBaseSalary;
        values["insuranceSalary"] = generated.InsuranceSalary;
        values["standardWorkDays"] = generated.StandardWorkDays;
        values["workDays"] = generated.WorkDays;
        values["standardMinutes"] = generated.StandardMinutes;
        values["regularWorkedMinutes"] = generated.RegularWorkedMinutes;
        values["salaryEligibleMinutes"] = generated.SalaryEligibleMinutes;
        values["approvedOvertimeMinutes"] = generated.ApprovedOvertimeMinutes;
        values["unpaidLeaveMinutes"] = generated.UnpaidLeaveMinutes;
        values["lateMinutes"] = generated.LateMinutes;
        values["overtimeMultiplier"] = generated.OvertimeMultiplier;
        values["baseSalary"] = generated.WorkSalary;
        values["productSalary"] = generated.ProductSalary;
        values["salesSalary"] = generated.SalesSalary;
        values["kpiSalary"] = generated.KpiSalary;
        values["otSalary"] = generated.OvertimeSalary;
        values["responsibilityAllowance"] = generated.ResponsibilityAllowance;
        values["otherAllowance"] = generated.OtherAllowance;
        values["totalAllowance"] = generated.TotalAllowance;
        values["bonus"] = generated.Bonus;
        values["socialInsurance"] = generated.SocialInsurance;
        values["healthInsurance"] = generated.HealthInsurance;
        values["unemploymentInsurance"] = generated.UnemploymentInsurance;
        values["employerSocialInsurance"] = generated.EmployerSocialInsurance;
        values["employerHealthInsurance"] = generated.EmployerHealthInsurance;
        values["employerUnemploymentInsurance"] = generated.EmployerUnemploymentInsurance;
        values["personalIncomeTax"] = generated.PersonalIncomeTax;
        values["advance"] = generated.Advance;
        values["unpaidLeaveDeduction"] = generated.UnpaidLeaveDeduction;
        values["lateDeduction"] = generated.LateDeduction;
        values["unionDeduction"] = generated.UnionDeduction;
        values["phoneDeduction"] = generated.PhoneDeduction;
        values["otherDeduction"] = generated.OtherDeduction;
        values["deduction"] = generated.TotalOtherDeductions;
        values["netSalary"] = generated.NetSalary;
        return values.ToJsonString();
    }

    public static string UpdateComputedAmounts(string json, decimal allowance, decimal deduction, decimal advance, decimal netSalary)
    {
        // A malformed existing payload must abort the save instead of replacing historical metadata.
        var values = string.IsNullOrWhiteSpace(json) ? new JsonObject() :
            JsonNode.Parse(json) as JsonObject ?? throw new JsonException("Payroll metadata must be an object.");
        values["totalAllowance"] = allowance;
        values["deduction"] = deduction;
        values["advance"] = advance;
        values["netSalary"] = netSalary;
        return values.ToJsonString();
    }

    public decimal? InsuranceSalary { get; init; }
    public decimal? ContractBaseSalary { get; init; }
    public decimal? StandardWorkDays { get; init; }
    public decimal? WorkDays { get; init; }
    public int? StandardMinutes { get; init; }
    public int? RegularWorkedMinutes { get; init; }
    public int? SalaryEligibleMinutes { get; init; }
    public int? ApprovedOvertimeMinutes { get; init; }
    public int? UnpaidLeaveMinutes { get; init; }
    public int? LateMinutes { get; init; }
    public decimal? OvertimeMultiplier { get; init; }
    public decimal? BaseSalary { get; init; }
    public decimal? ProductSalary { get; init; }
    public decimal? SalesSalary { get; init; }
    public decimal? KpiSalary { get; init; }
    public decimal? OtSalary { get; init; }
    public decimal? ResponsibilityAllowance { get; init; }
    public decimal? OtherAllowance { get; init; }
    public decimal? TotalAllowance { get; init; }
    public decimal? Bonus { get; init; }
    public decimal? SocialInsurance { get; init; }
    public decimal? HealthInsurance { get; init; }
    public decimal? UnemploymentInsurance { get; init; }
    public decimal? EmployerSocialInsurance { get; init; }
    public decimal? EmployerHealthInsurance { get; init; }
    public decimal? EmployerUnemploymentInsurance { get; init; }
    public decimal? NonTaxableIncome { get; init; }
    public decimal? PersonalDeduction { get; init; }
    public int? DependentCount { get; init; }
    public decimal? DependentDeduction { get; init; }
    public decimal? InsuranceDeduction { get; init; }
    public decimal? AssessedIncome { get; init; }
    public string TaxRate { get; init; }
    public decimal? PersonalIncomeTax { get; init; }
    public decimal? Advance { get; init; }
    public decimal? UnpaidLeaveDeduction { get; init; }
    public decimal? LateDeduction { get; init; }
    public decimal? UnionDeduction { get; init; }
    public decimal? PhoneDeduction { get; init; }
    public decimal? OtherDeduction { get; init; }
    public decimal? Deduction { get; init; }
    public decimal? NetSalary { get; init; }
    public bool IsGenerated { get; init; }
    public bool ManualOverride { get; init; }
    public string CalculationNote { get; init; }
    public string CalculationStatus { get; init; }

    public static PayrollStoredValues Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return new();
            decimal? Amount(string name) => root.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)
                && number >= 0 ? number : null;
            int? Count(string name) => root.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
                && number >= 0 ? number : null;
            bool Flag(string name) => root.TryGetProperty(name, out var value)
                && value.ValueKind is JsonValueKind.True or JsonValueKind.False && value.GetBoolean();
            return new()
            {
                InsuranceSalary = Amount("insuranceSalary"),
                ContractBaseSalary = Amount("contractBaseSalary"),
                StandardWorkDays = Amount("standardWorkDays"),
                WorkDays = Amount("workDays"),
                StandardMinutes = Count("standardMinutes"),
                RegularWorkedMinutes = Count("regularWorkedMinutes"),
                SalaryEligibleMinutes = Count("salaryEligibleMinutes"),
                ApprovedOvertimeMinutes = Count("approvedOvertimeMinutes"),
                UnpaidLeaveMinutes = Count("unpaidLeaveMinutes"),
                LateMinutes = Count("lateMinutes"),
                OvertimeMultiplier = Amount("overtimeMultiplier"),
                BaseSalary = Amount("baseSalary"),
                ProductSalary = Amount("productSalary"),
                SalesSalary = Amount("salesSalary"),
                KpiSalary = Amount("kpiSalary"),
                OtSalary = Amount("otSalary"),
                ResponsibilityAllowance = Amount("responsibilityAllowance"),
                OtherAllowance = Amount("otherAllowance"),
                TotalAllowance = Amount("totalAllowance"),
                Bonus = Amount("bonus"),
                SocialInsurance = Amount("socialInsurance"),
                HealthInsurance = Amount("healthInsurance"),
                UnemploymentInsurance = Amount("unemploymentInsurance"),
                EmployerSocialInsurance = Amount("employerSocialInsurance"),
                EmployerHealthInsurance = Amount("employerHealthInsurance"),
                EmployerUnemploymentInsurance = Amount("employerUnemploymentInsurance"),
                NonTaxableIncome = Amount("nonTaxableIncome"),
                PersonalDeduction = Amount("personalDeduction"),
                DependentCount = Count("dependentCount"),
                DependentDeduction = Amount("dependentDeduction"),
                InsuranceDeduction = Amount("insuranceDeduction"),
                AssessedIncome = Amount("assessedIncome"),
                TaxRate = root.TryGetProperty("taxRate", out var rate) && rate.ValueKind == JsonValueKind.String
                    ? rate.GetString() : null,
                PersonalIncomeTax = Amount("personalIncomeTax"),
                Advance = Amount("advance"),
                UnpaidLeaveDeduction = Amount("unpaidLeaveDeduction"),
                LateDeduction = Amount("lateDeduction"),
                UnionDeduction = Amount("unionDeduction"),
                PhoneDeduction = Amount("phoneDeduction"),
                OtherDeduction = Amount("otherDeduction"),
                Deduction = Amount("deduction"),
                NetSalary = Amount("netSalary"),
                IsGenerated = Flag("isGenerated"),
                ManualOverride = Flag("manualOverride"),
                CalculationNote = root.TryGetProperty("calculationNote", out var note) && note.ValueKind == JsonValueKind.String
                    ? note.GetString() : null,
                CalculationStatus = root.TryGetProperty("calculationStatus", out var calculationStatus) && calculationStatus.ValueKind == JsonValueKind.String
                    ? calculationStatus.GetString() : null
            };
        }
        catch (JsonException) { return new(); }
    }
}

public sealed class PayrollGeneratedValues
{
    public int GeneratedBy { get; init; }
    public DateTime GeneratedAt { get; init; }
    public DateTime PeriodFrom { get; init; }
    public DateTime PeriodTo { get; init; }
    public string CalculationNote { get; init; }
    public string CalculationStatus { get; init; }
    public decimal ContractBaseSalary { get; init; }
    public decimal InsuranceSalary { get; init; }
    public decimal StandardWorkDays { get; init; }
    public decimal WorkDays { get; init; }
    public int StandardMinutes { get; init; }
    public int RegularWorkedMinutes { get; init; }
    public int SalaryEligibleMinutes { get; init; }
    public int ApprovedOvertimeMinutes { get; init; }
    public int UnpaidLeaveMinutes { get; init; }
    public int LateMinutes { get; init; }
    public decimal OvertimeMultiplier { get; init; }
    public decimal WorkSalary { get; init; }
    public decimal ProductSalary { get; init; }
    public decimal SalesSalary { get; init; }
    public decimal KpiSalary { get; init; }
    public decimal OvertimeSalary { get; init; }
    public decimal ResponsibilityAllowance { get; init; }
    public decimal OtherAllowance { get; init; }
    public decimal TotalAllowance { get; init; }
    public decimal Bonus { get; init; }
    public decimal SocialInsurance { get; init; }
    public decimal HealthInsurance { get; init; }
    public decimal UnemploymentInsurance { get; init; }
    public decimal EmployerSocialInsurance { get; init; }
    public decimal EmployerHealthInsurance { get; init; }
    public decimal EmployerUnemploymentInsurance { get; init; }
    public decimal PersonalIncomeTax { get; init; }
    public decimal Advance { get; init; }
    public decimal UnpaidLeaveDeduction { get; init; }
    public decimal LateDeduction { get; init; }
    public decimal UnionDeduction { get; init; }
    public decimal PhoneDeduction { get; init; }
    public decimal OtherDeduction { get; init; }
    public decimal TotalOtherDeductions { get; init; }
    public decimal NetSalary { get; init; }
}
