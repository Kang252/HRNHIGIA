using System.Text.Json;
using System.Text.Json.Nodes;

namespace NHIGIA.Modern.Models;

/// <summary>Optional payroll details: missing metadata is unknown, not a zero amount or an assumed rate.</summary>
public sealed class PayrollStoredValues
{
    public static string UpdateManualAmounts(string json, PayrollManualEditRequest request)
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
        values["netSalary"] = request.NetSalary;
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
    public decimal? BaseSalary { get; init; }
    public decimal? ProductSalary { get; init; }
    public decimal? SalesSalary { get; init; }
    public decimal? KpiSalary { get; init; }
    public decimal? OtSalary { get; init; }
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
    public decimal? Deduction { get; init; }
    public decimal? NetSalary { get; init; }

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
            return new()
            {
                InsuranceSalary = Amount("insuranceSalary"),
                BaseSalary = Amount("baseSalary"),
                ProductSalary = Amount("productSalary"),
                SalesSalary = Amount("salesSalary"),
                KpiSalary = Amount("kpiSalary"),
                OtSalary = Amount("otSalary"),
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
                Deduction = Amount("deduction"),
                NetSalary = Amount("netSalary")
            };
        }
        catch (JsonException) { return new(); }
    }
}
