using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.DataProtection;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public sealed class RecruitmentIntegrationStore
{
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _protector;

    public RecruitmentIntegrationStore(IConfiguration configuration, IDataProtectionProvider dataProtection)
    {
        _configuration = configuration;
        _protector = dataProtection.CreateProtector("NHIGIA.RecruitmentIntegration.v1");
    }

    private Microsoft.Data.SqlClient.SqlConnection Open() => DatabaseConfiguration.OpenConnection(_configuration);

    public void Load(WorkPage page)
    {
        using var db = Open();
        page.RecruitmentIntegrations = db.Query<RecruitmentIntegrationSettingsModel>(@"
            SELECT ProviderCode,ProviderName,ApiBaseUrl,AccountId,
                   CASE WHEN ProtectedApiKey IS NULL THEN NULL ELSE '********' END ApiKey,
                   WebhookSecret,IsEnabled,LastReceivedAt,LastStatus,LastMessage
            FROM dbo.HrmRecruitmentIntegration ORDER BY ProviderName").ToList();
        page.RecruitmentCandidates = db.Query<RecruitmentCandidateModel>(@"
            SELECT c.Id,c.ProviderCode,c.ExternalApplicationId,c.ExternalJobId,c.RecruitmentWorkItemId,
                   w.Title PositionTitle,w.Reference PositionReference,c.FullName,c.Email,c.Phone,c.AppliedAt,
                   c.DateOfBirth,c.Address,c.CurrentPosition,c.Education,c.ExperienceSummary,c.Skills,c.ExpectedSalary,
                   c.StatusCode,c.CvFileName,c.CvContentType,c.CvUrl,
                   CONVERT(bit,CASE WHEN c.CvContent IS NULL THEN 0 ELSE 1 END) HasCvContent,c.ReceivedAt
            FROM dbo.HrmRecruitmentCandidate c
            LEFT JOIN dbo.HrmWorkItem w ON w.Id=c.RecruitmentWorkItemId
            ORDER BY COALESCE(c.AppliedAt,c.ReceivedAt) DESC,c.Id DESC").ToList();

        var counts = page.RecruitmentCandidates.Where(x => x.RecruitmentWorkItemId.HasValue)
            .GroupBy(x => x.RecruitmentWorkItemId.Value).ToDictionary(x => x.Key, x => x.Count());
        foreach (var item in page.Items)
            if (counts.TryGetValue(item.Id, out var count)) item.Actual = count;
    }

    public RecruitmentIntegrationSettingsModel GetSettings(string providerCode, bool includeSecret)
    {
        providerCode = NormalizeProvider(providerCode);
        using var db = Open();
        var settings = db.QuerySingleOrDefault<RecruitmentIntegrationSettingsModel>(@"
            SELECT ProviderCode,ProviderName,ApiBaseUrl,AccountId,ProtectedApiKey ApiKey,WebhookSecret,
                   IsEnabled,LastReceivedAt,LastStatus,LastMessage
            FROM dbo.HrmRecruitmentIntegration WHERE ProviderCode=@ProviderCode", new { ProviderCode = providerCode });
        if (settings == null) return null;
        if (includeSecret) settings.ApiKey = Unprotect(settings.ApiKey);
        else settings.ApiKey = string.IsNullOrWhiteSpace(settings.ApiKey) ? null : "********";
        return settings;
    }

    public void SaveSettings(RecruitmentIntegrationSettingsModel input, int actorId, string ipAddress)
    {
        input.ProviderCode = NormalizeProvider(input.ProviderCode);
        if (input.ProviderCode is not ("TOPCV" or "CAREERVIET")) throw new InvalidOperationException("Nguồn tuyển dụng không được hỗ trợ.");
        if (!string.IsNullOrWhiteSpace(input.ApiBaseUrl)
            && (!Uri.TryCreate(input.ApiBaseUrl.Trim(), UriKind.Absolute, out var apiUri) || apiUri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException("API URL phải là địa chỉ HTTPS hợp lệ.");

        var protectedApiKey = !string.IsNullOrWhiteSpace(input.ApiKey) && input.ApiKey != "********"
            ? _protector.Protect(input.ApiKey.Trim()) : null;
        var webhookSecret = string.IsNullOrWhiteSpace(input.WebhookSecret)
            ? Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant()
            : input.WebhookSecret.Trim();
        using var db = Open();
        using var tx = db.BeginTransaction();
        db.Execute(@"
            UPDATE dbo.HrmRecruitmentIntegration SET ApiBaseUrl=@ApiBaseUrl,AccountId=@AccountId,
                ProtectedApiKey=COALESCE(@ProtectedApiKey,ProtectedApiKey),WebhookSecret=@WebhookSecret,
                IsEnabled=@IsEnabled,UpdatedAt=SYSDATETIME(),UpdatedByUserId=@ActorId
            WHERE ProviderCode=@ProviderCode", new
        {
            input.ProviderCode,
            ApiBaseUrl = input.ApiBaseUrl?.Trim().TrimEnd('/'),
            AccountId = input.AccountId?.Trim(),
            ProtectedApiKey = protectedApiKey,
            WebhookSecret = webhookSecret,
            input.IsEnabled,
            ActorId = actorId
        }, tx);
        db.Execute(@"INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
            VALUES(@UserId,'UPDATE','HrmRecruitmentIntegration',@Provider,@Detail,@IpAddress)", new
        {
            UserId = actorId,
            Provider = input.ProviderCode,
            Detail = $"Cập nhật kết nối tuyển dụng {input.ProviderCode}",
            IpAddress = ipAddress
        }, tx);
        tx.Commit();
    }

    public RecruitmentCandidateSaveResult SaveCandidate(RecruitmentCandidatePayload payload)
    {
        PrepareCandidate(payload);
        using var db = Open();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        var result = SaveCandidateCore(db, tx, payload);
        tx.Commit();
        return result;
    }

    private static void PrepareCandidate(RecruitmentCandidatePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        payload.ProviderCode = NormalizeProvider(payload.ProviderCode);
        payload.ExternalApplicationId = Clean(payload.ExternalApplicationId, 200);
        payload.ExternalJobId = Clean(payload.ExternalJobId, 200);
        payload.JobReference = Clean(payload.JobReference, 100);
        payload.FullName = Clean(payload.FullName, 200);
        payload.Email = Clean(payload.Email, 200);
        payload.Phone = Clean(payload.Phone, 50);
        payload.Address = Clean(payload.Address, 500);
        payload.CurrentPosition = Clean(payload.CurrentPosition, 200);
        payload.Education = Clean(payload.Education, 2000);
        payload.ExperienceSummary = Clean(payload.ExperienceSummary, 4000);
        payload.Skills = Clean(payload.Skills, 2000);
        payload.ExpectedSalary = Clean(payload.ExpectedSalary, 100);
        payload.StatusCode = Clean(payload.StatusCode, 50) ?? "NEW";
        payload.CvFileName = CleanFileName(payload.CvFileName);
        payload.CvContentType = Clean(payload.CvContentType, 150);
        payload.CvUrl = Clean(payload.CvUrl, 2000);
        if (string.IsNullOrWhiteSpace(payload.FullName) && string.IsNullOrWhiteSpace(payload.Email) && string.IsNullOrWhiteSpace(payload.Phone))
            throw new InvalidOperationException("Hồ sơ không có tên, email hoặc số điện thoại ứng viên.");
        if (payload.CvContent?.Length > 10 * 1024 * 1024) throw new InvalidOperationException("Tệp CV vượt quá giới hạn 10 MB.");
        if (payload.CvContent?.Length > 0 && !IsSupportedCv(payload.CvFileName, payload.CvContentType))
            throw new InvalidOperationException("CV chỉ hỗ trợ tệp PDF, DOC hoặc DOCX.");
        payload.ExternalApplicationId ??= Sha256($"{payload.ProviderCode}|{payload.ExternalJobId}|{payload.Email}|{payload.Phone}|{payload.AppliedAt:O}|{payload.RawPayload}");
    }

    private static RecruitmentCandidateSaveResult SaveCandidateCore(
        Microsoft.Data.SqlClient.SqlConnection db,
        Microsoft.Data.SqlClient.SqlTransaction tx,
        RecruitmentCandidatePayload payload)
    {
        var workItemId = payload.RecruitmentWorkItemId.HasValue
            ? db.QuerySingleOrDefault<int?>("SELECT Id FROM dbo.HrmWorkItem WHERE Id=@Id AND Kind='recruitment'", new { Id = payload.RecruitmentWorkItemId }, tx)
            : db.QuerySingleOrDefault<int?>(@"
            SELECT TOP 1 w.Id FROM dbo.HrmWorkItem w
            LEFT JOIN dbo.HrmRecruitmentJobMap m ON m.RecruitmentWorkItemId=w.Id AND m.ProviderCode=@ProviderCode
            WHERE w.Kind='recruitment' AND ((@ExternalJobId IS NOT NULL AND m.ExternalJobId=@ExternalJobId)
                OR (@JobReference IS NOT NULL AND w.Reference=@JobReference))
            ORDER BY CASE WHEN m.ExternalJobId=@ExternalJobId THEN 0 ELSE 1 END,w.Id DESC", payload, tx);
        if (payload.RecruitmentWorkItemId.HasValue && !workItemId.HasValue)
            throw new InvalidOperationException("Vị trí tuyển dụng không tồn tại.");

        var existingId = db.QuerySingleOrDefault<long?>(@"
            SELECT Id FROM dbo.HrmRecruitmentCandidate WITH (UPDLOCK,HOLDLOCK)
            WHERE ProviderCode=@ProviderCode AND ExternalApplicationId=@ExternalApplicationId", payload, tx);
        if (existingId.HasValue)
        {
            db.Execute(@"UPDATE dbo.HrmRecruitmentCandidate SET
                RecruitmentWorkItemId=COALESCE(RecruitmentWorkItemId,@WorkItemId),ExternalJobId=COALESCE(@ExternalJobId,ExternalJobId),
                FullName=COALESCE(@FullName,FullName),Email=COALESCE(@Email,Email),Phone=COALESCE(@Phone,Phone),
                DateOfBirth=COALESCE(@DateOfBirth,DateOfBirth),Address=COALESCE(@Address,Address),
                CurrentPosition=COALESCE(@CurrentPosition,CurrentPosition),Education=COALESCE(@Education,Education),
                ExperienceSummary=COALESCE(@ExperienceSummary,ExperienceSummary),Skills=COALESCE(@Skills,Skills),
                ExpectedSalary=COALESCE(@ExpectedSalary,ExpectedSalary),
                AppliedAt=COALESCE(@AppliedAt,AppliedAt),StatusCode=COALESCE(@StatusCode,StatusCode),
                CvFileName=COALESCE(@CvFileName,CvFileName),CvContentType=COALESCE(@CvContentType,CvContentType),
                CvContent=COALESCE(@CvContent,CvContent),CvUrl=COALESCE(@CvUrl,CvUrl),RawPayload=@RawPayload,UpdatedAt=SYSDATETIME()
                WHERE Id=@Id", new
            {
                Id = existingId.Value,
                WorkItemId = workItemId,
                payload.ExternalJobId,
                payload.FullName,
                payload.Email,
                payload.Phone,
                payload.DateOfBirth,
                payload.Address,
                payload.CurrentPosition,
                payload.Education,
                payload.ExperienceSummary,
                payload.Skills,
                payload.ExpectedSalary,
                payload.AppliedAt,
                payload.StatusCode,
                payload.CvFileName,
                payload.CvContentType,
                payload.CvContent,
                payload.CvUrl,
                payload.RawPayload
            }, tx);
            UpdateStatus(db, tx, payload.ProviderCode, $"Đã cập nhật hồ sơ {payload.FullName ?? payload.ExternalApplicationId}.");
            return new RecruitmentCandidateSaveResult(existingId.Value, false, workItemId);
        }

        var candidateId = db.ExecuteScalar<long>(@"
            INSERT dbo.HrmRecruitmentCandidate(ProviderCode,ExternalApplicationId,ExternalJobId,RecruitmentWorkItemId,
                FullName,Email,Phone,DateOfBirth,Address,CurrentPosition,Education,ExperienceSummary,Skills,ExpectedSalary,
                AppliedAt,StatusCode,CvFileName,CvContentType,CvContent,CvUrl,RawPayload)
            OUTPUT INSERTED.Id VALUES(@ProviderCode,@ExternalApplicationId,@ExternalJobId,@WorkItemId,
                @FullName,@Email,@Phone,@DateOfBirth,@Address,@CurrentPosition,@Education,@ExperienceSummary,@Skills,@ExpectedSalary,
                @AppliedAt,@StatusCode,@CvFileName,@CvContentType,@CvContent,@CvUrl,@RawPayload)", new
        {
            payload.ProviderCode,
            payload.ExternalApplicationId,
            payload.ExternalJobId,
            WorkItemId = workItemId,
            payload.FullName,
            payload.Email,
            payload.Phone,
            payload.DateOfBirth,
            payload.Address,
            payload.CurrentPosition,
            payload.Education,
            payload.ExperienceSummary,
            payload.Skills,
            payload.ExpectedSalary,
            payload.AppliedAt,
            payload.StatusCode,
            payload.CvFileName,
            payload.CvContentType,
            payload.CvContent,
            payload.CvUrl,
            payload.RawPayload
        }, tx);
        db.Execute(@"
            INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
            SELECT Id,N'Có CV ứng viên mới',@Message,N'/Work?kind=recruitment#candidate-inbox'
            FROM dbo.HrmUserAccount WHERE IsActive=1 AND RoleCode IN ('ADMIN','HR','DIRECTOR')", new
        {
            Message = $"{payload.FullName ?? "Ứng viên mới"} từ {payload.ProviderCode}" + (workItemId.HasValue ? " đã được ghép vào vị trí tuyển dụng." : " cần được ghép vào vị trí tuyển dụng.")
        }, tx);
        UpdateStatus(db, tx, payload.ProviderCode, $"Đã nhận hồ sơ {payload.FullName ?? payload.ExternalApplicationId}.");
        return new RecruitmentCandidateSaveResult(candidateId, true, workItemId);
    }

    public Guid CreateCvImportDraft(RecruitmentCvImportPreviewModel preview, byte[] content, string contentType, int actorId)
    {
        if (preview == null) throw new ArgumentNullException(nameof(preview));
        if (content == null || content.Length == 0 || content.Length > 10 * 1024 * 1024)
            throw new InvalidDataException("Tệp CV trống hoặc vượt quá giới hạn 10 MB.");
        var draftId = Guid.NewGuid();
        using var db = Open();
        using var tx = db.BeginTransaction();
        if (preview.RecruitmentWorkItemId.HasValue && db.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM dbo.HrmWorkItem WHERE Id=@Id AND Kind='recruitment'",
                new { Id = preview.RecruitmentWorkItemId.Value }, tx) == 0)
            throw new InvalidOperationException("Vị trí tuyển dụng không tồn tại.");
        db.Execute("DELETE dbo.HrmRecruitmentImportDraft WHERE ExpiresAt<SYSDATETIME()", transaction: tx);
        db.Execute(@"INSERT dbo.HrmRecruitmentImportDraft
            (Id,CreatedByUserId,RecruitmentWorkItemId,FullName,Email,Phone,DateOfBirth,Address,CurrentPosition,
             Education,ExperienceSummary,Skills,ExpectedSalary,CvFileName,CvContentType,CvContent,ExtractedTextPreview,ExpiresAt)
            VALUES(@Id,@CreatedByUserId,@RecruitmentWorkItemId,@FullName,@Email,@Phone,@DateOfBirth,@Address,@CurrentPosition,
             @Education,@ExperienceSummary,@Skills,@ExpectedSalary,@CvFileName,@CvContentType,@CvContent,@ExtractedTextPreview,DATEADD(MINUTE,30,SYSDATETIME()))", new
        {
            Id = draftId,
            CreatedByUserId = actorId,
            preview.RecruitmentWorkItemId,
            preview.FullName,
            preview.Email,
            preview.Phone,
            preview.DateOfBirth,
            preview.Address,
            preview.CurrentPosition,
            preview.Education,
            preview.ExperienceSummary,
            preview.Skills,
            preview.ExpectedSalary,
            preview.CvFileName,
            CvContentType = Clean(contentType, 150) ?? "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            CvContent = content,
            preview.ExtractedTextPreview
        }, tx);
        tx.Commit();
        return draftId;
    }

    public RecruitmentCvImportDraftModel GetCvImportDraft(Guid draftId, int actorId)
    {
        using var db = Open();
        return db.QuerySingleOrDefault<RecruitmentCvImportDraftModel>(@"SELECT d.*,w.Title PositionTitle
            FROM dbo.HrmRecruitmentImportDraft d
            LEFT JOIN dbo.HrmWorkItem w ON w.Id=d.RecruitmentWorkItemId AND w.Kind='recruitment'
            WHERE d.Id=@DraftId AND d.CreatedByUserId=@ActorId AND d.ExpiresAt>=SYSDATETIME()",
            new { DraftId = draftId, ActorId = actorId });
    }

    public RecruitmentCandidateSaveResult CompleteCvImport(RecruitmentCvImportPreviewModel input, int actorId, string ipAddress)
    {
        using var db = Open();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        var draft = db.QuerySingleOrDefault<RecruitmentCvImportDraftModel>(@"SELECT d.*,w.Title PositionTitle
            FROM dbo.HrmRecruitmentImportDraft d WITH(UPDLOCK,HOLDLOCK)
            LEFT JOIN dbo.HrmWorkItem w ON w.Id=d.RecruitmentWorkItemId AND w.Kind='recruitment'
            WHERE d.Id=@DraftId AND d.CreatedByUserId=@ActorId AND d.ExpiresAt>=SYSDATETIME()",
            new { DraftId = input.DraftId, ActorId = actorId }, tx)
            ?? throw new InvalidOperationException("Bản xem trước đã hết hạn. Vui lòng tải lại CV.");
        var payload = new RecruitmentCandidatePayload
        {
            ProviderCode = "MANUAL",
            ExternalApplicationId = $"manual:{draft.DraftId:N}",
            RecruitmentWorkItemId = input.RecruitmentWorkItemId,
            FullName = input.FullName,
            Email = input.Email,
            Phone = input.Phone,
            DateOfBirth = input.DateOfBirth,
            Address = input.Address,
            CurrentPosition = input.CurrentPosition,
            Education = input.Education,
            ExperienceSummary = input.ExperienceSummary,
            Skills = input.Skills,
            ExpectedSalary = input.ExpectedSalary,
            AppliedAt = DateTime.UtcNow,
            StatusCode = "NEW",
            CvFileName = draft.CvFileName,
            CvContentType = draft.CvContentType,
            CvContent = draft.CvContent,
            RawPayload = JsonSerializer.Serialize(new { source = "manual-docx", importedBy = actorId, draft = draft.DraftId })
        };
        PrepareCandidate(payload);
        var saved = SaveCandidateCore(db, tx, payload);
        db.Execute("DELETE dbo.HrmRecruitmentImportDraft WHERE Id=@Id AND CreatedByUserId=@ActorId",
            new { Id = draft.DraftId, ActorId = actorId }, tx);
        db.Execute(@"INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
            VALUES(@UserId,'IMPORT','HrmRecruitmentCandidate',@EntityId,@Detail,@IpAddress)", new
        {
            UserId = actorId,
            EntityId = saved.CandidateId.ToString(),
            Detail = $"Nhập CV Word thủ công: {draft.CvFileName}",
            IpAddress = ipAddress
        }, tx);
        tx.Commit();
        return saved;
    }

    public void DeleteCvImportDraft(Guid draftId, int actorId)
    {
        using var db = Open();
        db.Execute("DELETE dbo.HrmRecruitmentImportDraft WHERE Id=@DraftId AND CreatedByUserId=@ActorId",
            new { DraftId = draftId, ActorId = actorId });
    }

    public RecruitmentCandidateFileModel GetCandidateFile(long id)
    {
        using var db = Open();
        return db.QuerySingleOrDefault<RecruitmentCandidateFileModel>(@"
            SELECT CvFileName FileName,CvContentType ContentType,CvContent Content,CvUrl Url,ProviderCode
            FROM dbo.HrmRecruitmentCandidate WHERE Id=@Id", new { Id = id });
    }

    public bool AssignCandidate(long candidateId, int workItemId, int actorId, string ipAddress)
    {
        using var db = Open();
        using var tx = db.BeginTransaction();
        if (db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmWorkItem WHERE Id=@Id AND Kind='recruitment'", new { Id = workItemId }, tx) == 0)
            throw new InvalidOperationException("Vị trí tuyển dụng không tồn tại.");
        var changed = db.Execute(@"UPDATE dbo.HrmRecruitmentCandidate SET RecruitmentWorkItemId=@WorkItemId,UpdatedAt=SYSDATETIME()
            WHERE Id=@CandidateId", new { CandidateId = candidateId, WorkItemId = workItemId }, tx) > 0;
        if (changed)
            db.Execute(@"INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
                VALUES(@UserId,'UPDATE','HrmRecruitmentCandidate',@EntityId,@Detail,@IpAddress)", new
            {
                UserId = actorId,
                EntityId = candidateId.ToString(),
                Detail = $"Ghép ứng viên vào vị trí tuyển dụng #{workItemId}",
                IpAddress = ipAddress
            }, tx);
        tx.Commit();
        return changed;
    }

    public void UpdateProviderFailure(string providerCode, string message)
    {
        using var db = Open();
        db.Execute(@"UPDATE dbo.HrmRecruitmentIntegration SET LastStatus='FAILED',LastMessage=@Message,UpdatedAt=SYSDATETIME()
            WHERE ProviderCode=@ProviderCode", new { ProviderCode = NormalizeProvider(providerCode), Message = Clean(message, 1000) });
    }

    private static void UpdateStatus(IDbConnection db, IDbTransaction tx, string providerCode, string message) => db.Execute(@"
        UPDATE dbo.HrmRecruitmentIntegration SET LastReceivedAt=SYSDATETIME(),LastStatus='SUCCESS',LastMessage=@Message,UpdatedAt=SYSDATETIME()
        WHERE ProviderCode=@ProviderCode", new { ProviderCode = providerCode, Message = Clean(message, 1000) }, tx);

    public static string NormalizeProvider(string value) => (value ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "TOPCV" => "TOPCV",
        "CAREERVIET" or "CAREER_VIET" or "CAREER-VIET" => "CAREERVIET",
        _ => (value ?? string.Empty).Trim().ToUpperInvariant()
    };

    private string Unprotect(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return _protector.Unprotect(value); } catch { return null; }
    }

    private static string Clean(string value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
    private static string CleanFileName(string value)
    {
        var name = string.IsNullOrWhiteSpace(value) ? null : Path.GetFileName(value.Trim());
        return Clean(name, 255);
    }
    private static bool IsSupportedCv(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (extension is ".pdf" or ".doc" or ".docx") return true;
        return (contentType ?? string.Empty).ToLowerInvariant() is "application/pdf" or "application/msword"
            or "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }
    private static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();
}
