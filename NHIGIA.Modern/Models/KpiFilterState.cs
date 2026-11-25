using NHIGIA.Modern.Infrastructure;

namespace NHIGIA.Modern.Models;

/// <summary>The KPI list filter that was on screen when a form was submitted.</summary>
public sealed class KpiFilterState
{
    public string PeriodType { get; set; }
    public string FromQuarter { get; set; }
    public string ToQuarter { get; set; }
    public string KpiType { get; set; }
    public string Query { get; set; }

    /// <summary>
    /// Keeps the submitted filter when it already shows <paramref name="saved"/>;
    /// otherwise moves to the period and tab that contain it.
    /// </summary>
    public KpiFilterState Showing(WorkItem saved)
    {
        var period = KpiPeriodPolicy.ResolveItem(saved);
        var visible = !string.IsNullOrWhiteSpace(PeriodType)
            && KpiPeriodPolicy.NormalizeType(PeriodType) == period.Type
            && KpiPeriodPolicy.TryResolve(period.Type, FromQuarter, out var from)
            && KpiPeriodPolicy.TryResolve(period.Type, ToQuarter, out var to)
            && KpiPeriodPolicy.MatchesRange(saved, from, to);
        return new KpiFilterState
        {
            PeriodType = period.Type,
            FromQuarter = visible ? FromQuarter : period.Label,
            ToQuarter = visible ? ToQuarter : period.Label,
            KpiType = saved.KpiType == "PLAN" ? "PLAN" : "ASSIGNED",
            // A leftover search term could hide the row that was just saved.
            Query = null
        };
    }
}
