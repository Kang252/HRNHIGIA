using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public enum OvertimeDayType { Workday, RestDay, Holiday }

/// <summary>Overtime rates come from the day and hours worked, never from a value posted by the browser.</summary>
public static class OvertimePolicy
{
    public const string EveningCategory = "Ca tối (trong tuần)";
    public const string EndOfDayCategory = "Ca làm cuối ngày";
    public const string NightCategory = "Ca qua đêm";
    public const string WeekendCategory = "Ca cuối tuần";
    public const string HolidayCategory = "Ca ngày lễ";

    public static readonly string[] Categories =
        [EveningCategory, EndOfDayCategory, NightCategory, WeekendCategory, HolidayCategory];

    // Fixed-date public holidays. Lunar holidays are declared through HolidayCategory and approved by a manager.
    private static readonly (int Month, int Day)[] FixedHolidays = [(1, 1), (4, 30), (5, 1), (9, 2)];

    public static bool IsFixedHoliday(DateTime date) => FixedHolidays.Contains((date.Month, date.Day));

    public static OvertimeDayType Classify(DateTime date, bool isScheduledWorkday, IEnumerable<string> categories) =>
        IsFixedHoliday(date) || (categories ?? []).Any(x => x == HolidayCategory) ? OvertimeDayType.Holiday
        : isScheduledWorkday ? OvertimeDayType.Workday
        : OvertimeDayType.RestDay;

    public static decimal Rate(OvertimeDayType dayType, OvertimeRates rates) => dayType switch
    {
        OvertimeDayType.Holiday => rates.Holiday,
        OvertimeDayType.RestDay => rates.RestDay,
        _ => rates.Workday
    };

    /// <summary>Minutes multiplied by their rate, so a mixed day/night window is paid proportionally.</summary>
    public static decimal WeightedMinutes(OvertimeMinutes minutes, OvertimeDayType dayType, OvertimeRates rates)
    {
        var rate = Rate(dayType, rates);
        return (minutes.Total - minutes.Night) * rate + minutes.Night * (rate + rates.NightExtra);
    }

    /// <summary>Returns the category that matches the date, or throws when the declared one contradicts it.</summary>
    public static string ResolveCategory(string category, DateTime date, bool isScheduledWorkday)
    {
        category = Categories.Contains(category?.Trim()) ? category.Trim() : EveningCategory;
        if (IsFixedHoliday(date)) return HolidayCategory;
        if (category == HolidayCategory) return category;
        if (!isScheduledWorkday) return WeekendCategory;
        if (category == WeekendCategory)
            throw new InvalidOperationException($"Ngày {date:dd/MM/yyyy} là ngày làm việc theo lịch, không phải ngày nghỉ hằng tuần.");
        return category;
    }

    /// <summary>Planning rate shown on the registration form; payroll recalculates from actual punches.</summary>
    public static decimal PlannedRate(string category, OvertimeRates rates) => category switch
    {
        HolidayCategory => rates.Holiday,
        WeekendCategory => rates.RestDay,
        NightCategory => rates.Workday + rates.NightExtra,
        _ => rates.Workday
    };

    public static bool IsWorkday(IEnumerable<ScheduleModel> schedules, DateTime date)
    {
        var active = (schedules ?? []).Where(x => x.StatusCode == "ACTIVE" && x.EffectiveFrom.Date <= date.Date &&
            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= date.Date)).ToList();
        return active.Count > 0
            ? active.Any(x => (x.WorkDaysMask & (1 << (int)date.DayOfWeek)) != 0)
            : date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
    }
}

public sealed record OvertimeRates(decimal Workday = 1.5m, decimal RestDay = 2m, decimal Holiday = 3m, decimal NightExtra = .3m)
{
    public static OvertimeRates From(IConfiguration configuration) => new(
        Math.Max(0, configuration.GetValue("Payroll:OvertimeMultiplier", 1.5m)),
        Math.Max(0, configuration.GetValue("Payroll:OvertimeRestDayMultiplier", 2m)),
        Math.Max(0, configuration.GetValue("Payroll:OvertimeHolidayMultiplier", 3m)),
        Math.Max(0, configuration.GetValue("Payroll:OvertimeNightExtra", .3m)));
}

public readonly record struct OvertimeMinutes(int Total, int Night);
