using System.Data;
using Dapper;
using Microsoft.AspNetCore.DataProtection;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    private readonly IDataProtector _faceProtector;
    public const string FaceEnrollmentConsentVersion = "photo-identity-v1";

    public static bool CanReviewFaceEnrollment(HrmUserAccountModel actor) =>
        actor?.IsActive == true && actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr;

    public FaceEnrollmentState GetFaceEnrollmentState(HrmUserAccountModel actor, string search = null)
    {
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
        if (search?.Length > 100) throw new InvalidOperationException("Tìm kiếm tối đa 100 ký tự.");
        using var db = OpenConnection();
        var state = new FaceEnrollmentState
        {
            UserId = actor.Id,
            CanReview = CanReviewFaceEnrollment(actor),
            Enrollment = db.QuerySingleOrDefault<FaceEnrollmentModel>(@"SELECT TOP (1) f.Id,f.UserId,u.DisplayName,
                f.StatusCode,f.ReviewNote,f.CreatedAt,f.ReviewedAt,f.RevokedAt
                FROM dbo.HrmFaceEnrollment f JOIN dbo.HrmUserAccount u ON u.Id=f.UserId
                WHERE f.UserId=@Id ORDER BY f.Id DESC", new { actor.Id })
        };
        if (state.CanReview)
        {
            state.Pending = db.Query<FaceEnrollmentModel>(@"SELECT f.Id,f.UserId,u.DisplayName,f.StatusCode,
                f.ReviewNote,f.CreatedAt,f.ReviewedAt,f.RevokedAt
                FROM dbo.HrmFaceEnrollment f JOIN dbo.HrmUserAccount u ON u.Id=f.UserId
                WHERE f.StatusCode='PENDING' AND f.UserId<>@Id AND u.IsActive=1 ORDER BY f.Id", new { actor.Id }).ToList();
            state.Active = db.Query<FaceEnrollmentModel>(@"SELECT TOP (200) f.Id,f.UserId,u.DisplayName,f.StatusCode,
                f.ReviewNote,f.CreatedAt,f.ReviewedAt,f.RevokedAt
                FROM dbo.HrmFaceEnrollment f JOIN dbo.HrmUserAccount u ON u.Id=f.UserId
                WHERE f.StatusCode='ACTIVE' AND (@Search='' OR CHARINDEX(@Search,u.DisplayName)>0 OR CHARINDEX(@Search,u.Username)>0)
                ORDER BY f.Id DESC", new { Search = search?.Trim() ?? "" }).ToList();
        }
        // DATETIME2 is stored as UTC; preserve the zone when serializing to browser clients.
        if (state.Enrollment != null) SetFaceUtcDates(state.Enrollment);
        foreach (var row in state.Pending) SetFaceUtcDates(row);
        foreach (var row in state.Active) SetFaceUtcDates(row);
        return state;
    }

    private static void SetFaceUtcDates(FaceEnrollmentModel row)
    {
        row.CreatedAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc);
        if (row.ReviewedAt.HasValue) row.ReviewedAt = DateTime.SpecifyKind(row.ReviewedAt.Value, DateTimeKind.Utc);
        if (row.RevokedAt.HasValue) row.RevokedAt = DateTime.SpecifyKind(row.RevokedAt.Value, DateTimeKind.Utc);
    }

    public long SubmitFaceEnrollment(byte[] photo, bool consent, HrmUserAccountModel actor, string ip)
    {
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
        if (!consent) throw new InvalidOperationException("Bạn cần đồng ý lưu ảnh để xác minh danh tính chấm công.");
        if (!IsValidFaceEnrollmentPhoto(photo)) throw new InvalidOperationException("Cần ảnh JPEG từ camera, tối đa 2 MB.");
        var protectedPhoto = _faceProtector.Protect(photo);
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        LockFaceOwner(db, tx, actor.Id);
        if (db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.HrmFaceEnrollment WHERE UserId=@Id
                AND StatusCode IN ('PENDING','ACTIVE')", new { actor.Id }, tx) != 0)
            throw new InvalidOperationException("Bạn đã có ảnh đang chờ duyệt hoặc đã kích hoạt. Thu hồi đăng ký hiện tại trước khi đăng ký lại.");
        if (db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.HrmFaceEnrollment WHERE UserId=@Id
                AND CreatedAt>=DATEADD(day,-1,SYSUTCDATETIME())", new { actor.Id }, tx) >= 3 ||
            db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.HrmFaceEnrollment WHERE UserId=@Id
                AND CreatedAt>=DATEADD(second,-60,SYSUTCDATETIME())", new { actor.Id }, tx) > 0)
            throw new InvalidOperationException("Đăng ký quá thường xuyên. Mỗi lần cách nhau ít nhất 1 phút và tối đa 3 lần trong 24 giờ.");
        var id = db.ExecuteScalar<long>(@"INSERT dbo.HrmFaceEnrollment(UserId,PhotoProtected,ConsentVersion)
            OUTPUT INSERTED.Id VALUES(@UserId,@Photo,@Version)",
            new { UserId = actor.Id, Photo = protectedPhoto, Version = FaceEnrollmentConsentVersion }, tx);
        AddAudit(db, actor.Id, "SUBMIT", "FaceEnrollment", id.ToString(), "Đồng ý lưu ảnh xác minh danh tính: " + FaceEnrollmentConsentVersion, ip, tx);
        db.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
            SELECT Id,N'Đăng ký khuôn mặt cần duyệt',@Message,'/FaceEnrollment'
            FROM dbo.HrmUserAccount WHERE IsActive=1 AND RoleCode IN ('ADMIN','HR') AND Id<>@UserId",
            new { UserId = actor.Id, Message = actor.DisplayName + " gửi ảnh đăng ký chấm công để đối chiếu danh tính." }, tx);
        tx.Commit();
        return id;
    }

    public void ReviewFaceEnrollment(long id, bool approve, string note, HrmUserAccountModel actor, string ip)
    {
        if (!CanReviewFaceEnrollment(actor)) throw new UnauthorizedAccessException();
        ValidateReviewNote(note);
        using var db = OpenConnection();
        var ownerId = db.QuerySingleOrDefault<int?>("SELECT UserId FROM dbo.HrmFaceEnrollment WHERE Id=@id", new { id });
        if (!ownerId.HasValue || ownerId.Value == actor.Id)
            throw new InvalidOperationException("Không thể tự duyệt hoặc đăng ký không tồn tại.");
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        LockFaceOwner(db, tx, ownerId.Value);
        var enrollment = db.QuerySingleOrDefault<FaceEnrollmentModel>("SELECT Id,UserId,StatusCode FROM dbo.HrmFaceEnrollment WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id", new { id }, tx);
        if (enrollment?.StatusCode != "PENDING") throw new InvalidOperationException("Đăng ký đã được xử lý. Vui lòng tải lại danh sách.");
        var status = approve ? "ACTIVE" : "REJECTED";
        db.Execute(@"UPDATE dbo.HrmFaceEnrollment SET StatusCode=@Status,ReviewNote=@Note,ReviewedBy=@ActorId,
            ReviewedAt=SYSUTCDATETIME(),PhotoProtected=CASE WHEN @Status='ACTIVE' THEN PhotoProtected ELSE NULL END WHERE Id=@id",
            new { id, Status = status, Note = note.Trim(), ActorId = actor.Id }, tx);
        AddAudit(db, actor.Id, approve ? "APPROVE" : "REJECT", "FaceEnrollment", id.ToString(), note.Trim(), ip, tx);
        NotifyFaceOwner(db, tx, ownerId.Value, approve ? "Đã kích hoạt ảnh đăng ký chấm công" : "Đăng ký khuôn mặt bị từ chối", note.Trim());
        tx.Commit();
    }

    public void RevokeFaceEnrollment(long id, string note, HrmUserAccountModel actor, string ip)
    {
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
        ValidateReviewNote(note);
        using var db = OpenConnection();
        var ownerId = db.QuerySingleOrDefault<int?>("SELECT UserId FROM dbo.HrmFaceEnrollment WHERE Id=@id", new { id });
        if (!ownerId.HasValue || (ownerId != actor.Id && !CanReviewFaceEnrollment(actor)))
            throw new UnauthorizedAccessException();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        LockFaceOwner(db, tx, ownerId.Value, requireActive: false);
        var changed = db.Execute(@"UPDATE dbo.HrmFaceEnrollment SET StatusCode='REVOKED',PhotoProtected=NULL,
            RevokedAt=SYSUTCDATETIME(),RevokedBy=@ActorId,ReviewNote=@Note WHERE Id=@id AND StatusCode IN ('PENDING','ACTIVE')",
            new { id, ActorId = actor.Id, Note = note.Trim() }, tx);
        if (changed != 1) throw new InvalidOperationException("Đăng ký đã được thu hồi hoặc không còn hiệu lực.");
        AddAudit(db, actor.Id, "REVOKE", "FaceEnrollment", id.ToString(), "Xóa ảnh đăng ký. " + note.Trim(), ip, tx);
        if (actor.Id != ownerId.Value) NotifyFaceOwner(db, tx, ownerId.Value, "Đăng ký khuôn mặt đã bị thu hồi", note.Trim());
        tx.Commit();
    }

    public byte[] GetFaceEnrollmentPhoto(long id, HrmUserAccountModel actor)
    {
        if (actor?.IsActive != true) throw new UnauthorizedAccessException();
        using var db = OpenConnection();
        var photo = db.QuerySingleOrDefault<byte[]>(@"SELECT PhotoProtected FROM dbo.HrmFaceEnrollment
            WHERE Id=@id AND StatusCode IN ('PENDING','ACTIVE') AND (UserId=@UserId OR @Reviewer=1)",
            new { id, UserId = actor.Id, Reviewer = CanReviewFaceEnrollment(actor) });
        return photo == null ? null : _faceProtector.Unprotect(photo);
    }

    public static bool IsValidFaceEnrollmentPhoto(byte[] photo) =>
        photo is { Length: >= 4 and <= 2 * 1024 * 1024 } && photo[0] == 0xff && photo[1] == 0xd8 &&
        photo[2] == 0xff && photo[^2] == 0xff && photo[^1] == 0xd9;

    private static void LockFaceOwner(IDbConnection db, IDbTransaction tx, int ownerId, bool requireActive = true)
    {
        var active = db.QuerySingleOrDefault<bool?>("SELECT IsActive FROM dbo.HrmUserAccount WITH(UPDLOCK,HOLDLOCK) WHERE Id=@ownerId", new { ownerId }, tx);
        if (!active.HasValue || (requireActive && !active.Value)) throw new InvalidOperationException("Tài khoản đăng ký không còn hoạt động.");
    }

    private static void NotifyFaceOwner(IDbConnection db, IDbTransaction tx, int userId, string title, string message) =>
        db.Execute("INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl) VALUES(@userId,@title,@message,'/FaceEnrollment')",
            new { userId, title, message }, tx);
}
