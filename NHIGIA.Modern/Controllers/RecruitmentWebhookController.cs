using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/recruitment/{provider}/applications")]
public sealed class RecruitmentWebhookController : ControllerBase
{
    private readonly RecruitmentIntegrationStore _store;
    private readonly ILogger<RecruitmentWebhookController> _logger;

    public RecruitmentWebhookController(RecruitmentIntegrationStore store, ILogger<RecruitmentWebhookController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Receive(string provider, CancellationToken cancellationToken)
    {
        provider = RecruitmentIntegrationStore.NormalizeProvider(provider);
        if (provider is not ("TOPCV" or "CAREERVIET")) return NotFound();
        try
        {
            var settings = _store.GetSettings(provider, true);
            if (settings == null || !settings.IsEnabled) return StatusCode(StatusCodes.Status503ServiceUnavailable, new { success = false, message = "Kết nối đang tắt." });
            if (!ValidSecret(settings.WebhookSecret)) return Unauthorized(new { success = false, message = "Webhook secret không hợp lệ." });

            var payload = await ReadPayload(provider, cancellationToken);
            var saved = _store.SaveCandidate(payload);
            return Ok(new { success = true, inserted = saved.Inserted, candidateId = saved.CandidateId, workItemId = saved.WorkItemId });
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(new { success = false, message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Recruitment webhook failed for {Provider}", provider);
            try { _store.UpdateProviderFailure(provider, exception.Message); } catch { }
            return BadRequest(new { success = false, message = exception.Message });
        }
    }

    private bool ValidSecret(string expected)
    {
        if (string.IsNullOrWhiteSpace(expected)) return false;
        var authorization = Request.Headers.Authorization.FirstOrDefault();
        var supplied = Request.Headers["X-Recruitment-Secret"].FirstOrDefault()
            ?? Request.Headers["X-API-Key"].FirstOrDefault()
            ?? Request.Headers["Api-Key"].FirstOrDefault()
            ?? (authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true ? authorization[7..].Trim() : null)
            ?? Request.Query["secret"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(supplied)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));
    }

    private async Task<RecruitmentCandidatePayload> ReadPayload(string provider, CancellationToken cancellationToken)
    {
        JsonObject json;
        IFormFile cvFile = null;
        string formCvBase64 = null;
        string raw;
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            formCvBase64 = form.FirstOrDefault(x => string.Equals(x.Key, "cv_base64", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Key, "cvBase64", StringComparison.OrdinalIgnoreCase)).Value.ToString();
            json = new JsonObject(form.Where(x => !string.Equals(x.Key, "cv_base64", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => (JsonNode)x.Value.ToString()));
            cvFile = form.Files.FirstOrDefault();
            raw = json.ToJsonString();
        }
        else
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            raw = await reader.ReadToEndAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(raw)) throw new InvalidDataException("Nội dung webhook trống.");
            json = JsonNode.Parse(raw)?.AsObject() ?? throw new InvalidDataException("Webhook không phải JSON hợp lệ.");
        }

        var root = PayloadRoot(json);
        byte[] cvContent = null;
        string cvFileName = Value(root, "cv_file_name", "cvFileName", "resume_file_name", "filename");
        string cvContentType = Value(root, "cv_content_type", "cvContentType", "content_type");
        if (cvFile != null)
        {
            if (cvFile.Length > 10 * 1024 * 1024) throw new InvalidDataException("Tệp CV vượt quá giới hạn 10 MB.");
            await using var stream = new MemoryStream();
            await cvFile.CopyToAsync(stream, cancellationToken);
            cvContent = stream.ToArray();
            cvFileName = cvFile.FileName;
            cvContentType = cvFile.ContentType;
        }
        else
        {
            var base64 = formCvBase64 ?? Value(root, "cv_base64", "cvBase64", "resume_base64", "file_base64");
            if (!string.IsNullOrWhiteSpace(base64))
            {
                var comma = base64.IndexOf(',');
                if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0) base64 = base64[(comma + 1)..];
                try { cvContent = Convert.FromBase64String(base64); }
                catch (FormatException) { throw new InvalidDataException("Dữ liệu CV base64 không hợp lệ."); }
            }
        }

        return new RecruitmentCandidatePayload
        {
            ProviderCode = provider,
            ExternalApplicationId = Value(root, "application_id", "applicationId", "apply_id", "applyId", "id"),
            ExternalJobId = Value(root, "job_id", "jobId", "recruitment_id", "recruitmentId", "campaign_id"),
            JobReference = Value(root, "job_reference", "jobReference", "job_code", "jobCode", "reference"),
            FullName = Value(root, "full_name", "fullName", "candidate_name", "candidateName", "name"),
            Email = Value(root, "email", "candidate_email"),
            Phone = Value(root, "phone", "mobile", "phone_number", "candidate_phone"),
            AppliedAt = ParseDate(Value(root, "applied_at", "appliedAt", "apply_time", "created_at", "date")),
            StatusCode = Value(root, "status", "status_code") ?? "NEW",
            CvFileName = cvFileName,
            CvContentType = cvContentType,
            CvContent = cvContent,
            CvUrl = Value(root, "cv_url", "cvUrl", "resume_url", "resumeUrl", "download_url"),
            RawPayload = raw.Length <= 1000000 ? raw : raw[..1000000]
        };
    }

    private static JsonObject PayloadRoot(JsonObject json)
    {
        foreach (var name in new[] { "data", "application", "candidate" })
            if (json.FirstOrDefault(x => string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase)).Value is JsonObject nested) return nested;
        return json;
    }

    private static string Value(JsonObject source, params string[] names)
    {
        var queue = new Queue<(JsonObject Node, int Depth)>();
        queue.Enqueue((source, 0));
        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            foreach (var property in node)
            {
                if (names.Any(x => string.Equals(x, property.Key, StringComparison.OrdinalIgnoreCase))) return property.Value?.ToString();
                if (depth < 3 && property.Value is JsonObject nested) queue.Enqueue((nested, depth + 1));
            }
        }
        return null;
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (long.TryParse(value, out var epoch))
        {
            if (epoch > 9999999999) epoch /= 1000;
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date : null;
    }
}
