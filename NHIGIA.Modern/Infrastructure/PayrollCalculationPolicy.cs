using System.Globalization;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>
/// Pure payroll calculations. Attendance and approved leave are converted to
/// minutes before entering this policy so each minute is paid or deducted once.
/// </summary>
public static class PayrollCalculationPolicy
{
    public const int DefaultStandardDayMinutes = 480;

    public static PayrollCalculationResult Calculate(PayrollCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.BaseSalary < 0 || input.InsuranceSalary < 0 || input.StandardMinutes < 0 ||
            input.SalaryEligibleMinutes < 0 || input.ApprovedOvertimeMinutes < 0 ||
            input.UnpaidLeaveMinutes < 0 || input.LateMinutes < 0 || input.OvertimeMultiplier < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "Dữ liệu tính lương không được âm.");

        var standardMinutes = input.StandardMinutes;
        var eligibleMinutes = standardMinutes > 0
            ? Math.Min(standardMinutes, input.SalaryEligibleMinutes)
            : 0;
        var workSalary = standardMinutes > 0
            ? RoundMoney(input.BaseSalary * eligibleMinutes / standardMinutes)
            : 0;
        var overtimeSalary = standardMinutes > 0
            ? RoundMoney(input.BaseSalary * input.ApprovedOvertimeMinutes * input.OvertimeMultiplier / standardMinutes)
            : 0;
        var unpaidLeaveDeduction = standardMinutes > 0
            ? RoundMoney(input.BaseSalary * Math.Min(standardMinutes, input.UnpaidLeaveMinutes) / standardMinutes)
            : 0;
        var lateDeduction = standardMinutes > 0
            ? RoundMoney(input.BaseSalary * Math.Min(standardMinutes, input.LateMinutes) / standardMinutes)
            : 0;

        var socialInsurance = RoundMoney(input.InsuranceSalary * 0.08m);
        var healthInsurance = RoundMoney(input.InsuranceSalary * 0.015m);
        var unemploymentInsurance = RoundMoney(input.InsuranceSalary * 0.01m);
        var employerSocialInsurance = RoundMoney(input.InsuranceSalary * 0.175m);
        var employerHealthInsurance = RoundMoney(input.InsuranceSalary * 0.03m);
        var employerUnemploymentInsurance = RoundMoney(input.InsuranceSalary * 0.01m);

        var totalAllowance = NonNegative(input.ResponsibilityAllowance) + NonNegative(input.OtherAllowance);
        var grossIncome = workSalary + NonNegative(input.ProductSalary) + NonNegative(input.SalesSalary) +
            NonNegative(input.KpiSalary) + overtimeSalary + totalAllowance + NonNegative(input.Bonus);
        var otherDeductions = unpaidLeaveDeduction + lateDeduction + NonNegative(input.UnionDeduction) +
            NonNegative(input.PhoneDeduction) + NonNegative(input.OtherDeduction);
        var totalInsurance = socialInsurance + healthInsurance + unemploymentInsurance;
        var netSalary = Math.Max(0, grossIncome - totalInsurance - NonNegative(input.PersonalIncomeTax) -
            NonNegative(input.Advance) - otherDeductions);
        var workDays = standardMinutes > 0
            ? Math.Round(input.StandardWorkDays * eligibleMinutes / standardMinutes, 2, MidpointRounding.AwayFromZero)
            : 0;

        return new PayrollCalculationResult
        {
            WorkDays = workDays,
            WorkSalary = workSalary,
            OvertimeSalary = overtimeSalary,
            UnpaidLeaveDeduction = unpaidLeaveDeduction,
            LateDeduction = lateDeduction,
            SocialInsurance = socialInsurance,
            HealthInsurance = healthInsurance,
            UnemploymentInsurance = unemploymentInsurance,
            EmployerSocialInsurance = employerSocialInsurance,
            EmployerHealthInsurance = employerHealthInsurance,
            EmployerUnemploymentInsurance = employerUnemploymentInsurance,
            TotalAllowance = totalAllowance,
            GrossIncome = grossIncome,
            OtherDeductions = otherDeductions,
            NetSalary = RoundMoney(netSalary)
        };
    }

    public static decimal ParseMoney(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var input = value.Trim().ToLowerInvariant().Replace("vnd", string.Empty).Replace("đ", string.Empty).Trim();
        if (input.Contains('-') || input.Any(char.IsLetter)) return 0;
        var normalized = new string(input.Where(char.IsDigit).ToArray());
        return decimal.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : 0;
    }

    public static int ScheduledMinutes(TimeSpan startTime, TimeSpan endTime, int configuredBreakMinutes, DateTime workDate)
    {
        var start = workDate.Date.Add(startTime);
        var end = workDate.Date.Add(endTime);
        if (end <= start) end = end.AddDays(1);
        var total = (int)(end - start).TotalMinutes;

        if (end.Date == start.Date)
        {
            var lunchStart = workDate.Date.AddHours(12);
            var lunchEnd = workDate.Date.AddHours(13.5);
            var overlapStart = start > lunchStart ? start : lunchStart;
            var overlapEnd = end < lunchEnd ? end : lunchEnd;
            if (overlapEnd > overlapStart) total -= (int)(overlapEnd - overlapStart).TotalMinutes;
        }
        else
        {
            total -= Math.Max(0, configuredBreakMinutes);
        }

        return Math.Max(0, total);
    }

    public static decimal RoundMoney(decimal value) =>
        Math.Round(Math.Max(0, value), 0, MidpointRounding.AwayFromZero);

    private static decimal NonNegative(decimal value) => Math.Max(0, value);
}

public sealed class PayrollCalculationInput
{
    public decimal BaseSalary { get; init; }
    public decimal InsuranceSalary { get; init; }
    public int StandardMinutes { get; init; }
    public decimal StandardWorkDays { get; init; }
    public int SalaryEligibleMinutes { get; init; }
    public int ApprovedOvertimeMinutes { get; init; }
    public int UnpaidLeaveMinutes { get; init; }
    public int LateMinutes { get; init; }
    public decimal OvertimeMultiplier { get; init; } = 1.5m;
    public decimal ResponsibilityAllowance { get; init; }
    public decimal OtherAllowance { get; init; }
    public decimal ProductSalary { get; init; }
    public decimal SalesSalary { get; init; }
    public decimal KpiSalary { get; init; }
    public decimal Bonus { get; init; }
    public decimal PersonalIncomeTax { get; init; }
    public decimal Advance { get; init; }
    public decimal UnionDeduction { get; init; }
    public decimal PhoneDeduction { get; init; }
    public decimal OtherDeduction { get; init; }
}

public sealed class PayrollCalculationResult
{
    public decimal WorkDays { get; init; }
    public decimal WorkSalary { get; init; }
    public decimal OvertimeSalary { get; init; }
    public decimal UnpaidLeaveDeduction { get; init; }
    public decimal LateDeduction { get; init; }
    public decimal SocialInsurance { get; init; }
    public decimal HealthInsurance { get; init; }
    public decimal UnemploymentInsurance { get; init; }
    public decimal EmployerSocialInsurance { get; init; }
    public decimal EmployerHealthInsurance { get; init; }
    public decimal EmployerUnemploymentInsurance { get; init; }
    public decimal TotalAllowance { get; init; }
    public decimal GrossIncome { get; init; }
    public decimal OtherDeductions { get; init; }
    public decimal NetSalary { get; init; }
}
