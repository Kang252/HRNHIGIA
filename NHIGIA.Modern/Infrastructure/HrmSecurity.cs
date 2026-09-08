using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public static class HrmPasswordHasher
{
    private const int Iterations = 100000;

    public static bool Verify(string password, string saltBase64, string expectedHashBase64)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64)) return false;
        var salt = Convert.FromBase64String(saltBase64);
        var expected = Convert.FromBase64String(expectedHashBase64);
        using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA1);
        var actual = derive.GetBytes(32);
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
