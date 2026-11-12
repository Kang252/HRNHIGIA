using ClosedXML.Excel;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

var tests = new List<(string Name, Action Run)>
{
    ("Payroll period is previous 25th through current 24th", () =>
    {
        var range = AttendancePeriodRange.Parse("2026-10");
        Equal(new DateTime(2026, 9, 25), range.FromDate);
        Equal(new DateTime(2026, 10, 24), range.ToDate);
        var yearBoundary = AttendancePeriodRange.Parse("2027-01");
        Equal(new DateTime(2026, 12, 25), yearBoundary.FromDate);
        Equal(new DateTime(2027, 1, 24), yearBoundary.ToDate);
    }),
    ("Invalid payroll period is rejected", () =>
    {
        Throws<InvalidOperationException>(() => AttendancePeriodRange.Parse("10-2026"));
    }),
    ("Company lunch 12:00-13:30 is removed once", () =>
    {
        Equal(480, PayrollCalculationPolicy.ScheduledMinutes(TimeSpan.FromHours(8), TimeSpan.FromHours(17.5), 60,
            new DateTime(2026, 10, 1)));
    }),
    ("Salary text accepts VND formatting but rejects ambiguous text", () =>
    {
        Equal(15_000_000m, PayrollCalculationPolicy.ParseMoney("15.000.000 đ"));
        Equal(0m, PayrollCalculationPolicy.ParseMoney("15,5 triệu"));
        Equal(0m, PayrollCalculationPolicy.ParseMoney("-1000"));
    }),
    ("Golden payroll fixture produces expected net", () =>
    {
        var result = PayrollCalculationPolicy.Calculate(new PayrollCalculationInput
        {
            BaseSalary = 24_000_000,
            InsuranceSalary = 15_000_000,
            StandardMinutes = 9_600,
            StandardWorkDays = 20,
            SalaryEligibleMinutes = 9_120,
            ApprovedOvertimeMinutes = 180,
            ResponsibilityAllowance = 1_000_000,
            OtherDeduction = 200_000,
            Advance = 500_000,
            OvertimeMultiplier = 1.5m
        });
        Equal(22_800_000m, result.WorkSalary);
        Equal(675_000m, result.OvertimeSalary);
        Equal(1_575_000m, result.SocialInsurance + result.HealthInsurance + result.UnemploymentInsurance);
        Equal(24_475_000m, result.GrossIncome);
        Equal(22_200_000m, result.NetSalary);
    }),
    ("Unpaid leave and lateness are deducted exactly once", () =>
    {
        var result = PayrollCalculationPolicy.Calculate(new PayrollCalculationInput
        {
            BaseSalary = 24_000_000,
            StandardMinutes = 9_600,
            StandardWorkDays = 20,
            SalaryEligibleMinutes = 9_600,
            UnpaidLeaveMinutes = 480,
            LateMinutes = 30
        });
        Equal(24_000_000m, result.WorkSalary);
        Equal(1_200_000m, result.UnpaidLeaveDeduction);
        Equal(75_000m, result.LateDeduction);
        Equal(22_725_000m, result.NetSalary);
    }),
    ("Only approved overtime intersection is separated from regular time", () =>
    {
        var day = new DateTime(2026, 10, 5);
        var row = new AttendanceRecordModel
        {
            UserId = 7,
            WorkDate = day,
            ScheduledStart = TimeSpan.FromHours(8),
            ScheduledEnd = TimeSpan.FromHours(17.5),
            ExpectedStartAt = day.AddHours(8),
            ExpectedEndAt = day.AddHours(17.5),
            CheckIn = day.AddHours(7.5),
            CheckOut = day.AddHours(21)
        };
        var approved = new ApprovedOvertimeModel
        {
            UserId = 7, WorkDate = day, StartAt = day.AddHours(18), EndAt = day.AddHours(21), StatusCode = "APPROVED"
        };
        var pending = new ApprovedOvertimeModel
        {
            UserId = 7, WorkDate = day, StartAt = day.AddHours(17.5), EndAt = day.AddHours(18), StatusCode = "PENDING"
        };
        Equal(180, AttendanceLeavePolicy.CalculateApprovedOvertimeMinutes(row, [approved, pending]));
    }),
    ("Manual override preserves unrelated metadata", () =>
    {
        var json = PayrollStoredValues.UpdateManualAmounts("{\"custom\":\"keep\",\"isGenerated\":true}",
            new PayrollManualEditRequest { Id = 3, BaseSalary = 10, Deduction = 2 }, 9,
            new DateTime(2026, 10, 26, 9, 0, 0));
        True(json.Contains("\"custom\":\"keep\""), "custom metadata");
        var values = PayrollStoredValues.Parse(json);
        True(values.ManualOverride, "manual override flag");
        Equal(8m, values.NetSalary ?? -1);
        Equal(0m, values.UnpaidLeaveDeduction ?? -1);
        Equal(0m, values.LateDeduction ?? -1);
        Equal(2m, values.OtherDeduction ?? -1);
    }),
    ("XLSX export matches the 22-column salary template", () =>
    {
        var item = new PayrollItemDetail
        {
            EmployeeCode = "NV001", EmployeeName = "Nhân viên A", JobTitle = "Chuyên viên",
            HireDate = new DateTime(2025, 1, 2), DepartmentName = "Phòng A", InsuranceSalary = 7_000_000,
            ContractBaseSalary = 10_000_000, StandardWorkDays = 20, WorkDays = 20, BaseSalary = 10_000_000,
            ResponsibilityAllowance = 500_000, OtherAllowance = 300_000, ApprovedOvertimeMinutes = 120,
            OtSalary = 375_000, SocialInsurance = 560_000, HealthInsurance = 105_000,
            UnemploymentInsurance = 70_000, NetSalary = 10_375_000
        };
        var bytes = PayrollWorkbookExporter.Build([item], "2026-10");
        True(bytes.Length > 4 && bytes[0] == (byte)'P' && bytes[1] == (byte)'K', "xlsx zip signature");
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheet(1);
        Equal("STT", sheet.Cell(5, 1).GetString());
        Equal("Thực lãnh", sheet.Cell(5, 22).GetString());
        True(sheet.CellsUsed().All(x => !x.GetString().Contains("#REF!", StringComparison.OrdinalIgnoreCase)), "no broken references");
        var totalRow = sheet.Column(3).CellsUsed().Single(x => x.GetString() == "TỔNG DỮ LIỆU XUẤT").Address.RowNumber;
        Equal(10_375_000d, sheet.Cell(totalRow, 22).GetDouble());
    }),
    ("Manual payroll export does not repeat stale automatic deductions", () =>
    {
        var item = new PayrollItemDetail
        {
            EmployeeCode = "NV002", EmployeeName = "Nhân viên B", DepartmentName = "Phòng B",
            BaseSalary = 10_000_000, UnpaidLeaveDeduction = 500_000, LateDeduction = 100_000,
            UnionDeduction = 50_000, PhoneDeduction = 25_000, Deduction = 200_000,
            PersonalIncomeTax = 300_000, Advance = 1_000_000, NetSalary = 8_500_000,
            StoredValues = PayrollStoredValues.Parse("{\"manualOverride\":true}")
        };
        using var workbook = new XLWorkbook(new MemoryStream(PayrollWorkbookExporter.Build([item], "2026-10")));
        var sheet = workbook.Worksheet(1);
        var row = sheet.Column(2).CellsUsed().Single(x => x.GetString() == "NV002").Address.RowNumber;
        Equal(0d, sheet.Cell(row, 15).GetDouble());
        Equal(0d, sheet.Cell(row, 17).GetDouble());
        Equal(0d, sheet.Cell(row, 19).GetDouble());
        Equal(0d, sheet.Cell(row, 20).GetDouble());
        Equal(1_500_000d, sheet.Cell(row, 21).GetDouble());
    })
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures.Add($"FAIL {test.Name}: {ex.Message}");
        Console.Error.WriteLine(failures[^1]);
    }
}

if (failures.Count > 0) Environment.Exit(1);
Console.WriteLine($"Payroll tests passed: {tests.Count}");

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}");
}

static void True(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException(label);
}

static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
