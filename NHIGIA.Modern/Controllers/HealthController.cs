using NHIGIA.Modern.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NHIGIA.Modern.Controllers;

public sealed class HealthController : Controller
{
    private readonly HrmDbContext _dbContext;

    public HealthController(HrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var connected = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return Content(connected ? "Database connection: OK" : "Database connection: FAILED");
    }
}
