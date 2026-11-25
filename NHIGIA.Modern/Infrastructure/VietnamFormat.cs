using System.Globalization;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>Vietnamese digit grouping for people to read; machine formats stay invariant.</summary>
public static class VietnamFormat
{
    public static readonly NumberFormatInfo Numbers = new()
    {
        NumberGroupSeparator = ".", NumberDecimalSeparator = ",",
        PercentGroupSeparator = ".", PercentDecimalSeparator = ","
    };
}
