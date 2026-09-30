using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;

namespace NHIGIA.Modern.Models;

public class RemoteWorkPlan
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public int? DepartmentId { get; set; }
    public string RoleCode { get; set; }
    public string Mode { get; set; } = "FIELD";
    public string PlaceName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int WorkDaysMask { get; set; } = 62;
    public TimeSpan WindowStart { get; set; } = TimeSpan.FromHours(8);
    public TimeSpan WindowEnd { get; set; } = TimeSpan.FromHours(17.5);
    public bool IsFlexible { get; set; }
    public int RequiredMinutes { get; set; } = 480;
    public int BreakMinutes { get; set; } = 60;
    [ModelBinder(BinderType = typeof(InvariantCoordinateBinder))]
    public double? Latitude { get; set; }
    [ModelBinder(BinderType = typeof(InvariantCoordinateBinder))]
    public double? Longitude { get; set; }
    public int RadiusMeters { get; set; } = 200;
    public int? LeaveRequestId { get; set; }
    public string Note { get; set; }
    public string StatusCode { get; set; }
    public string ReviewNote { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class RemotePunchRequest
{
    public Guid ClientId { get; set; }
    public int OwnerUserId { get; set; }
    public long PlanId { get; set; }
    public string Kind { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public bool WasOffline { get; set; }
    public bool FaceMatchConsent { get; set; }
    [ModelBinder(BinderType = typeof(InvariantCoordinateBinder))]
    public double? Latitude { get; set; }
    [ModelBinder(BinderType = typeof(InvariantCoordinateBinder))]
    public double? Longitude { get; set; }
    [ModelBinder(BinderType = typeof(InvariantCoordinateBinder))]
    public double? AccuracyMeters { get; set; }
    public string PlaceName { get; set; }
    public string Note { get; set; }
}

public class RemotePunch : RemotePunchRequest
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public int? DepartmentId { get; set; }
    public string RoleCode { get; set; }
    public DateTime CheckTime { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string StatusCode { get; set; }
    public string ReviewReason { get; set; }
    public string ReviewNote { get; set; }
    public double? DistanceMeters { get; set; }
    public bool CanReview { get; set; }
    public long? FaceEnrollmentId { get; set; }
    public string FaceVerificationStatus { get; set; }
    public string FaceMatchStatus { get; set; }
    public double? FaceMatchScore { get; set; }
    public double? FaceMatchThreshold { get; set; }
    public string FaceMatchModelVersion { get; set; }
    public DateTime? FaceComparedAt { get; set; }
    public DateTime? FaceMatchConsentAt { get; set; }
    public bool CanCompareFace { get; set; }
}
