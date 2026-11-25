using System.Globalization;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

/// <summary>One waiting decision, with what the approver needs to decide without opening another page.</summary>
public sealed class ApprovalInboxItem
{
    public string Source { get; init; }
    public string Kind { get; init; }
    public int Id { get; init; }
    public string Key => $"{Source}:{Kind}:{Id}";
    public string Code { get; init; }
    public string RequestType { get; init; }
    public string Title { get; init; }
    public string EmployeeName { get; init; }
    public string DepartmentName { get; init; }
    public string When { get; init; }
    public IReadOnlyList<string> Facts { get; init; } = [];
    public string Description { get; init; }
    public string AttachmentName { get; init; }
    public bool HasAttachment { get; init; }
    public DateTime CreatedAt { get; init; }
    /// <summary>Set when the decision is taken on another page, because it covers many records at once.</summary>
    public string Url { get; init; }
    /// <summary>Money and personnel decisions are always taken one by one.</summary>
    public bool IsRoutine { get; init; }
}

public static class ApprovalInbox
{
    private static readonly string[] RoutineRequests = ["Công tác/Ra ngoài", "Đi muộn về sớm", "Suất ăn", "Đổi ca làm", "Ca làm", "Đăng ký ca làm"];
    private static readonly string[] RoutineKinds = ["overtime", "meeting", "vehicle", "business-trip"];

    public static ApprovalInboxItem From(LeaveRequestModel item)
    {
        var facts = new List<string>();
        if (item.RequestedDays.HasValue) facts.Add($"{Number(item.RequestedDays.Value)} ngày công · {item.SessionCode}");
        if (item.AdjustKind != null) facts.Add($"Xin miễn trừ {(item.AdjustKind == "EARLY" ? "về sớm" : "đi muộn")} {item.AdjustMinutes} phút");
        if (item.Amount > 0) facts.Add($"Số tiền {Money(item.Amount.Value)}");
        if (item.RequestedDays.HasValue && !string.IsNullOrWhiteSpace(item.HandoverTo)) facts.Add($"Bàn giao: {item.HandoverTo}");
        facts.Add(item.StatusCode == "PENDING_HR" ? "Bước: HR/Giám đốc duyệt" : "Bước: Trưởng phòng duyệt");
        return new ApprovalInboxItem
        {
            Source = "leave", Kind = "leave", Id = item.Id, Code = item.RequestCode, RequestType = item.LeaveType, Title = item.LeaveType,
            EmployeeName = item.DisplayName, DepartmentName = item.DepartmentName, When = Range(item.StartDate, item.EndDate, false),
            Facts = facts, Description = item.Reason, AttachmentName = item.AttachmentName, HasAttachment = item.HasAttachment,
            CreatedAt = item.CreatedAt,
            IsRoutine = RoutineRequests.Contains(item.LeaveType?.Trim(), StringComparer.OrdinalIgnoreCase) ||
                AttendanceLeavePolicy.LeaveTypes.Contains(item.LeaveType?.Trim(), StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>A payroll period is one decision, however many payslips it holds.</summary>
    public static IEnumerable<ApprovalInboxItem> FromWork(IEnumerable<WorkItem> items)
    {
        var list = items.ToList();
        var payrolls = list.Where(x => x.Kind == "payroll").GroupBy(x => x.Quarter).Select(period => new ApprovalInboxItem
        {
            Source = "work", Kind = "payroll", Id = period.Min(x => x.Id), Code = period.Key, RequestType = KindLabel("payroll"),
            Title = $"Bảng lương kỳ {period.Key}", EmployeeName = $"Bảng lương kỳ {period.Key}", When = $"{period.Count()} nhân viên",
            Facts = [$"Tổng thực nhận {Money(period.Sum(x => x.Actual ?? 0))}", "Bước: Ban Giám đốc duyệt"],
            Description = "Mở bảng lương để đối chiếu từng nhân viên rồi duyệt hoặc trả lại cả kỳ.",
            CreatedAt = period.Max(x => x.UpdatedAtLocal ?? x.CreatedAtLocal), Url = $"/Work?kind=payroll&pyPeriod={Uri.EscapeDataString(period.Key ?? "")}"
        });
        return list.Where(x => x.Kind != "payroll").Select(From).Concat(payrolls);
    }

    public static ApprovalInboxItem From(WorkItem item) => new()
    {
        Source = "work", Kind = item.Kind, Id = item.Id, Code = item.RecordCode, RequestType = KindLabel(item.Kind), Title = item.Title,
        EmployeeName = item.EmployeeName, DepartmentName = item.Kind == "transfer" ? null : item.DepartmentName, When = item.Kind switch
        {
            "resignation" or "offboarding" => item.DueDate is { } last ? $"Ngày làm cuối {Date(last)}" : null,
            "transfer" => item.DueDate is { } effective ? $"Hiệu lực {Date(effective)}" : null,
            _ => item.StartAt is { } start && item.EndAt is { } end ? Range(start, end, true) : item.DueDate is { } due ? Date(due) : null
        },
        Facts = Facts(item).Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
        Description = item.Description, CreatedAt = item.CreatedAtLocal, IsRoutine = RoutineKinds.Contains(item.Kind)
    };

    private static IEnumerable<string> Facts(WorkItem item)
    {
        switch (item.Kind)
        {
            case "overtime":
                if (item.Target.HasValue) yield return $"{Number(item.Target.Value)} giờ";
                yield return item.Category + (item.Weight > 0 ? $" · hệ số {Number(item.Weight.Value)}" : "");
                break;
            case "vehicle":
                yield return $"{item.Category} · {Number(item.Target ?? 0)} người";
                yield return $"{item.Location} → {item.Destination}";
                if (!string.IsNullOrWhiteSpace(item.ParticipantNames)) yield return $"Người đi: {item.ParticipantNames}";
                break;
            case "meeting":
                yield return $"{item.Location} · {Number(item.Target ?? 0)} người";
                if (!string.IsNullOrWhiteSpace(item.ParticipantNames)) yield return $"Tham dự: {item.ParticipantNames}";
                break;
            case "business-trip":
                yield return string.IsNullOrWhiteSpace(item.Destination) ? item.Location : $"Nơi đến: {item.Destination}";
                break;
            case "transfer":
                yield return $"Phòng ban mới: {item.DepartmentName}";
                break;
        }
    }

    public static string KindLabel(string kind) => kind switch
    {
        "overtime" => "Tăng ca", "resignation" => "Nghỉ việc", "vehicle" => "Đặt xe",
        "meeting" => "Đặt phòng họp", "business-trip" => "Công tác", "offboarding" => "Thôi việc",
        "transfer" => "Điều chuyển", "payroll" => "Bảng lương", _ => "Yêu cầu"
    };

    private static string Range(DateTime start, DateTime end, bool withTime)
    {
        if (!withTime) return start.Date == end.Date ? Date(start) : $"{Date(start)} – {Date(end)}";
        return start.Date == end.Date
            ? $"{Date(start)} · {start:HH:mm}–{end:HH:mm}"
            : $"{Date(start)} {start:HH:mm} – {Date(end)} {end:HH:mm}";
    }

    private static string Date(DateTime value) => value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    private static string Number(decimal value) => value.ToString("0.##", VietnamFormat.Numbers);
    private static string Money(decimal value) => value.ToString("N0", VietnamFormat.Numbers) + " đ";
}
