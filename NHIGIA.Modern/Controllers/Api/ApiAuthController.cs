using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [Route("api/auth")]
    public class ApiAuthController : ApiControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<ApiAuthController> _logger;

        public ApiAuthController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            JwtService jwtService,
            ILogger<ApiAuthController> logger)
            : base(store, userAccessor)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] MobileLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return FailResponse("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

            try
            {
                var user = Store.FindUser(request.Username.Trim());
                if (user == null || !user.IsActive)
                    return FailResponse("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

                if (!HrmPasswordHasher.Verify(request.Password, user.PasswordSalt, user.PasswordHash))
                    return FailResponse("Mật khẩu không chính xác.");

                // Ghi nhận đăng nhập
                Store.MarkLogin(user.Id, ClientIp);

                // Sinh JWT Token
                var (token, expiresAt) = _jwtService.GenerateToken(user);

                // Lấy thông tin chi tiết hồ sơ
                var profile = Store.GetEmployeeProfile(user.Id);

                var response = new MobileAuthResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công vào hệ sinh thái eHRM Nhị Gia.",
                    Token = token,
                    RefreshToken = Guid.NewGuid().ToString("N"),
                    ExpiresAt = expiresAt,
                    User = new MobileUserProfile
                    {
                        Id = user.Id,
                        Username = user.Username,
                        DisplayName = user.DisplayName,
                        RoleCode = user.RoleCode,
                        RoleLabel = user.RoleLabel,
                        DepartmentId = user.DepartmentId,
                        DepartmentName = user.DepartmentName,
                        JobTitle = profile?.JobTitle ?? user.RoleLabel,
                        EmployeeCode = profile?.EmployeeCode ?? $"NG{user.Id:D3}",
                        AvatarUrl = profile?.AvatarUrl,
                        CompanyEmail = profile?.CompanyEmail,
                        MobilePhone = profile?.MobilePhone
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đăng nhập Mobile API cho người dùng {Username}", request.Username);
                return FailResponse("Lỗi máy chủ khi xác thực: " + ex.Message, 500);
            }
        }

        [HttpGet("profile")]
        [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
        public IActionResult GetProfile()
        {
            var user = Store.FindUser(CurrentUserId);
            if (user == null) return FailResponse("Không tìm thấy thông tin nhân viên.", 404);

            var profile = Store.GetEmployeeProfile(user.Id);
            return OkResponse(new MobileUserProfile
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                RoleCode = user.RoleCode,
                RoleLabel = user.RoleLabel,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.DepartmentName,
                JobTitle = profile?.JobTitle ?? user.RoleLabel,
                EmployeeCode = profile?.EmployeeCode ?? $"NG{user.Id:D3}",
                AvatarUrl = profile?.AvatarUrl,
                CompanyEmail = profile?.CompanyEmail,
                MobilePhone = profile?.MobilePhone
            });
        }
    }
}
