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
        var connection = new SqlConnection(DatabaseConfiguration.Resolve(_configuration));
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }
    public void Load(WorkPage page, int userId)
    {
        using var db = Open();
        page.Items = db.Query<WorkItem>(@"SELECT w.*, u.DisplayName EmployeeName, d.Name DepartmentName
            FROM dbo.HrmWorkItem w LEFT JOIN dbo.HrmUserAccount u ON u.Id=w.EmployeeId
            LEFT JOIN dbo.HrmDepartment d ON d.Id=w.DepartmentId
            WHERE w.Kind=@Kind AND (@Manage=1 OR w.EmployeeId=@UserId OR w.CreatedBy=@UserId)
              AND (@Kind<>'payroll' OR @Manage=1 OR w.Status IN ('PUBLISHED','PAID','DISPUTED','RESOLVED'))
            ORDER BY w.CreatedAt DESC, w.Id DESC", new { page.Kind, Manage = page.CanManage, UserId = userId }).ToList();
        if (page.CanManage)
        {
            page.People = db.Query<WorkPerson>("SELECT Id,DisplayName FROM dbo.HrmUserAccount WHERE IsActive=1 ORDER BY DisplayName").ToList();
            page.Departments = db.Query<WorkDepartment>("SELECT Id,Name FROM dbo.HrmDepartment WHERE IsActive=1 ORDER BY Name").ToList();
        }
        if (!string.IsNullOrWhiteSpace(page.Query))
            page.Items = page.Items.Where(x => $"{x.Title} {x.Reference} {x.EmployeeName} {x.Category}".Contains(page.Query, StringComparison.OrdinalIgnoreCase)).ToList();
        page.Available = true;
    }
    public int Create(WorkItem item)
    {
        using var db = Open();
        return db.QuerySingle<int>(@"INSERT dbo.HrmWorkItem
            (Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,Target,Actual,Weight,Priority,Status,CreatedBy)
            OUTPUT INSERTED.Id VALUES
            (@Kind,@Title,@Description,@Category,@Reference,@EmployeeId,@DepartmentId,@DueDate,@Target,@Actual,@Weight,@Priority,@Status,@CreatedBy)", item);
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
}
