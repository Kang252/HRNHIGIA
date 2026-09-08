using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;

namespace NHIGIA.Modern.Controllers;

[ApiController]
[Route("Health")]
public sealed class HealthController : ControllerBase
{
    private readonly HrmDataStore _store;

    public HealthController(HrmDataStore store) => _store = store;

    [HttpGet]
    public IActionResult Index() => Ok(new { Status = "OK" });

    [HttpGet("Database")]
    public IActionResult Database()
    {
        try { return Ok(new { Status = _store.CanConnect() ? "OK" : "FAILED" }); }
        catch (Exception exception) { return StatusCode(503, new { Status = "FAILED", Message = exception.Message }); }
    }
}
