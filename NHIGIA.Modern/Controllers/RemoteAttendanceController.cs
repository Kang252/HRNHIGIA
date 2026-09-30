using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RemoteAttendanceController : BaseController
{
    private readonly ILogger<RemoteAttendanceController> _logger;
    private readonly IOpenCvFaceMatcher _faceMatcher;
    public RemoteAttendanceController(HrmDataStore store, HrmUserAccessor users, ILogger<RemoteAttendanceController> logger,
        IOpenCvFaceMatcher faceMatcher) : base(store, users) { _logger = logger; _faceMatcher = faceMatcher; }

    private HrmUserAccountModel Actor()
    {
        var actor = Store.FindUser(CurrentHrmUser?.Id ?? 0);
        if (actor == null || !actor.IsActive) throw new UnauthorizedAccessException();
        return actor;
    }
    private string Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private IActionResult Run(Func<HrmUserAccountModel, object> action)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse.Fail("Dữ liệu nhập không hợp lệ."));
            return Json(ApiResponse.Ok(action(Actor())));
        }
        catch (UnauthorizedAccessException) { return StatusCode(403, ApiResponse.Fail("Tài khoản không còn quyền truy cập.")); }
        catch (InvalidOperationException error) { return BadRequest(ApiResponse.Fail(error.Message)); }
        catch (Exception error)
        {
            _logger.LogError(error, "Remote attendance request failed");
            return StatusCode(503, ApiResponse.Fail("Chưa thể lưu/tải dữ liệu. Vui lòng thử lại; bản ghi chờ đồng bộ được giữ trên thiết bị."));
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Title = "Chấm công ngoài công ty";
        return View(CurrentHrmUser);
    }

    [HttpGet]
    public IActionResult State(DateTime from, DateTime to) => Run(actor => new
    {
        UserId = actor.Id,
        FaceEnrollmentStatus = Store.GetFaceEnrollmentState(actor).Enrollment?.StatusCode,
        OpenCvEnabled = _faceMatcher.IsEnabled,
        Plans = Store.GetRemotePlans(actor, from, to).Select(p => new
        {
            Plan = p, CanReview = p.StatusCode == "PENDING" && RemoteAttendancePolicy.CanReview(actor, p.UserId, p.DepartmentId, p.RoleCode),
            CanCancel = p.StatusCode is "PENDING" or "APPROVED" && (p.UserId == actor.Id || RemoteAttendancePolicy.CanReview(actor, p.UserId, p.DepartmentId, p.RoleCode))
        }),
        Punches = Store.GetRemotePunches(actor, from, to),
        Trips = Store.GetLeaveRequests(actor).Where(p => p.UserId == actor.Id && p.StatusCode == "APPROVED" && p.LeaveType == "Công tác/Ra ngoài")
            .Select(p => new { p.Id, p.RequestCode, p.StartDate, p.EndDate })
    });

    [HttpPost]
    public IActionResult Plan(RemoteWorkPlan plan) => Run(actor => new { Id = Store.CreateRemotePlan(plan, actor, Ip) });
    [HttpPost]
    public IActionResult ReviewPlan(long id, bool approve, string note) => Run(actor => { Store.DecideRemotePlan(id, approve, note, actor, Ip); return new { Id = id }; });
    [HttpPost]
    public IActionResult CancelPlan(long id) => Run(actor => { Store.CancelRemotePlan(id, actor, Ip); return new { Id = id }; });
    [HttpPost]
    public IActionResult ReviewPunch(long id, bool approve, string note) => Run(actor => { Store.DecideRemotePunch(id, approve, note, actor, Ip); return new { Id = id }; });

    [HttpPost]
    public async Task<IActionResult> CompareFace(long id, bool consent, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse.Fail("Dữ liệu nhập không hợp lệ."));
            var result = await Store.CompareRemoteFaceAsync(id, consent, Actor(), Ip, _faceMatcher, cancellationToken);
            return Json(ApiResponse.Ok(result));
        }
        catch (UnauthorizedAccessException) { return StatusCode(403, ApiResponse.Fail("Tài khoản không có quyền đối chiếu lượt chấm này.")); }
        catch (InvalidOperationException error) { return BadRequest(ApiResponse.Fail(error.Message)); }
        catch (Exception error)
        {
            _logger.LogError(error, "OpenCV face trial failed for remote punch {PunchId}", id);
            return StatusCode(503, ApiResponse.Fail("Chưa thể thử so khớp. Lượt chấm vẫn chờ xác minh thủ công."));
        }
    }

    [HttpPost, RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Punch(RemotePunchRequest request, IFormFile photo)
    {
        if (photo == null || photo.Length < 4 || photo.Length > 2 * 1024 * 1024)
            return BadRequest(ApiResponse.Fail("Cần ảnh JPEG từ camera, tối đa 2 MB."));
        await using var stream = new MemoryStream();
        await photo.CopyToAsync(stream);
        var bytes = stream.ToArray();
        if (bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[2] != 0xff || bytes[^2] != 0xff || bytes[^1] != 0xd9)
            return BadRequest(ApiResponse.Fail("Ảnh JPEG không hợp lệ."));
        return Run(actor => Store.RecordRemotePunch(request, bytes, actor, Ip));
    }

    [HttpGet]
    public IActionResult FaceReference(long id)
    {
        try
        {
            var content = Store.GetRemoteFaceReference(id, Actor());
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return content == null ? NotFound() : File(content, "image/jpeg");
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception error)
        {
            _logger.LogError(error, "Cannot load face reference for remote punch {PunchId}", id);
            return StatusCode(503, ApiResponse.Fail("Chưa tải được ảnh tham chiếu. Vui lòng thử lại."));
        }
    }

    [HttpGet]
    public IActionResult Photo(long id)
    {
        try
        {
            var content = Store.GetRemotePhoto(id, Actor());
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return content == null ? NotFound() : File(content, "image/jpeg");
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
