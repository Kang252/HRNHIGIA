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
}
