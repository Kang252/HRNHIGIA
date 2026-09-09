using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public const string DefaultSecretKey = "NHIGIA_GROUP_EHRM_SECRET_KEY_2026_ENTERPRISE_SYSTEM_KEY_V2_LONG_ENOUGH_256_BITS";
        public const string Issuer = "NHIGIA.eHRM";
        public const string Audience = "NHIGIA.eHRM.Mobile";

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string SecretKey => _configuration["JWT_SECRET_KEY"] ?? DefaultSecretKey;

        public (string Token, DateTime ExpiresAt) GenerateToken(HrmUserAccountModel user, TimeSpan? lifetime = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(30));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("display_name", user.DisplayName ?? user.Username),
                new Claim(ClaimTypes.Role, user.RoleCode ?? HrmRoles.Employee),
                new Claim("department_id", user.DepartmentId?.ToString() ?? string.Empty),
                new Claim("department_name", user.DepartmentName ?? string.Empty),
                new Claim("supervisor_id", user.SupervisorUserId?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}

