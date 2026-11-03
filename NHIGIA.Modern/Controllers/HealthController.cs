using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;

namespace NHIGIA.Modern.Controllers;

[ApiController]
[Route("Health")]
public sealed class HealthController : ControllerBase
{
    private readonly HrmDataStore _store;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HrmDataStore store, ILogger<HealthController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var missing = _store.GetMissingRequiredSchema();
            if (missing.Count == 0) return Ok(new { Status = "OK" });
            _logger.LogError("HRM schema chưa sẵn sàng. Thiếu: {MissingSchema}", string.Join(", ", missing));
            return StatusCode(503, new { Status = "FAILED", Component = "DatabaseSchema" });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Không thể kiểm tra trạng thái sẵn sàng của HRM");
            return StatusCode(503, new { Status = "FAILED", Component = "Database" });
        }
    }

    [HttpGet("Live")]
    public IActionResult Live() => Ok(new { Status = "OK" });

    [HttpGet("Database")]
    public IActionResult Database()
    {
        try { return Ok(new { Status = _store.CanConnect() ? "OK" : "FAILED" }); }
        catch (Exception exception) { return StatusCode(503, new { Status = "FAILED", Message = exception.Message }); }
    }
}
