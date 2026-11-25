using System.Text.RegularExpressions;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>Format checks for a profile form. A value that was already stored and is not being changed never blocks saving.</summary>
public static partial class EmployeeProfilePolicy
{
    public static IEnumerable<(string Field, string Message)> Validate(EmployeeProfileModel input, EmployeeProfileModel stored, DateTime today)
    {
        bool Changed(string value, string previous) => !string.IsNullOrWhiteSpace(value) && value.Trim() != previous?.Trim();

        if (Changed(input.IdentityNumber, stored?.IdentityNumber) && !Identity().IsMatch(Compact(input.IdentityNumber)))
            yield return (nameof(input.IdentityNumber), "Số CCCD/CMND phải gồm 12 hoặc 9 chữ số.");
        foreach (var (field, label, value, previous) in new[]
        {
            (nameof(input.MobilePhone), "Điện thoại di động", input.MobilePhone, stored?.MobilePhone),
            (nameof(input.OfficePhone), "Điện thoại cơ quan", input.OfficePhone, stored?.OfficePhone),
            (nameof(input.HomePhone), "Điện thoại nhà", input.HomePhone, stored?.HomePhone),
            (nameof(input.EmergencyContactPhone), "Số điện thoại liên hệ khẩn cấp", input.EmergencyContactPhone, stored?.EmergencyContactPhone)
        })
            if (Changed(value, previous) && !IsPhone(value)) yield return (field, $"{label} không hợp lệ (8–15 chữ số).");
        if (Changed(input.PersonalTaxCode, stored?.PersonalTaxCode) && !TaxCode().IsMatch(Compact(input.PersonalTaxCode)))
            yield return (nameof(input.PersonalTaxCode), "Mã số thuế phải gồm 10 chữ số (có thể kèm -XXX) hoặc 12 chữ số.");
        if (Changed(input.SocialInsuranceNumber, stored?.SocialInsuranceNumber) && !SocialInsurance().IsMatch(Compact(input.SocialInsuranceNumber)))
            yield return (nameof(input.SocialInsuranceNumber), "Số sổ BHXH phải gồm 10 chữ số.");
        if (Changed(input.HealthInsuranceNumber, stored?.HealthInsuranceNumber) && !HealthInsurance().IsMatch(Compact(input.HealthInsuranceNumber).ToUpperInvariant()))
            yield return (nameof(input.HealthInsuranceNumber), "Số thẻ BHYT gồm 10 hoặc 15 ký tự chữ và số.");
        if (Changed(input.BankAccountNumber, stored?.BankAccountNumber) && !BankAccount().IsMatch(Compact(input.BankAccountNumber)))
            yield return (nameof(input.BankAccountNumber), "Số tài khoản ngân hàng gồm 6–20 chữ số.");
        if (Changed(input.BasicSalary, stored?.BasicSalary) && !IsMoney(input.BasicSalary))
            yield return (nameof(input.BasicSalary), "Mức lương cơ bản phải là số tiền, ví dụ 15.000.000.");
        if (Changed(input.InsuranceSalary, stored?.InsuranceSalary) && !IsMoney(input.InsuranceSalary))
            yield return (nameof(input.InsuranceSalary), "Mức lương đóng BHXH phải là số tiền, ví dụ 15.000.000.");
        if (input.DateOfBirth.HasValue && input.DateOfBirth != stored?.DateOfBirth &&
            (input.DateOfBirth > today.AddYears(-15) || input.DateOfBirth < today.AddYears(-100)))
            yield return (nameof(input.DateOfBirth), "Ngày sinh không hợp lệ (nhân viên phải từ 15 tuổi).");
        if (input.HireDate.HasValue && input.HireDate != stored?.HireDate && input.HireDate > today.AddYears(1))
            yield return (nameof(input.HireDate), "Ngày vào làm không được quá một năm trong tương lai.");
    }

    public static bool IsPhone(string value) =>
        PhoneCharacters().IsMatch(value?.Trim() ?? "") && value.Count(char.IsDigit) is >= 8 and <= 15;

    public static bool IsMoney(string value) =>
        MoneyCharacters().IsMatch(value?.Trim().ToLowerInvariant() ?? "") && PayrollCalculationPolicy.ParseMoney(value) > 0;

    private static string Compact(string value) => (value ?? "").Replace(" ", "").Replace(".", "");

    [GeneratedRegex(@"^(\d{9}|\d{12})$")] private static partial Regex Identity();
    [GeneratedRegex(@"^(\d{10}(-\d{3})?|\d{12})$")] private static partial Regex TaxCode();
    [GeneratedRegex(@"^\d{10}$")] private static partial Regex SocialInsurance();
    [GeneratedRegex(@"^([A-Z0-9]{10}|[A-Z0-9]{15})$")] private static partial Regex HealthInsurance();
    [GeneratedRegex(@"^\d{6,20}$")] private static partial Regex BankAccount();
    [GeneratedRegex(@"^\+?[\d\s().-]+$")] private static partial Regex PhoneCharacters();
    [GeneratedRegex(@"^[\d.,\s]+(đ|vnd)?$")] private static partial Regex MoneyCharacters();
}
