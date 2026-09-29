using System.Data;
using Dapper;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    private const string RemoteScope = @"(u.Id=@ActorId OR @All=1 OR
        (@Manager=1 AND u.DepartmentId=@DepartmentId AND u.RoleCode='EMPLOYEE'))";

    private static object RemoteFilter(HrmUserAccountModel actor, DateTime from, DateTime to) => new
    {
        ActorId = actor.Id, All = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director,
        Manager = actor.RoleCode == HrmRoles.Manager, actor.DepartmentId, From = from.Date, To = to.Date
    };

    private static void ValidateRemoteRange(DateTime from, DateTime to)
    {
        if (to < from || (to - from).TotalDays > 100) throw new InvalidOperationException("Khoảng xem tối đa 100 ngày.");
    }

    public IList<RemoteWorkPlan> GetRemotePlans(HrmUserAccountModel actor, DateTime from, DateTime to)
    {
        ValidateRemoteRange(from, to);
        using var db = OpenConnection();
        return db.Query<RemoteWorkPlan>(@"SELECT p.*,u.DisplayName,u.DepartmentId,u.RoleCode FROM dbo.HrmRemoteWorkPlan p
            JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE p.ToDate>=@From AND p.FromDate<=@To AND " + RemoteScope +
            " ORDER BY p.Id DESC", RemoteFilter(actor, from, to)).ToList();
    }

    public IList<RemotePunch> GetRemotePunches(HrmUserAccountModel actor, DateTime from, DateTime to)
    {
        ValidateRemoteRange(from, to);
        using var db = OpenConnection();
        var rows = db.Query<RemotePunch>(@"SELECT TOP (500) p.Id,p.ClientId,p.UserId,p.PlanId,p.Kind,p.CapturedAt,p.CheckTime,
            p.ReceivedAt,p.WasOffline,p.Latitude,p.Longitude,p.AccuracyMeters,p.DistanceMeters,p.PlaceName,p.Note,
            p.StatusCode,p.ReviewReason,p.ReviewNote,u.DisplayName,u.DepartmentId,u.RoleCode
            FROM dbo.HrmRemotePunch p JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
            WHERE p.CheckTime>=@From AND p.CheckTime<DATEADD(day,1,@To) AND " + RemoteScope +
            " ORDER BY p.CheckTime DESC,p.Id DESC", RemoteFilter(actor, from, to)).ToList();
        foreach (var row in rows) row.CanReview = row.StatusCode == "PENDING" && RemoteAttendancePolicy.CanReview(actor, row.UserId, row.DepartmentId, row.RoleCode);
        return rows;
    }

    public long CreateRemotePlan(RemoteWorkPlan plan, HrmUserAccountModel actor, string ip)
    {
        RemoteAttendancePolicy.ValidatePlan(plan);
        if (plan.FromDate.Date < CurrentVietnamTime().Date.AddDays(-7)) throw new InvalidOperationException("Không thể đăng ký lùi quá 7 ngày.");
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        if (plan.LeaveRequestId.HasValue)
        {
            var linked = db.QuerySingleOrDefault<LeaveRequestModel>("SELECT * FROM dbo.HrmLeaveRequest WHERE Id=@Id AND UserId=@UserId",
                new { Id = plan.LeaveRequestId.Value, UserId = actor.Id }, tx);
            if (plan.Mode != "FIELD" || linked == null || linked.StatusCode != "APPROVED" || linked.LeaveType != "Công tác/Ra ngoài" ||
                linked.StartDate.Date > plan.FromDate.Date || linked.EndDate.Date < plan.ToDate.Date)
                throw new InvalidOperationException("Đơn liên kết phải là công tác đã duyệt của bạn, bao phủ thời gian đăng ký.");
        }
        plan.UserId = actor.Id;
        var id = db.ExecuteScalar<long>(@"INSERT dbo.HrmRemoteWorkPlan(UserId,Mode,PlaceName,FromDate,ToDate,WorkDaysMask,
            WindowStart,WindowEnd,IsFlexible,RequiredMinutes,BreakMinutes,Latitude,Longitude,RadiusMeters,LeaveRequestId,Note)
            OUTPUT INSERTED.Id VALUES(@UserId,@Mode,@PlaceName,@FromDate,@ToDate,@WorkDaysMask,@WindowStart,@WindowEnd,
            @IsFlexible,@RequiredMinutes,@BreakMinutes,@Latitude,@Longitude,@RadiusMeters,@LeaveRequestId,@Note)", plan, tx);
        AddAudit(db, actor.Id, "CREATE", "RemoteWorkPlan", id.ToString(), "Đăng ký làm việc ngoài công ty", ip, tx);
        NotifyRemoteReviewers(db, tx, actor.Id, "Đăng ký làm việc ngoài công ty", actor.DisplayName + " gửi đăng ký cần duyệt.");
        tx.Commit();
        return id;
    }

    public void DecideRemotePlan(long id, bool approve, string note, HrmUserAccountModel actor, string ip)
    {
        ValidateReviewNote(note);
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        var plan = db.QuerySingleOrDefault<RemoteWorkPlan>(@"SELECT p.*,u.DepartmentId,u.RoleCode FROM dbo.HrmRemoteWorkPlan p WITH(UPDLOCK,HOLDLOCK)
            JOIN dbo.HrmUserAccount u ON u.Id=p.UserId AND u.IsActive=1 WHERE p.Id=@id", new { id }, tx);
        if (plan == null || plan.StatusCode != "PENDING" || !RemoteAttendancePolicy.CanReview(actor, plan.UserId, plan.DepartmentId, plan.RoleCode))
            throw new InvalidOperationException("Không có quyền duyệt hoặc đăng ký đã được xử lý.");
        if (approve && db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.HrmRemoteWorkPlan WITH(UPDLOCK,HOLDLOCK)
            WHERE UserId=@UserId AND StatusCode='APPROVED' AND FromDate<=@ToDate AND ToDate>=@FromDate AND (WorkDaysMask & @WorkDaysMask)<>0", plan, tx) > 0)
            throw new InvalidOperationException("Nhân viên đã có lịch ngoài công ty được duyệt trùng ngày. Hủy lịch trùng trước khi duyệt.");
        db.Execute("UPDATE dbo.HrmRemoteWorkPlan SET StatusCode=@Status,ReviewNote=@note,ReviewedBy=@Actor,ReviewedAt=SYSDATETIME() WHERE Id=@id",
            new { id, Status = approve ? "APPROVED" : "REJECTED", note, Actor = actor.Id }, tx);
        AddAudit(db, actor.Id, approve ? "APPROVE" : "REJECT", "RemoteWorkPlan", id.ToString(), note, ip, tx);
        NotifyRemoteOwner(db, tx, plan.UserId, approve ? "Lịch ngoài công ty đã được duyệt" : "Lịch ngoài công ty bị từ chối", note);
        tx.Commit();
    }

    public void CancelRemotePlan(long id, HrmUserAccountModel actor, string ip)
    {
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        var plan = db.QuerySingleOrDefault<RemoteWorkPlan>(@"SELECT p.*,u.DepartmentId,u.RoleCode FROM dbo.HrmRemoteWorkPlan p WITH(UPDLOCK)
            JOIN dbo.HrmUserAccount u ON u.Id=p.UserId WHERE p.Id=@id", new { id }, tx);
        if (plan == null || plan.StatusCode is not ("PENDING" or "APPROVED") ||
            (plan.UserId != actor.Id && !RemoteAttendancePolicy.CanReview(actor, plan.UserId, plan.DepartmentId, plan.RoleCode)))
            throw new InvalidOperationException("Không có quyền hủy đăng ký này.");
        db.Execute("UPDATE dbo.HrmRemoteWorkPlan SET StatusCode='CANCELLED' WHERE Id=@id", new { id }, tx);
        AddAudit(db, actor.Id, "CANCEL", "RemoteWorkPlan", id.ToString(), "Hủy hiệu lực lịch; giữ nguyên lượt chấm đã xác minh", ip, tx);
        tx.Commit();
    }

    public object RecordRemotePunch(RemotePunchRequest punch, byte[] photo, HrmUserAccountModel actor, string ip)
    {
        if (punch.OwnerUserId != actor.Id) throw new InvalidOperationException("Bản ghi thuộc tài khoản khác. Đăng nhập lại đúng tài khoản đã chấm.");
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        // Serialize per user, including retries from another tab/device.
        db.ExecuteScalar<int>("SELECT Id FROM dbo.HrmUserAccount WITH(UPDLOCK,HOLDLOCK) WHERE Id=@Id", new { actor.Id }, tx);
        var existing = db.QuerySingleOrDefault<RemotePunch>("SELECT Id,StatusCode,ReviewReason FROM dbo.HrmRemotePunch WHERE UserId=@UserId AND ClientId=@ClientId",
            new { UserId = actor.Id, punch.ClientId }, tx);
        if (existing != null) { tx.Commit(); return new { existing.Id, existing.StatusCode, existing.ReviewReason }; }
        var plan = db.QuerySingleOrDefault<RemoteWorkPlan>("SELECT * FROM dbo.HrmRemoteWorkPlan WITH(HOLDLOCK) WHERE Id=@PlanId AND UserId=@UserId",
            new { punch.PlanId, UserId = actor.Id }, tx);
        if (plan == null) throw new InvalidOperationException("Không tìm thấy đăng ký của bạn.");
        var evaluated = RemoteAttendancePolicy.Evaluate(plan, punch, DateTimeOffset.UtcNow);
        EnsureRemotePeriodOpen(db, tx, evaluated.CheckTime);
        var status = string.IsNullOrEmpty(evaluated.Reason) ? "APPROVED" : "PENDING";
        var id = db.ExecuteScalar<long>(@"INSERT dbo.HrmRemotePunch(ClientId,UserId,PlanId,Kind,CapturedAt,CheckTime,WasOffline,
            Latitude,Longitude,AccuracyMeters,DistanceMeters,PlaceName,Note,PhotoContent,PhotoContentType,StatusCode,ReviewReason,IncludedAt)
            OUTPUT INSERTED.Id VALUES(@ClientId,@UserId,@PlanId,@Kind,@CapturedAt,@CheckTime,@WasOffline,@Latitude,@Longitude,
            @AccuracyMeters,@Distance,@PlaceName,@Note,@Photo,'image/jpeg',@Status,@Reason,CASE WHEN @Status='APPROVED' THEN SYSDATETIME() END)",
            new { punch.ClientId, UserId = actor.Id, punch.PlanId, punch.Kind, punch.CapturedAt, evaluated.CheckTime, punch.WasOffline,
                punch.Latitude, punch.Longitude, punch.AccuracyMeters, evaluated.Distance, punch.PlaceName, punch.Note, Photo = photo, Status = status, evaluated.Reason }, tx);
        if (status == "APPROVED" && punch.Kind != "VISIT") InvalidateRemoteConfirmation(db, tx, actor.Id, evaluated.CheckTime);
        if (status == "PENDING") NotifyRemoteReviewers(db, tx, actor.Id, "Lượt chấm ngoài công ty cần xác minh", actor.DisplayName + ": " + evaluated.Reason);
        AddAudit(db, actor.Id, "CREATE", "RemotePunch", id.ToString(), $"{punch.Kind}: {status}", ip, tx);
        tx.Commit();
        return new { Id = id, StatusCode = status, ReviewReason = evaluated.Reason };
    }

    public void DecideRemotePunch(long id, bool approve, string note, HrmUserAccountModel actor, string ip)
    {
        ValidateReviewNote(note);
        using var db = OpenConnection();
        using var tx = db.BeginTransaction(IsolationLevel.Serializable);
        var item = db.QuerySingleOrDefault<RemotePunch>(@"SELECT p.Id,p.UserId,p.CheckTime,p.Kind,p.StatusCode,u.DepartmentId,u.RoleCode
            FROM dbo.HrmRemotePunch p WITH(UPDLOCK,HOLDLOCK) JOIN dbo.HrmUserAccount u ON u.Id=p.UserId AND u.IsActive=1 WHERE p.Id=@id", new { id }, tx);
        if (item == null || item.StatusCode != "PENDING" || !RemoteAttendancePolicy.CanReview(actor, item.UserId, item.DepartmentId, item.RoleCode))
            throw new InvalidOperationException("Không có quyền duyệt hoặc lượt chấm đã được xử lý.");
        EnsureRemotePeriodOpen(db, tx, item.CheckTime);
        db.Execute(@"UPDATE dbo.HrmRemotePunch SET StatusCode=@Status,ReviewNote=@note,ReviewedBy=@Actor,ReviewedAt=SYSDATETIME(),
            IncludedAt=CASE WHEN @Status='APPROVED' THEN SYSDATETIME() END WHERE Id=@id", new { id, note, Actor = actor.Id, Status = approve ? "APPROVED" : "REJECTED" }, tx);
        if (approve && item.Kind != "VISIT") InvalidateRemoteConfirmation(db, tx, item.UserId, item.CheckTime);
        AddAudit(db, actor.Id, approve ? "APPROVE" : "REJECT", "RemotePunch", id.ToString(), note, ip, tx);
        NotifyRemoteOwner(db, tx, item.UserId, approve ? "Đã xác minh lượt chấm ngoài công ty" : "Lượt chấm ngoài công ty bị từ chối", note);
        tx.Commit();
    }

    public byte[] GetRemotePhoto(long id, HrmUserAccountModel actor)
    {
        using var db = OpenConnection();
        var filter = new DynamicParameters(RemoteFilter(actor, DateTime.Today, DateTime.Today));
        filter.Add("Id", id);
        return db.QuerySingleOrDefault<byte[]>(@"SELECT p.PhotoContent FROM dbo.HrmRemotePunch p JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
            WHERE p.Id=@Id AND " + RemoteScope, filter);
    }

    private static void EnsureRemotePeriodOpen(IDbConnection db, IDbTransaction tx, DateTime day)
    {
        var status = db.QuerySingleOrDefault<string>("SELECT StatusCode FROM dbo.HrmAttendancePeriod WITH(UPDLOCK,HOLDLOCK) WHERE Period=@Period",
            new { Period = day.ToString("yyyy-MM") }, tx);
        if (status == "LOCKED") throw new InvalidOperationException("Kỳ công đã khóa. HR cần mở khóa trước khi tiếp nhận/xác minh lượt chấm.");
    }

    private static void InvalidateRemoteConfirmation(IDbConnection db, IDbTransaction tx, int userId, DateTime day) =>
        db.Execute("DELETE FROM dbo.HrmAttendancePeriodConfirmation WHERE UserId=@userId AND Period=@Period", new { userId, Period = day.ToString("yyyy-MM") }, tx);

    private static void ValidateReviewNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note) || note.Length > 1000) throw new InvalidOperationException("Nhập ghi chú xử lý từ 1 đến 1.000 ký tự.");
    }

    private static void NotifyRemoteOwner(IDbConnection db, IDbTransaction tx, int userId, string title, string message) =>
        db.Execute("INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl) VALUES(@userId,@title,@message,'/RemoteAttendance')", new { userId, title, message }, tx);

    private static void NotifyRemoteReviewers(IDbConnection db, IDbTransaction tx, int ownerId, string title, string message) =>
        db.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
            SELECT r.Id,@title,@message,'/RemoteAttendance' FROM dbo.HrmUserAccount r JOIN dbo.HrmUserAccount u ON u.Id=@ownerId
            WHERE r.IsActive=1 AND r.Id<>u.Id AND (r.RoleCode IN ('ADMIN','HR','DIRECTOR') OR
                (r.RoleCode='MANAGER' AND u.RoleCode='EMPLOYEE' AND r.DepartmentId=u.DepartmentId))", new { ownerId, title, message }, tx);
}
