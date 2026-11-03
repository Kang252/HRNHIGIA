using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
public sealed class ReportsController : BaseController
{
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(HrmDataStore store, HrmUserAccessor userAccessor, ILogger<ReportsController> logger)
        : base(store, userAccessor) => _logger = logger;

    [HttpGet]
    public IActionResult Summary(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var current = AttendancePeriodRange.Current();
            var from = (fromDate ?? current.FromDate).Date;
            var to = (toDate ?? current.ToDate).Date;
            return Json(NHIGIA.Modern.Models.ApiResponse.Ok(Store.GetReportSummary(CurrentHrmUser, from, to)));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(NHIGIA.Modern.Models.ApiResponse.Fail(exception.Message));
        }
        catch (SqlException exception)
        {
            _logger.LogError(exception, "Cannot build HR report summary");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse.Fail("Chưa thể truy cập dữ liệu báo cáo. Vui lòng thử lại sau."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot build HR report summary");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("Không thể lập báo cáo lúc này. Vui lòng thử lại."));
        }
    }
}
