using System.Globalization;
using System.Text.RegularExpressions;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public readonly record struct KpiPeriod(string Type, string Label, DateTime Start, DateTime End);

public static partial class KpiPeriodPolicy
{
    public const string Month = "MONTH";
    public const string Quarter = "QUARTER";
    public const string Year = "YEAR";

    public static string NormalizeType(string value) => value?.Trim().ToUpperInvariant() switch
    {
        Month => Month,
        Year => Year,
        _ => Quarter
    };

    public static bool TryResolve(string type, string value, out KpiPeriod period)
    {
        type = NormalizeType(type);
        value = value?.Trim() ?? string.Empty;
        period = default;

        if (type == Month)
        {
            var match = MonthLabel().Match(value);
            if (!match.Success && DateTime.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var monthInput))
            {
                return ResolveMonth(monthInput.Year, monthInput.Month, out period);
            }
            return match.Success && ResolveMonth(int.Parse(match.Groups[2].Value), int.Parse(match.Groups[1].Value), out period);
        }

        if (type == Year)
        {
            var match = YearLabel().Match(value);
            if (!match.Success) return false;
            var year = int.Parse(match.Groups[1].Value);
            if (year is < 2000 or > 2100) return false;
            period = new KpiPeriod(Year, year.ToString(CultureInfo.InvariantCulture), new DateTime(year, 1, 1), new DateTime(year, 12, 31));
            return true;
        }

        var quarterMatch = QuarterLabel().Match(value);
        if (!quarterMatch.Success) return false;
        var quarter = int.Parse(quarterMatch.Groups[1].Value);
        var quarterYear = int.Parse(quarterMatch.Groups[2].Value);
        if (quarter is < 1 or > 4 || quarterYear is < 2000 or > 2100) return false;
        var start = new DateTime(quarterYear, (quarter - 1) * 3 + 1, 1);
        period = new KpiPeriod(Quarter, $"Quý {quarter}-{quarterYear}", start, start.AddMonths(3).AddDays(-1));
        return true;
    }

    public static KpiPeriod ResolveOrDefault(string type, string value, DateTime now)
    {
        if (TryResolve(type, value, out var period)) return period;
        type = NormalizeType(type);
        return type switch
        {
            Month => ResolveOrThrow(Month, $"Tháng {now.Month:00}-{now.Year}"),
            Year => ResolveOrThrow(Year, now.Year.ToString(CultureInfo.InvariantCulture)),
            _ => ResolveOrThrow(Quarter, $"Quý {((now.Month - 1) / 3) + 1}-{now.Year}")
        };
    }

    public static bool MatchesRange(WorkItem item, KpiPeriod from, KpiPeriod to)
    {
        var itemPeriod = ResolveItem(item);
        return itemPeriod.Start <= to.End && itemPeriod.End >= from.Start;
    }

    public static KpiPeriod ResolveItem(WorkItem item)
    {
        if (item.PeriodStart.HasValue && item.PeriodEnd.HasValue)
        {
            var type = NormalizeType(item.KpiPeriodType);
            var label = string.IsNullOrWhiteSpace(item.Quarter)
                ? ResolveOrDefault(type, null, item.PeriodStart.Value).Label
                : item.Quarter;
            return new KpiPeriod(type, label, item.PeriodStart.Value.Date, item.PeriodEnd.Value.Date);
        }
        return ResolveOrDefault(item.KpiPeriodType, item.Quarter, item.DueDate ?? DateTime.Today);
    }

    private static bool ResolveMonth(int year, int month, out KpiPeriod period)
    {
        period = default;
        if (year is < 2000 or > 2100 || month is < 1 or > 12) return false;
        var start = new DateTime(year, month, 1);
        period = new KpiPeriod(Month, $"Tháng {month:00}-{year}", start, start.AddMonths(1).AddDays(-1));
        return true;
    }

    private static KpiPeriod ResolveOrThrow(string type, string value) =>
        TryResolve(type, value, out var period) ? period : throw new InvalidOperationException("Kỳ KPI không hợp lệ.");

    [GeneratedRegex(@"^Tháng\s+(0?[1-9]|1[0-2])-(\d{4})$", RegexOptions.IgnoreCase)]
    private static partial Regex MonthLabel();

    [GeneratedRegex(@"^Quý\s+([1-4])-(\d{4})$", RegexOptions.IgnoreCase)]
    private static partial Regex QuarterLabel();

    [GeneratedRegex(@"^(?:Năm\s+)?(\d{4})$", RegexOptions.IgnoreCase)]
    private static partial Regex YearLabel();
}
