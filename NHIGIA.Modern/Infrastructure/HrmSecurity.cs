using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class HrmAuthenticationMode
{
    private const string ModePath = "Authentication:Mode";

    public static string CookieName(bool usesKeycloak) =>
        usesKeycloak ? "NHIGIA.Auth.Keycloak.v1" : "NHIGIA.Auth.Local.v1";

    public static bool UsesKeycloak(IConfiguration configuration)
    {
        var mode = configuration[ModePath]?.Trim();
        if (string.IsNullOrWhiteSpace(mode))
        {
            return configuration.GetValue<bool>("Authentication:Keycloak:Enabled");
        }

        if (string.Equals(mode, "Local", StringComparison.OrdinalIgnoreCase)) return false;
        if (string.Equals(mode, "Keycloak", StringComparison.OrdinalIgnoreCase)) return true;

        throw new InvalidOperationException(
            $"{ModePath} chỉ chấp nhận giá trị Local hoặc Keycloak.");
    }
}

public static class HrmPasswordHasher
{
    private const int Iterations = 100000;

    public static bool Verify(string password, string saltBase64, string expectedHashBase64)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64)) return false;
        var salt = Convert.FromBase64String(saltBase64);
        var expected = Convert.FromBase64String(expectedHashBase64);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA1, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public sealed class HrmUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HrmUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public HrmUserAccountModel Current
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true) return null;
            int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
            int.TryParse(principal.FindFirstValue("department_id"), out var departmentId);
            int.TryParse(principal.FindFirstValue("supervisor_id"), out var supervisorId);
            return new HrmUserAccountModel
            {
                Id = id,
                Username = principal.Identity.Name,
                DisplayName = principal.FindFirstValue("display_name") ?? principal.Identity.Name,
                RoleCode = principal.FindFirstValue(ClaimTypes.Role) ?? HrmRoles.Employee,
                DepartmentId = departmentId > 0 ? departmentId : null,
                DepartmentName = principal.FindFirstValue("department_name"),
                SupervisorUserId = supervisorId > 0 ? supervisorId : null,
                IsActive = true
            };
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class HrmAuthorizeAttribute : AuthorizeAttribute
{
    public HrmAuthorizeAttribute(params string[] roles)
    {
        Roles = roles == null || roles.Length == 0 ? null : string.Join(',', roles);
    }
}
