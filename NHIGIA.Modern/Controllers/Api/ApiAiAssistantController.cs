using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [Route("api/ai")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ApiAiAssistantController : ApiControllerBase
    {
        private readonly AiHrAssistantService _aiService;
        private readonly ILogger<ApiAiAssistantController> _logger;

        public ApiAiAssistantController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            AiHrAssistantService aiService,
            ILogger<ApiAiAssistantController> logger)
            : base(store, userAccessor)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public IActionResult Chat([FromBody] AiChatRequest request)
        {
            try
            {
                var response = _aiService.ProcessChat(CurrentUserId, request?.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý câu hỏi HR Chatbot");
                return FailResponse("Lỗi khi xử lý hội thoại AI: " + ex.Message, 500);
            }
        }

        [HttpPost("parse-cv")]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public async Task<IActionResult> ParseCv(IFormFile cvFile, [FromForm] string targetPosition)
        {
            try
            {
                string text = "";
                if (cvFile != null && cvFile.Length > 0)
                {
                    using var reader = new StreamReader(cvFile.OpenReadStream(), Encoding.UTF8);
                    text = await reader.ReadToEndAsync();
                }
                else
                {
                    text = "Nguyễn Văn Tuấn. Email: tuan.nguyen@example.com. SĐT: 0912345678. Đại học Bách Khoa. Kỹ năng: C#, .NET 8, SQL Server, Flutter, Kiến trúc Microservices. 4 năm kinh nghiệm phát triển hệ thống doanh nghiệp.";
                }

                var position = string.IsNullOrWhiteSpace(targetPosition) ? "Lập trình viên .NET / Mobile" : targetPosition;
                var result = _aiService.ParseCvText(text, position);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân tích CV");
                return FailResponse("Lỗi khi bóc tách hồ sơ CV: " + ex.Message, 500);
            }
        }
    }
}

