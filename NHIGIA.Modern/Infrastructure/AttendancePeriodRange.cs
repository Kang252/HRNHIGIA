using System.Globalization;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>
/// Company attendance periods run from the 25th of the previous month through
/// the 24th of the named month. The period key is therefore the ending month.
/// </summary>
public readonly record struct AttendancePeriodRange(string Period, DateTime FromDate, DateTime ToDate)
{
    public static AttendancePeriodRange Parse(string period)
    {
        if (!DateTime.TryParseExact(period + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var endingMonth))
            throw new InvalidOperationException("Kỳ chấm công không hợp lệ.");

        var to = endingMonth.AddDays(23);
        var from = endingMonth.AddMonths(-1).AddDays(24);
        return new AttendancePeriodRange(endingMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture), from, to);
    }

    public static AttendancePeriodRange ForDate(DateTime date)
    {
        var day = date.Date;
        var endingMonth = new DateTime(day.Year, day.Month, 1);
        if (day.Day >= 25) endingMonth = endingMonth.AddMonths(1);
        return Parse(endingMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture));
    }

    public static AttendancePeriodRange Current() => ForDate(HrmDataStore.CurrentVietnamTime());
}
