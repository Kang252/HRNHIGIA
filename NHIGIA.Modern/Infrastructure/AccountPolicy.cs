using System.Security.Cryptography;
using System.Text.RegularExpressions;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static partial class AccountPolicy
{
    public const int MinimumPasswordLength = 10;

    public static bool CanManageAccounts(string roleCode) => roleCode is HrmRoles.Admin or HrmRoles.Hr;

    /// <summary>HR cannot create or change accounts that hold HR, director or administrator rights.</summary>
    public static IReadOnlyList<string> AssignableRoles(string actorRole) => actorRole switch
    {
        HrmRoles.Admin => [HrmRoles.Employee, HrmRoles.Manager, HrmRoles.Accountant, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Admin],
        HrmRoles.Hr => [HrmRoles.Employee, HrmRoles.Manager, HrmRoles.Accountant],
        _ => []
    };

    public static bool CanManage(HrmUserAccountModel actor, HrmUserAccountModel target) =>
        actor != null && target != null && actor.Id != target.Id && AssignableRoles(actor.RoleCode).Contains(target.RoleCode);

    public static string ValidateUsername(string username) =>
        UsernamePattern().IsMatch(username ?? "") ? null : "Tài khoản dài 3–50 ký tự, chỉ gồm chữ thường không dấu, số, dấu chấm, gạch dưới hoặc gạch ngang.";

    public static string ValidatePassword(string password, string username)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinimumPasswordLength || password.Length > 128)
            return $"Mật khẩu dài từ {MinimumPasswordLength} đến 128 ký tự.";
        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit)) return "Mật khẩu cần có cả chữ và số.";
        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username, StringComparison.OrdinalIgnoreCase))
            return "Mật khẩu không được chứa tên tài khoản.";
        return null;
    }

    public static string TemporaryPassword()
    {
        // No look-alike characters: this is read aloud or retyped once.
        const string letters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        var chars = RandomNumberGenerator.GetItems<char>(letters, 10).Concat(RandomNumberGenerator.GetItems<char>(digits, 4)).ToArray();
        RandomNumberGenerator.Shuffle<char>(chars);
        return new string(chars);
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9._-]{2,49}$")]
    private static partial Regex UsernamePattern();
}
