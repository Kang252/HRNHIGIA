using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class PayrollPolicy
{
    public static bool CanEdit(string roleCode) => roleCode is HrmRoles.Admin or HrmRoles.Accountant;

    public static bool CanApprove(string roleCode) => roleCode is HrmRoles.Admin or HrmRoles.Director;

    public static bool CanPublishOrPay(string roleCode) => roleCode is HrmRoles.Admin or HrmRoles.Accountant;

    public static DateTime AddBusinessDays(DateTime date, int businessDays)
    {
        if (businessDays < 0) throw new ArgumentOutOfRangeException(nameof(businessDays));
        var result = date.Date;
        while (businessDays > 0)
        {
            result = result.AddDays(1);
            if (result.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            businessDays--;
        }
        return result;
    }
}
