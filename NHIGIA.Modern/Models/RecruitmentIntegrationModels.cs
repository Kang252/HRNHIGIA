using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models;

public sealed class RecruitmentIntegrationSettingsModel
{
    [Required, StringLength(30)] public string ProviderCode { get; set; }
    public string ProviderName { get; set; }
    [StringLength(500)] public string ApiBaseUrl { get; set; }
    [StringLength(200)] public string AccountId { get; set; }
    public string ApiKey { get; set; }
    public string WebhookSecret { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastReceivedAt { get; set; }
    public string LastStatus { get; set; }
    public string LastMessage { get; set; }
}

public sealed class RecruitmentCandidateModel
{
    public long Id { get; set; }
    public string ProviderCode { get; set; }
    public string ExternalApplicationId { get; set; }
    public string ExternalJobId { get; set; }
    public int? RecruitmentWorkItemId { get; set; }
    public string PositionTitle { get; set; }
    public string PositionReference { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime? AppliedAt { get; set; }
    public string StatusCode { get; set; }
    public string CvFileName { get; set; }
    public string CvContentType { get; set; }
    public string CvUrl { get; set; }
    public bool HasCvContent { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public sealed class RecruitmentCandidatePayload
{
    public string ProviderCode { get; set; }
    public string ExternalApplicationId { get; set; }
    public string ExternalJobId { get; set; }
    public string JobReference { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime? AppliedAt { get; set; }
    public string StatusCode { get; set; }
    public string CvFileName { get; set; }
    public string CvContentType { get; set; }
    public byte[] CvContent { get; set; }
    public string CvUrl { get; set; }
    public string RawPayload { get; set; }
}

public sealed record RecruitmentCandidateSaveResult(long CandidateId, bool Inserted, int? WorkItemId);

public sealed class RecruitmentCandidateFileModel
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Content { get; set; }
    public string Url { get; set; }
    public string ProviderCode { get; set; }
}
