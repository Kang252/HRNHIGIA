namespace NHIGIA.Modern.Models;

public sealed class FaceEnrollmentModel
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public string StatusCode { get; set; }
    public string ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public sealed class FaceEnrollmentState
{
    public int UserId { get; set; }
    public FaceEnrollmentModel Enrollment { get; set; }
    public bool CanReview { get; set; }
    public IList<FaceEnrollmentModel> Pending { get; set; } = new List<FaceEnrollmentModel>();
    public IList<FaceEnrollmentModel> Active { get; set; } = new List<FaceEnrollmentModel>();
}
