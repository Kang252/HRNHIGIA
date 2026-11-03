using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
public sealed class RecruitmentImportController : Controller
{
    private readonly RecruitmentIntegrationStore _store;
    private readonly HrmUserAccessor _user;
    private readonly ILogger<RecruitmentImportController> _logger;

    public RecruitmentImportController(RecruitmentIntegrationStore store, HrmUserAccessor user, ILogger<RecruitmentImportController> logger)
    {
        _store = store;
        _user = user;
        _logger = logger;
    }

    private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Preview(IFormFile cvFile, int? recruitmentWorkItemId, CancellationToken cancellationToken)
    {
        if (cvFile == null || cvFile.Length == 0)
        {
            TempData["WorkError"] = "Vui lòng chọn một tệp CV Word .docx.";
            return RecruitmentPage();
        }
        try
        {
            if (cvFile.Length > 10 * 1024 * 1024) throw new InvalidDataException("Tệp CV vượt quá giới hạn 10 MB.");
            await using var memory = new MemoryStream();
            await cvFile.CopyToAsync(memory, cancellationToken);
            var content = memory.ToArray();
            var preview = DocxCvParser.Parse(content, cvFile.FileName);
            preview.RecruitmentWorkItemId = recruitmentWorkItemId;
            preview.DraftId = _store.CreateCvImportDraft(preview, content, cvFile.ContentType, _user.Current.Id);
            ViewBag.Title = "Kiểm tra dữ liệu CV";
            return View(preview);
        }
        catch (Exception exception) when (exception is InvalidDataException or InvalidOperationException)
        {
            TempData["WorkError"] = exception.Message;
            return RecruitmentPage();
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Confirm(RecruitmentCvImportPreviewModel input)
    {
        var draft = _store.GetCvImportDraft(input.DraftId, _user.Current.Id);
        if (draft == null)
        {
            TempData["WorkError"] = "Bản xem trước đã hết hạn. Vui lòng tải lại CV.";
            return RecruitmentPage();
        }
        if (!ModelState.IsValid)
        {
            input.CvFileName = draft.CvFileName;
            input.ExtractedTextPreview = draft.ExtractedTextPreview;
            input.PositionTitle = draft.PositionTitle;
            ViewBag.Title = "Kiểm tra dữ liệu CV";
            return View("Preview", input);
        }
        try
        {
            var result = _store.CompleteCvImport(input, _user.Current.Id, ClientIp);
            TempData["WorkSuccess"] = result.Inserted
                ? $"Đã nhập CV và tạo hồ sơ ứng viên #{result.CandidateId}."
                : $"Đã cập nhật hồ sơ ứng viên #{result.CandidateId}.";
            return RecruitmentPage();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot complete CV import draft {DraftId}", input.DraftId);
            ModelState.AddModelError(string.Empty, exception is InvalidOperationException ? exception.Message : "Không thể lưu hồ sơ ứng viên. Vui lòng thử lại.");
            input.CvFileName = draft.CvFileName;
            input.ExtractedTextPreview = draft.ExtractedTextPreview;
            input.PositionTitle = draft.PositionTitle;
            ViewBag.Title = "Kiểm tra dữ liệu CV";
            return View("Preview", input);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancel(Guid draftId)
    {
        _store.DeleteCvImportDraft(draftId, _user.Current.Id);
        TempData["WorkSuccess"] = "Đã hủy bản nhập CV.";
        return RecruitmentPage();
    }

    private RedirectResult RecruitmentPage() => Redirect(Url.Action("Index", "Work", new { kind = "recruitment" }) + "#candidate-inbox");
}
