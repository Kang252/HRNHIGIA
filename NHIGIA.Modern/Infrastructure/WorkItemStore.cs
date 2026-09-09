using Dapper;
using Microsoft.Data.SqlClient;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public sealed class WorkItemStore
{
    private readonly IConfiguration _configuration;
    public WorkItemStore(IConfiguration configuration) => _configuration = configuration;
    private SqlConnection Open()
    {
        return DatabaseConfiguration.OpenConnection(_configuration);
    }
    public void Load(WorkPage page, HrmUserAccountModel actor)
    {
        using var db = Open();
        var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
        var isManager = actor.RoleCode == HrmRoles.Manager;
        var canManageDepartment = isManager && page.CanManage;
        page.Items = db.Query<WorkItem>(@"SELECT w.*, u.DisplayName EmployeeName, d.Name DepartmentName,
            STUFF((SELECT N', ' + pu.DisplayName
             FROM dbo.HrmWorkItemParticipant wp
             INNER JOIN dbo.HrmUserAccount pu ON pu.Id=wp.UserId
             WHERE wp.WorkItemId=w.Id ORDER BY pu.DisplayName
             FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,2,N'') ParticipantNames
            FROM dbo.HrmWorkItem w LEFT JOIN dbo.HrmUserAccount u ON u.Id=w.EmployeeId
            LEFT JOIN dbo.HrmDepartment d ON d.Id=COALESCE(w.DepartmentId,u.DepartmentId)
            WHERE w.Kind=@Kind
              AND (@CanSeeAll=1 OR w.EmployeeId=@UserId OR w.CreatedBy=@UserId
                   OR EXISTS (SELECT 1 FROM dbo.HrmWorkItemParticipant wp WHERE wp.WorkItemId=w.Id AND wp.UserId=@UserId)
                   OR (w.Kind='kpi' AND w.DepartmentId=@DepartmentId)
                   OR (@IsManager=1 AND (u.DepartmentId=@DepartmentId OR w.DepartmentId=@DepartmentId)))
              AND (@Kind<>'payroll' OR @CanSeeAll=1 OR w.Status IN ('PUBLISHED','PAID','DISPUTED','RESOLVED'))
            ORDER BY w.CreatedAt DESC, w.Id DESC", new
            {
                page.Kind,
                CanSeeAll = canSeeAll,
                IsManager = canManageDepartment,
                UserId = actor.Id,
                actor.DepartmentId
            }).ToList();
        if (page.CanManage || page.Kind is "meeting" or "vehicle")
        {
            page.People = db.Query<WorkPerson>(@"SELECT u.Id,u.DisplayName,d.Name DepartmentName FROM dbo.HrmUserAccount u
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
                  AND (@ParticipantPicker=1 OR @CanSeeAll=1 OR u.Id=@UserId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY u.DisplayName", new
            {
                ParticipantPicker = page.Kind is "meeting" or "vehicle",
                CanSeeAll = canSeeAll,
                IsManager = canManageDepartment,
                UserId = actor.Id,
                actor.DepartmentId
            }).ToList();
            page.Departments = db.Query<WorkDepartment>(@"SELECT Id,Name FROM dbo.HrmDepartment
                WHERE IsActive=1 AND (@CanSeeAll=1 OR Id=@DepartmentId) ORDER BY Name",
                new { CanSeeAll = canSeeAll, actor.DepartmentId }).ToList();
        }
        if (!string.IsNullOrWhiteSpace(page.Query))
            page.Items = page.Items.Where(x => $"{x.Title} {x.Reference} {x.EmployeeName} {x.ParticipantNames} {x.Category} {x.Location} {x.Destination}".Contains(page.Query, StringComparison.OrdinalIgnoreCase)).ToList();
        page.Available = true;
    }
    public int Create(WorkItem item, IReadOnlyCollection<int> participantIds = null)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var id = db.QuerySingle<int>(@"INSERT dbo.HrmWorkItem
            (Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,StartAt,EndAt,Location,Destination,Target,Actual,Weight,Priority,Status,CreatedBy,AssetInUse)
            OUTPUT INSERTED.Id VALUES
            (@Kind,@Title,@Description,@Category,@Reference,@EmployeeId,@DepartmentId,@DueDate,@StartAt,@EndAt,@Location,@Destination,@Target,@Actual,@Weight,@Priority,@Status,@CreatedBy,@AssetInUse)", item, transaction);
        if (item.Kind is "meeting" or "vehicle" && participantIds?.Count > 0)
        {
            foreach (var userId in participantIds.Distinct())
                db.Execute("INSERT dbo.HrmWorkItemParticipant(WorkItemId,UserId) VALUES(@WorkItemId,@UserId)", new { WorkItemId = id, UserId = userId }, transaction);
            var state = item.Status == "APPROVED" ? "Đã được xác nhận." : "Đang chờ phê duyệt.";
            var title = item.Kind == "meeting" ? $"Lời mời họp: {item.Title}" : $"Thông tin chuyến xe: {item.Title}";
            var route = item.Kind == "meeting" ? item.Location : $"{item.Location} → {item.Destination}";
            AddParticipantNotifications(db, transaction, id, item.Kind, title,
                $"{route} · {item.StartAt:dd/MM/yyyy HH:mm}–{item.EndAt:HH:mm}. {state}");
        }
        transaction.Commit();
        return id;
    }

    public string GetNextAssetReference()
    {
        return GetNextReference("assets", "TS");
    }

    public string GetNextOffboardingReference()
    {
        return GetNextReference("offboarding", "TV");
    }

    private string GetNextReference(string kind, string prefix)
    {
        using var db = Open();
        var sequence = db.ExecuteScalar<int>(@"SELECT ISNULL(MAX(TRY_CONVERT(INT,SUBSTRING(Reference,4,97))),0)+1
            FROM dbo.HrmWorkItem WHERE Kind=@Kind AND Reference LIKE @PrefixPattern",
            new { Kind = kind, PrefixPattern = $"{prefix}.%" });
        return $"{prefix}.{sequence:D3}";
    }

    public int CreateAsset(WorkItem item)
    {
        SqlException duplicate = null;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            item.Reference = GetNextAssetReference();
            try { return Create(item); }
            catch (SqlException exception) when (exception.Number is 2601 or 2627) { duplicate = exception; }
        }
        throw new InvalidOperationException("Không thể cấp mã tài sản duy nhất sau nhiều lần thử.", duplicate);
    }

    public bool HasMeetingConflict(string location, DateTime startAt, DateTime endAt)
    {
        using var db = Open();
        return db.ExecuteScalar<int>(@"SELECT COUNT(1) FROM dbo.HrmWorkItem
            WHERE Kind='meeting' AND Location=@Location AND Status IN ('PENDING','APPROVED')
              AND StartAt < DATEADD(MINUTE, 10, @EndAt)
              AND EndAt > DATEADD(MINUTE, -10, @StartAt)", new { Location = location, StartAt = startAt, EndAt = endAt }) > 0;
    }

    public bool TransitionBooking(int id, string kind, string expectedStatus, string newStatus, string note, int actorId, string ipAddress)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var changed = db.Execute(@"UPDATE dbo.HrmWorkItem SET Status=@NewStatus, LastActionNote=@Note, UpdatedAt=SYSUTCDATETIME()
            WHERE Id=@Id AND Kind=@Kind AND Status=@ExpectedStatus",
            new { Id = id, Kind = kind, ExpectedStatus = expectedStatus, NewStatus = newStatus, Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim() }, transaction) > 0;
        if (changed)
        {
            AddAudit(db, transaction, actorId, newStatus, id, $"{kind}: {expectedStatus} -> {newStatus}. {note}".Trim(), ipAddress);
            if (kind is "meeting" or "vehicle")
            {
                var item = db.QuerySingle<WorkItem>("SELECT * FROM dbo.HrmWorkItem WHERE Id=@Id", new { Id = id }, transaction);
                var statusLabel = newStatus switch { "APPROVED" => "đã được duyệt", "REJECTED" => "đã bị từ chối", "CANCELLED" => "đã bị hủy", _ => "đã được cập nhật" };
                var subject = kind == "meeting" ? "Lịch họp" : "Chuyến xe";
                var route = kind == "meeting" ? item.Location : $"{item.Location} → {item.Destination}";
                AddParticipantNotifications(db, transaction, id, kind, $"{subject} {statusLabel}: {item.Title}",
                    $"{route} · {item.StartAt:dd/MM/yyyy HH:mm}–{item.EndAt:HH:mm}. {note}".Trim());
            }
        }
        transaction.Commit();
        return changed;
    }

    public IReadOnlyList<WorkNotification> GetNotifications(int userId)
    {
        using var db = Open();
        return db.Query<WorkNotification>(@"SELECT TOP (100) Id,Title,Message,LinkUrl,IsRead,CreatedAt
            FROM dbo.HrmNotification WHERE UserId=@UserId ORDER BY CreatedAt DESC,Id DESC", new { UserId = userId }).ToList();
    }

    public int GetUnreadNotificationCount(int userId)
    {
        using var db = Open();
        return db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmNotification WHERE UserId=@UserId AND IsRead=0", new { UserId = userId });
    }

    public IReadOnlyList<WorkItem> GetPendingApprovals(HrmUserAccountModel actor)
    {
        using var db = Open();
        var canSeeAll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
        var canApprovePayroll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Director;
        return db.Query<WorkItem>(@"SELECT w.*,u.DisplayName EmployeeName,d.Name DepartmentName,
                STUFF((SELECT N', ' + pu.DisplayName FROM dbo.HrmWorkItemParticipant wp
                 INNER JOIN dbo.HrmUserAccount pu ON pu.Id=wp.UserId WHERE wp.WorkItemId=w.Id
                 ORDER BY pu.DisplayName FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,2,N'') ParticipantNames
            FROM dbo.HrmWorkItem w
            LEFT JOIN dbo.HrmUserAccount u ON u.Id=w.EmployeeId
            LEFT JOIN dbo.HrmDepartment d ON d.Id=COALESCE(w.DepartmentId,u.DepartmentId)
            WHERE ((w.Status='PENDING' AND w.Kind IN ('overtime','resignation','vehicle','meeting','business-trip','offboarding','transfer'))
                   OR (w.Status='PENDING_APPROVAL' AND w.Kind='payroll' AND @CanApprovePayroll=1))
              AND (@CanSeeAll=1 OR (@IsManager=1 AND w.Kind<>'transfer'
                   AND (u.DepartmentId=@DepartmentId OR w.DepartmentId=@DepartmentId OR w.CreatedBy=@ActorId)))
            ORDER BY w.CreatedAt,w.Id", new
        {
            CanSeeAll = canSeeAll,
            CanApprovePayroll = canApprovePayroll,
            IsManager = actor.RoleCode == HrmRoles.Manager,
            actor.DepartmentId,
            ActorId = actor.Id
        }).ToList();
    }

    public string ReadNotification(long id, int userId)
    {
        using var db = Open();
        var link = db.QuerySingleOrDefault<string>("SELECT LinkUrl FROM dbo.HrmNotification WHERE Id=@Id AND UserId=@UserId", new { Id = id, UserId = userId });
        if (link != null) db.Execute("UPDATE dbo.HrmNotification SET IsRead=1 WHERE Id=@Id AND UserId=@UserId", new { Id = id, UserId = userId });
        return link;
    }

    public bool UpdateKpi(WorkItem item, int actorId, string ipAddress)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var changed = db.Execute(@"UPDATE dbo.HrmWorkItem SET Title=@Title, Description=@Description,
                Category=@Category, Reference=@Reference, EmployeeId=@EmployeeId, DepartmentId=@DepartmentId,
                DueDate=@DueDate, Target=@Target, Actual=@Actual, Weight=@Weight, UpdatedAt=SYSUTCDATETIME()
            WHERE Id=@Id AND Kind='kpi'", item, transaction) > 0;
        if (changed) AddAudit(db, transaction, actorId, "UPDATE", item.Id, "Cập nhật KPI", ipAddress);
        transaction.Commit();
        return changed;
    }

    public int UpdateAssets(IReadOnlyCollection<int> ids, string command, int? employeeId, string note, int actorId, string ipAddress)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var status = command switch
        {
            "allocate" => "ASSIGNED", "recover" => "AVAILABLE", "maintenance" => "MAINTENANCE",
            "damaged" => "DAMAGED", "lost" => "LOST", "dispose" => "DISPOSED", _ => null
        };
        if (status == null) return 0;
        var changedIds = db.Query<int>(@"UPDATE dbo.HrmWorkItem SET
                Status=CASE WHEN @Command='recover' AND AssetInUse>1 THEN 'ASSIGNED' ELSE @Status END,
                EmployeeId=CASE WHEN @Command='allocate' THEN @EmployeeId
                    WHEN @Command='recover' AND AssetInUse>1 THEN EmployeeId
                    WHEN @Command IN ('recover','maintenance','damaged','lost','dispose') THEN NULL ELSE EmployeeId END,
                AssetInUse=CASE WHEN @Command='allocate' THEN AssetInUse+1 WHEN @Command='recover' THEN AssetInUse-1 ELSE AssetInUse END,
                AssetMaintenance=AssetMaintenance+CASE WHEN @Command='maintenance' THEN 1 ELSE 0 END,
                AssetDamaged=AssetDamaged+CASE WHEN @Command='damaged' THEN 1 ELSE 0 END,
                AssetLost=AssetLost+CASE WHEN @Command='lost' THEN 1 ELSE 0 END,
                AssetDisposed=AssetDisposed+CASE WHEN @Command='dispose' THEN 1 ELSE 0 END,
                LastActionNote=@Note,UpdatedAt=SYSUTCDATETIME()
            OUTPUT INSERTED.Id
            WHERE Kind='assets' AND Id IN @Ids
              AND ((@Command='allocate' AND COALESCE(Target,1)>AssetInUse+AssetMaintenance+AssetDamaged+AssetLost+AssetDisposed)
                OR (@Command='recover' AND AssetInUse>0)
                OR (@Command IN ('maintenance','damaged','lost','dispose')
                    AND COALESCE(Target,1)>AssetInUse+AssetMaintenance+AssetDamaged+AssetLost+AssetDisposed))",
            new { Ids = ids, Command = command, Status = status, EmployeeId = employeeId, Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim() }, transaction).ToList();
        foreach (var id in changedIds)
            AddAudit(db, transaction, actorId, command.ToUpperInvariant(), id, $"Tài sản -> {status}. {note}".Trim(), ipAddress);
        transaction.Commit();
        return changedIds.Count;
    }

    public bool UpdatePayrollDraft(WorkItem item, int actorId, string ipAddress)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var changed = db.Execute(@"UPDATE dbo.HrmWorkItem SET Title=@Title, Description=@Description,
                Category=@Category, Reference=@Reference, EmployeeId=@EmployeeId, DueDate=@DueDate,
                Target=@Target, Actual=@Actual, UpdatedAt=SYSUTCDATETIME()
            WHERE Id=@Id AND Kind='payroll' AND Status IN ('DRAFT','REJECTED')", item, transaction) > 0;
        if (changed) AddAudit(db, transaction, actorId, "UPDATE", item.Id, "Cập nhật bảng lương nháp", ipAddress);
        transaction.Commit();
        return changed;
    }

    public bool TransitionPayroll(int id, string expectedStatus, string newStatus, string action, string note, int actorId, int? employeeId, string ipAddress)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction();
        var changed = db.Execute(@"UPDATE dbo.HrmWorkItem SET Status=@NewStatus,
                LastActionNote=@Note, UpdatedAt=SYSUTCDATETIME()
            WHERE Id=@Id AND Kind='payroll' AND Status=@ExpectedStatus
              AND (@EmployeeId IS NULL OR EmployeeId=@EmployeeId)",
            new { Id = id, ExpectedStatus = expectedStatus, NewStatus = newStatus, Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(), EmployeeId = employeeId }, transaction) > 0;
        if (changed) AddAudit(db, transaction, actorId, action, id, $"Bảng lương: {expectedStatus} -> {newStatus}. {note}".Trim(), ipAddress);
        transaction.Commit();
        return changed;
    }

    private static void AddAudit(SqlConnection db, SqlTransaction transaction, int actorId, string action, int id, string detail, string ipAddress)
    {
        db.Execute(@"INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
            VALUES(@UserId,@Action,'HrmWorkItem',@EntityId,@Detail,@IpAddress)",
            new { UserId = actorId, Action = action, EntityId = id.ToString(), Detail = detail, IpAddress = ipAddress }, transaction);
    }

    private static void AddParticipantNotifications(SqlConnection db, SqlTransaction transaction, int workItemId, string kind, string title, string message)
    {
        db.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
            SELECT UserId,@Title,@Message,@LinkUrl
            FROM dbo.HrmWorkItemParticipant WHERE WorkItemId=@WorkItemId",
            new { WorkItemId = workItemId, Title = title.Length > 200 ? title[..200] : title, Message = message.Length > 1000 ? message[..1000] : message, LinkUrl = $"/Work?kind={kind}" }, transaction);
    }
}
