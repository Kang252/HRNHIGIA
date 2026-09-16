using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[Authorize]
public sealed class AssistantController : Controller
{
    private readonly HrmAssistantService _assistant;
    private readonly HrmUserAccessor _users;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(HrmAssistantService assistant, HrmUserAccessor users, ILogger<AssistantController> logger)
    {
        _assistant = assistant;
        _users = users;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Ask([FromBody] AssistantQuestionRequest request)
    {
        var question = request?.Question?.Trim();
        if (string.IsNullOrWhiteSpace(question)) return BadRequest(new { success = false, message = "Vui lòng nhập câu hỏi." });
        if (question.Length > 500) return BadRequest(new { success = false, message = "Câu hỏi không được dài quá 500 ký tự." });

        try
        {
            return Json(new { success = true, data = _assistant.Ask(question, _users.Current) });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AI assistant could not answer for user {UserId}", _users.Current?.Id);
            return StatusCode(500, new { success = false, message = "Tôi chưa thể đọc dữ liệu lúc này. Vui lòng thử lại sau." });
        }
    }
}
