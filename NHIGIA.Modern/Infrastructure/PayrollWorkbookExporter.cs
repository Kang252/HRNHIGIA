using ClosedXML.Excel;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class PayrollWorkbookExporter
{
    private static readonly string[] Headers =
    [
        "STT", "Mã NV", "Họ tên", "Chức vụ", "Ngày vào làm", "Lương đóng BHXH",
        "Lương căn bản", "Ngày công", "Lương ngày công", "Phụ cấp trách nhiệm",
        "Tổng lương", "Số giờ tăng ca", "Số tiền tăng ca", "Trợ cấp khác",
        "Số tiền ngày nghỉ không lương", "Số giờ nghỉ không lương", "Trừ tiền đi trễ",
        "Trừ BHXH", "Trừ công đoàn", "Trừ tiền ĐT", "Trừ khác", "Thực lãnh"
    ];

    public static byte[] Build(IReadOnlyCollection<PayrollItemDetail> items, string period)
    {
        var range = AttendancePeriodRange.Parse(period);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add($"Luong {period}");
        sheet.SheetView.FreezeRows(5);
        sheet.SheetView.FreezeColumns(3);

        sheet.Range(1, 1, 1, Headers.Length).Merge().Value = "CÔNG TY NHỊ GIA";
        sheet.Range(2, 1, 2, Headers.Length).Merge().Value = $"BẢNG LƯƠNG KỲ {period}";
        sheet.Range(3, 1, 3, Headers.Length).Merge().Value =
            $"Dữ liệu chấm công từ {range.FromDate:dd/MM/yyyy} đến {range.ToDate:dd/MM/yyyy} · Chỉ tính OT đã duyệt";

        var title = sheet.Range(1, 1, 2, Headers.Length);
        title.Style.Font.Bold = true;
        title.Style.Font.FontColor = XLColor.White;
        title.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D4ED8");
        title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        title.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Row(1).Height = 26;
        sheet.Row(2).Height = 24;
        sheet.Range(3, 1, 3, Headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
        sheet.Range(3, 1, 3, Headers.Length).Style.Font.FontColor = XLColor.FromHtml("#1E3A8A");
        sheet.Range(3, 1, 3, Headers.Length).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        for (var column = 1; column <= Headers.Length; column++)
            sheet.Cell(5, column).Value = Headers[column - 1];
        var header = sheet.Range(5, 1, 5, Headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Alignment.WrapText = true;
        sheet.Row(5).Height = 34;

        var row = 6;
        var sequence = 1;
        foreach (var department in items.OrderBy(x => x.DepartmentName).ThenBy(x => x.EmployeeName)
                     .GroupBy(x => string.IsNullOrWhiteSpace(x.DepartmentName) ? "Chưa phân phòng" : x.DepartmentName))
        {
            sheet.Range(row, 1, row, Headers.Length).Merge().Value = department.Key;
            sheet.Range(row, 1, row, Headers.Length).Style.Font.Bold = true;
            sheet.Range(row, 1, row, Headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
            row++;
            var firstDataRow = row;

            foreach (var item in department)
            {
                var manualOverride = item.StoredValues.ManualOverride;
                var c = 1;
                sheet.Cell(row, c++).Value = sequence++;
                sheet.Cell(row, c++).Value = item.EmployeeCode ?? string.Empty;
                sheet.Cell(row, c++).Value = item.EmployeeName ?? string.Empty;
                sheet.Cell(row, c++).Value = item.JobTitle ?? string.Empty;
                if (item.HireDate.HasValue) sheet.Cell(row, c).Value = item.HireDate.Value;
                c++;
                sheet.Cell(row, c++).Value = item.InsuranceSalary;
                sheet.Cell(row, c++).Value = item.ContractBaseSalary;
                sheet.Cell(row, c++).Value = item.WorkDays;
                sheet.Cell(row, c++).Value = item.BaseSalary;
                sheet.Cell(row, c++).Value = item.ResponsibilityAllowance;
                sheet.Cell(row, c++).Value = item.BaseSalary + item.ResponsibilityAllowance;
                sheet.Cell(row, c++).Value = item.OvertimeHours;
                sheet.Cell(row, c++).Value = item.OtSalary;
                sheet.Cell(row, c++).Value = item.OtherAllowance + item.ProductSalary + item.SalesSalary + item.KpiSalary + item.Bonus;
                sheet.Cell(row, c++).Value = manualOverride ? 0 : item.UnpaidLeaveDeduction;
                sheet.Cell(row, c++).Value = item.UnpaidLeaveHours;
                sheet.Cell(row, c++).Value = manualOverride ? 0 : item.LateDeduction;
                sheet.Cell(row, c++).Value = item.TotalInsurance;
                sheet.Cell(row, c++).Value = manualOverride ? 0 : item.UnionDeduction;
                sheet.Cell(row, c++).Value = manualOverride ? 0 : item.PhoneDeduction;
                sheet.Cell(row, c++).Value = item.ExportOtherDeduction + item.PersonalIncomeTax + item.Advance;
                sheet.Cell(row, c).Value = item.NetSalary;
                row++;
            }

            var subtotalRow = row++;
            sheet.Cell(subtotalRow, 3).Value = $"Tổng {department.Key}";
            sheet.Cell(subtotalRow, 3).Style.Font.Bold = true;
            for (var column = 6; column <= Headers.Length; column++)
            {
                if (column is 8 or 12 or 16) continue;
                sheet.Cell(subtotalRow, column).Value = department.Sum(x => ExportValue(x, column));
            }
            sheet.Cell(subtotalRow, 8).Value = department.Sum(x => x.WorkDays);
            sheet.Cell(subtotalRow, 12).Value = department.Sum(x => x.OvertimeHours);
            sheet.Cell(subtotalRow, 16).Value = department.Sum(x => x.UnpaidLeaveHours);
            sheet.Range(subtotalRow, 1, subtotalRow, Headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            sheet.Range(subtotalRow, 1, subtotalRow, Headers.Length).Style.Font.Bold = true;
            if (row == firstDataRow) row++;
        }

        var totalRow = row;
        sheet.Cell(totalRow, 3).Value = "TỔNG DỮ LIỆU XUẤT";
        sheet.Range(totalRow, 1, totalRow, Headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#BFDBFE");
        sheet.Range(totalRow, 1, totalRow, Headers.Length).Style.Font.Bold = true;
        for (var column = 6; column <= Headers.Length; column++)
        {
            sheet.Cell(totalRow, column).Value = column switch
            {
                8 => items.Sum(x => x.WorkDays),
                12 => items.Sum(x => x.OvertimeHours),
                16 => items.Sum(x => x.UnpaidLeaveHours),
                _ => items.Sum(x => ExportValue(x, column))
            };
        }

        if (items.Count > 0) sheet.Range(5, 1, Math.Max(5, totalRow - 1), Headers.Length).SetAutoFilter();
        sheet.Range(5, 1, totalRow, Headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(5, 1, totalRow, Headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        sheet.Range(6, 5, totalRow, 5).Style.DateFormat.Format = "dd/MM/yyyy";
        sheet.Range(6, 6, totalRow, Headers.Length).Style.NumberFormat.Format = "#,##0";
        sheet.Range(6, 8, totalRow, 8).Style.NumberFormat.Format = "0.00";
        sheet.Range(6, 12, totalRow, 12).Style.NumberFormat.Format = "0.00";
        sheet.Range(6, 16, totalRow, 16).Style.NumberFormat.Format = "0.00";
        sheet.Columns(1, Headers.Length).AdjustToContents(5, Math.Max(5, totalRow));
        sheet.Column(1).Width = 6;
        sheet.Column(2).Width = Math.Max(sheet.Column(2).Width, 14);
        sheet.Column(3).Width = Math.Max(sheet.Column(3).Width, 24);
        sheet.Column(4).Width = Math.Max(sheet.Column(4).Width, 20);
        for (var column = 6; column <= Headers.Length; column++)
            sheet.Column(column).Width = Math.Max(sheet.Column(column).Width, 14);
        sheet.Range(1, 1, totalRow, Headers.Length).Style.Font.FontName = "Arial";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static decimal ExportValue(PayrollItemDetail item, int column) => column switch
    {
        6 => item.InsuranceSalary,
        7 => item.ContractBaseSalary,
        9 => item.BaseSalary,
        10 => item.ResponsibilityAllowance,
        11 => item.BaseSalary + item.ResponsibilityAllowance,
        13 => item.OtSalary,
        14 => item.OtherAllowance + item.ProductSalary + item.SalesSalary + item.KpiSalary + item.Bonus,
        15 => item.StoredValues.ManualOverride ? 0 : item.UnpaidLeaveDeduction,
        17 => item.StoredValues.ManualOverride ? 0 : item.LateDeduction,
        18 => item.TotalInsurance,
        19 => item.StoredValues.ManualOverride ? 0 : item.UnionDeduction,
        20 => item.StoredValues.ManualOverride ? 0 : item.PhoneDeduction,
        21 => item.ExportOtherDeduction + item.PersonalIncomeTax + item.Advance,
        22 => item.NetSalary,
        _ => 0
    };
}
