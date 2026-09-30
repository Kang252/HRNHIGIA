using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class FaceEnrollmentController : BaseController
{
    private readonly ILogger<FaceEnrollmentController> _logger;
    public FaceEnrollmentController(HrmDataStore store, HrmUserAccessor users, ILogger<FaceEnrollmentController> logger) : base(store, users) => _logger = logger;

    private HrmUserAccountModel Actor()
    {
        var actor = Store.FindUser(CurrentHrmUser?.Id ?? 0);
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
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
        catch (UnauthorizedAccessException) { return StatusCode(403, ApiResponse.Fail("Tài khoản không có quyền thực hiện thao tác này.")); }
        catch (InvalidOperationException error) { return BadRequest(ApiResponse.Fail(error.Message)); }
        catch (Exception error)
        {
            _logger.LogError(error, "Face enrollment operation failed");
            return StatusCode(503, ApiResponse.Fail("Chưa thể xử lý đăng ký khuôn mặt. Vui lòng thử lại sau."));
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Title = "Khuôn mặt chấm công";
        return View(CurrentHrmUser);
    }

    [HttpGet]
    public IActionResult State(string search) => Run(actor => Store.GetFaceEnrollmentState(actor, search));

    [HttpPost, RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Submit(IFormFile photo, bool consent)
    {
        if (photo == null || photo.Length < 4 || photo.Length > 2 * 1024 * 1024)
            return BadRequest(ApiResponse.Fail("Cần ảnh JPEG từ camera, tối đa 2 MB."));
        await using var buffer = new MemoryStream();
        await photo.CopyToAsync(buffer, HttpContext.RequestAborted);
        return Run(actor => new { Id = Store.SubmitFaceEnrollment(buffer.ToArray(), consent, actor, Ip), StatusCode = "PENDING" });
    }

    [HttpPost]
    public IActionResult Review(long id, bool approve, string note) => Run(actor =>
    {
        Store.ReviewFaceEnrollment(id, approve, note, actor, Ip);
        return new { Id = id, StatusCode = approve ? "ACTIVE" : "REJECTED" };
    });

    [HttpPost]
    public IActionResult Revoke(long id, string note) => Run(actor =>
    {
        Store.RevokeFaceEnrollment(id, note, actor, Ip);
        return new { Id = id, StatusCode = "REVOKED" };
    });

    [HttpGet]
    public IActionResult Photo(long id)
    {
        try
        {
            var photo = Store.GetFaceEnrollmentPhoto(id, Actor());
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return photo == null ? NotFound() : File(photo, "image/jpeg");
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception error)
        {
            _logger.LogError(error, "Cannot read protected face enrollment photo");
            return StatusCode(503);
        }
    }
}
