using System.Data;
using Dapper;
using Microsoft.AspNetCore.DataProtection;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    public const string FaceMatchConsentVersion = "opencv-local-trial-v1";

    public async Task<FaceMatchResult> CompareRemoteFaceAsync(long id, bool consent, HrmUserAccountModel actor,
        string ip, IOpenCvFaceMatcher matcher, CancellationToken cancellationToken)
    {
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
        FaceMatchEvidence evidence;
        var attemptId = Guid.NewGuid();
        // Only short DB transactions surround a potentially slow model call. The lease prevents duplicate work.
        using (var db = OpenConnection())
        {
            var ownerId = db.QuerySingleOrDefault<int?>("SELECT UserId FROM dbo.HrmRemotePunch WHERE Id=@id", new { id });
            if (!ownerId.HasValue) throw new UnauthorizedAccessException();
            using var tx = db.BeginTransaction(IsolationLevel.Serializable);
            LockFaceOwner(db, tx, ownerId.Value);
            evidence = db.QuerySingleOrDefault<FaceMatchEvidence>(@"SELECT p.Id,p.UserId,p.StatusCode,p.Kind,p.FaceEnrollmentId,
                p.FaceMatchStatus,p.FaceMatchScore,p.FaceMatchThreshold,p.FaceMatchModelVersion,p.FaceMatchConsentAt,p.FaceMatchStartedAt,
                p.PhotoContent,u.DepartmentId,u.RoleCode,f.PhotoProtected,f.StatusCode AS EnrollmentStatus
                FROM dbo.HrmRemotePunch p WITH(UPDLOCK,HOLDLOCK)
                JOIN dbo.HrmUserAccount u ON u.Id=p.UserId AND u.IsActive=1
                LEFT JOIN dbo.HrmFaceEnrollment f WITH(HOLDLOCK) ON f.Id=p.FaceEnrollmentId AND f.UserId=p.UserId
                WHERE p.Id=@id", new { id }, tx);
            if (evidence == null || evidence.UserId != actor.Id && !RemoteAttendancePolicy.CanReview(actor, evidence.UserId, evidence.DepartmentId, evidence.RoleCode))
                throw new UnauthorizedAccessException();
            if (evidence.Kind is not ("IN" or "OUT") || !evidence.FaceEnrollmentId.HasValue)
                throw new InvalidOperationException("Lượt chấm này không có mẫu khuôn mặt để đối chiếu.");
            // Approval/rejection and revocation remain authoritative, even for late browser retries.
            if (evidence.StatusCode != "PENDING") throw new InvalidOperationException("Lượt chấm đã được xử lý; không chạy thử lại.");
            if (evidence.EnrollmentStatus != "ACTIVE" || evidence.PhotoProtected == null)
                throw new InvalidOperationException("Mẫu khuôn mặt đã bị thu hồi. Vui lòng đăng ký và chấm lại.");
            if (!matcher.IsEnabled)
            {
                tx.Commit();
                return new FaceMatchResult { Status = "DISABLED" };
            }
            if (!evidence.FaceMatchConsentAt.HasValue)
            {
                if (!consent || evidence.UserId != actor.Id)
                    throw new InvalidOperationException("Chỉ nhân viên sở hữu lượt chấm mới có thể đồng ý thử đối chiếu OpenCV.");
                db.Execute(@"UPDATE dbo.HrmRemotePunch SET FaceMatchConsentAt=SYSUTCDATETIME(),FaceMatchConsentVersion=@Version WHERE Id=@id",
                    new { id, Version = FaceMatchConsentVersion }, tx);
                AddAudit(db, actor.Id, "CONSENT", "RemoteFaceMatch", id.ToString(), FaceMatchConsentVersion, ip, tx);
            }
            if (FaceMatchResult.IsTerminal(evidence.FaceMatchStatus))
            {
                tx.Commit();
                return SavedFaceMatch(evidence);
            }
            if (evidence.FaceMatchStatus == "PROCESSING" && evidence.FaceMatchStartedAt > DateTime.UtcNow.AddSeconds(-30))
            {
                tx.Commit();
                return new FaceMatchResult { Status = "BUSY" };
            }
            db.Execute(@"UPDATE dbo.HrmRemotePunch SET FaceMatchStatus='PROCESSING',FaceMatchAttemptId=@attemptId,
                FaceMatchStartedAt=SYSUTCDATETIME() WHERE Id=@id", new { id, attemptId }, tx);
            tx.Commit();
        }

        FaceMatchResult result;
        try
        {
            var reference = _faceProtector.Unprotect(evidence.PhotoProtected);
            try { result = await matcher.CompareAsync(reference, evidence.PhotoContent, cancellationToken); }
            finally { Array.Clear(reference); }
        }
        catch (OperationCanceledException) { result = new FaceMatchResult { Status = "CANCELLED" }; }
        catch (Exception error) when (error is System.Security.Cryptography.CryptographicException or IOException or InvalidOperationException)
        {
            result = new FaceMatchResult { Status = "UNAVAILABLE" };
        }
        finally
        {
            Array.Clear(evidence.PhotoProtected);
            if (evidence.PhotoContent != null) Array.Clear(evidence.PhotoContent);
        }
        // Never trust model output as an attendance decision, nor persist after a concurrent manual decision/revoke.
        result ??= new FaceMatchResult { Status = "UNAVAILABLE" };
        using (var db = OpenConnection())
        using (var tx = db.BeginTransaction(IsolationLevel.Serializable))
        {
            LockFaceOwner(db, tx, evidence.UserId);
            var changed = db.Execute(@"UPDATE p SET FaceMatchStatus=@Status,FaceMatchScore=@Score,FaceMatchThreshold=@Threshold,
                FaceMatchModelVersion=@ModelVersion,FaceComparedAt=SYSUTCDATETIME(),FaceMatchAttemptId=NULL
                FROM dbo.HrmRemotePunch p JOIN dbo.HrmFaceEnrollment f WITH(HOLDLOCK) ON f.Id=p.FaceEnrollmentId AND f.UserId=p.UserId
                WHERE p.Id=@id AND p.UserId=@UserId AND p.StatusCode='PENDING' AND f.StatusCode='ACTIVE'
                AND p.FaceEnrollmentId=@FaceEnrollmentId AND p.FaceMatchAttemptId=@attemptId AND p.FaceMatchConsentAt IS NOT NULL",
                new { id, evidence.UserId, evidence.FaceEnrollmentId, attemptId, result.Status, result.Score, result.Threshold, result.ModelVersion }, tx);
            if (changed != 1)
            {
                tx.Commit();
                return new FaceMatchResult { Status = "STALE" };
            }
            AddAudit(db, actor.Id, "COMPARE", "RemoteFaceMatch", id.ToString(), "OpenCV thử nghiệm: " + result.Status + "; không xác thực liveness", ip, tx);
            tx.Commit();
        }
        return result;
    }

    private static FaceMatchResult SavedFaceMatch(RemotePunch row) => new()
    {
        Status = row.FaceMatchStatus, Score = row.FaceMatchScore, Threshold = row.FaceMatchThreshold, ModelVersion = row.FaceMatchModelVersion
    };

    private sealed class FaceMatchEvidence : RemotePunch
    {
        public byte[] PhotoContent { get; set; }
        public byte[] PhotoProtected { get; set; }
        public string EnrollmentStatus { get; set; }
        public DateTime? FaceMatchStartedAt { get; set; }
    }
}
