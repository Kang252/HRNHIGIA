using System.ComponentModel.DataAnnotations;

namespace NHIGIA.Modern.Models;

public sealed class WorkItem
{
    public int Id { get; set; }
    [Required] public string Kind { get; set; }
    [Required, StringLength(200)] public string Title { get; set; }
    [StringLength(2000)] public string Description { get; set; }
    [StringLength(100)] public string Category { get; set; }
    [StringLength(100)] public string Reference { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    [StringLength(250)] public string Location { get; set; }
    [StringLength(250)] public string Destination { get; set; }
    [Range(0, 1000000000)] public decimal? Target { get; set; }
    [Range(0, 1000000000)] public decimal? Actual { get; set; }
    [Range(1, 100)] public decimal? Weight { get; set; }
    public int AssetInUse { get; set; }
    public int AssetMaintenance { get; set; }
    public int AssetLost { get; set; }
    public int AssetDisposed { get; set; }
    public int AssetDamaged { get; set; }
    [StringLength(20)] public string Priority { get; set; } = "NORMAL";
    public string Status { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string LastActionNote { get; set; }
    public string EmployeeName { get; set; }
    public string DepartmentName { get; set; }
    public string ParticipantNames { get; set; }
    public List<int> ParticipantIds { get; set; } = new();

    public string RecordCode => FormatCode(Kind, Id);

    public static string FormatCode(string kind, int id)
    {
        var prefix = kind?.ToLowerInvariant() switch
        {
            "kpi" => "KPI", "payroll" => "BL", "recruitment" => "TD",
            "training" => "DT", "overtime" => "TC", "resignation" => "NV",
            "transfer" => "DC", "assets" => "TS", "helpdesk" => "HD",
            "vehicle" => "XE", "meeting" => "PH", "business-trip" => "CT", "offboarding" => "TV",
            _ => "HS"
        };
        return $"{prefix}{id:D8}";
    }
}

public sealed class WorkPage
{
    public string Kind { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public bool CanCreate { get; set; }
    public bool CanManage { get; set; }
    public bool Available { get; set; }
    public string Query { get; set; }
    public int? EditId { get; set; }
    public WorkItem Draft { get; set; } = new();
    public List<WorkItem> Items { get; set; } = new();
    public List<WorkPerson> People { get; set; } = new();
    public List<WorkDepartment> Departments { get; set; } = new();
}
public sealed class WorkPerson { public int Id { get; set; } public string DisplayName { get; set; } public string DepartmentName { get; set; } }
public sealed class WorkDepartment { public int Id { get; set; } public string Name { get; set; } }

public sealed class WorkNotification
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
