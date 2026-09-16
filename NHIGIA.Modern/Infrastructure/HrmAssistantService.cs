using System.Globalization;
using System.Text;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public sealed class HrmAssistantService
{
    private readonly HrmDataStore _hrm;
    private readonly WorkItemStore _work;
    private readonly GeminiAssistantClient _gemini;

    public HrmAssistantService(HrmDataStore hrm, WorkItemStore work, GeminiAssistantClient gemini)
    {
        _hrm = hrm;
        _work = work;
        _gemini = gemini;
    }

    public async Task<AssistantAnswer> AskAsync(string question, IReadOnlyList<AssistantChatMessage> history, HrmUserAccountModel actor, CancellationToken cancellationToken = default)
    {
        var grounded = BuildGroundedAnswer(question, actor);
        var generated = await _gemini.GenerateAsync(question, history, grounded, actor, cancellationToken);
        if (!string.IsNullOrWhiteSpace(generated))
        {
            grounded.Answer = generated;
            grounded.UsedGemini = true;
        }
        return grounded;
    }

    private AssistantAnswer BuildGroundedAnswer(string question, HrmUserAccountModel actor)
    {
        var normalized = Normalize(question);
        var suggestions = Suggestions(actor);
        var scope = ScopeLabel(actor);

        if (ContainsAny(normalized, "xin chao", "chao", "hello", "hi", "tro giup", "ho tro", "lam duoc gi"))
            return Reply($"Xin chào {actor.DisplayName}. Tôi có thể tra cứu dữ liệu HRM theo phạm vi {scope}: chấm công, nghỉ phép, lịch làm việc, KPI, tài sản, Helpdesk, đào tạo, tăng ca, đặt phòng và đặt xe.", scope, suggestions: suggestions);

        if (ContainsAny(normalized, "ho so", "thong tin cua toi", "thong tin ca nhan", "phong ban cua toi", "chuc vu"))
        {
            var profile = _hrm.GetEmployeeProfile(actor.Id);
            if (profile == null) return Reply("Chưa tìm thấy hồ sơ nhân viên của bạn trong cơ sở dữ liệu.", scope, "/Home/MyProfile", "Mở hồ sơ", suggestions);
            var hireDate = profile.HireDate.HasValue ? profile.HireDate.Value.ToString("dd/MM/yyyy") : "chưa cập nhật";
            return Reply($"Hồ sơ của bạn: mã nhân viên {Value(profile.EmployeeCode)}, phòng ban {Value(profile.DepartmentName)}, chức danh {Value(profile.JobTitle)}, ngày vào làm {hireDate}, trạng thái {Value(profile.EmploymentStatus)}.", scope, "/Home/MyProfile", "Xem hồ sơ", suggestions);
        }

        if (ContainsAny(normalized, "cham cong", "gio vao", "gio ra", "di muon", "ve som", "co mat hom nay"))
        {
            var today = DateTime.Today;
            var rows = _hrm.GetAttendance(actor, today, today);
            if (actor.RoleCode == HrmRoles.Employee)
            {
                var row = rows.FirstOrDefault(x => x.UserId == actor.Id);
                if (row == null) return Reply("Hôm nay chưa có dữ liệu chấm công của bạn.", scope, "/Home/Attendance", "Mở chấm công", suggestions);
                return Reply($"Chấm công hôm nay: vào {Time(row.CheckIn)}, ra {Time(row.CheckOut)}, trạng thái {AttendanceStatus(row.StatusCode)}. Đi muộn {row.LateMinutes} phút, về sớm {row.EarlyMinutes} phút.", scope, "/Home/Attendance", "Xem chi tiết", suggestions);
            }
            var present = rows.Select(x => x.UserId).Distinct().Count();
            var exceptions = rows.Count(x => x.LateMinutes > 0 || x.EarlyMinutes > 0 || x.StatusCode == "MISSING_CHECK");
            return Reply($"Hôm nay có {present} nhân viên trong phạm vi của bạn có dữ liệu chấm công; {exceptions} bản ghi cần chú ý vì đi muộn, về sớm hoặc thiếu lượt chấm.", scope, "/Home/Attendance", "Xem bảng chấm công", suggestions);
        }

        if (ContainsAny(normalized, "nghi phep", "phep nam", "con bao nhieu phep", "don nghi"))
        {
            var stats = _hrm.GetLeaveStats(actor);
            var requests = _hrm.GetLeaveRequests(actor).Where(x => CanSeeEmployee(actor, x.UserId, x.DepartmentId)).ToList();
            if (actor.RoleCode == HrmRoles.Employee)
                return Reply($"Năm nay bạn đã dùng {stats.UsedDays:0.#}/{stats.AnnualAllowance:0.#} ngày phép, còn {Math.Max(0, stats.AnnualAllowance - stats.UsedDays):0.#} ngày; có {stats.PendingCount} đơn đang chờ và {stats.ApprovedCount} đơn đã duyệt.", scope, "/Home/LeaveRequests", "Xem nghỉ phép", suggestions);
            var pending = requests.Count(x => x.StatusCode is "PENDING_MANAGER" or "PENDING_HR");
            return Reply($"Có {requests.Count} đơn nghỉ phép trong {scope}; {pending} đơn đang chờ xử lý. Phép cá nhân của bạn còn {Math.Max(0, stats.AnnualAllowance - stats.UsedDays):0.#} ngày.", scope, "/Home/Approvals", "Mở phê duyệt", suggestions);
        }

        if (ContainsAny(normalized, "lich lam", "ca lam", "phan ca", "ca cua toi"))
        {
            var schedules = _hrm.GetSchedules(actor).ToList();
            if (actor.RoleCode == HrmRoles.Employee)
            {
                var current = schedules.FirstOrDefault(x => x.UserId == actor.Id && x.EffectiveFrom.Date <= DateTime.Today && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= DateTime.Today));
                return current == null
                    ? Reply("Bạn chưa có ca làm việc đang hiệu lực.", scope, "/Home/WorkSchedules", "Mở lịch làm việc", suggestions)
                    : Reply($"Ca hiện tại của bạn là {current.ShiftName}, từ {current.StartTime:hh\\:mm} đến {current.EndTime:hh\\:mm}, áp dụng từ {current.EffectiveFrom:dd/MM/yyyy}.", scope, "/Home/WorkSchedules", "Xem lịch làm việc", suggestions);
            }
            return Reply($"Có {schedules.Count} lịch làm việc trong {scope}. Bạn có thể mở màn hình phân ca để xem và chỉnh sửa theo quyền.", scope, "/Home/WorkSchedules", "Mở phân ca", suggestions);
        }

        var kind = DetectWorkKind(normalized);
        if (kind != null)
        {
            if (kind == "payroll")
                return Reply("Dữ liệu lương là thông tin nhạy cảm nên chatbot không hiển thị số tiền. Bạn có thể mở phiếu lương hoặc màn hình duyệt lương theo đúng quyền tài khoản.", scope, "/Work?kind=payroll", "Mở bảng lương", suggestions);
            var summary = _work.GetAssistantSummary(kind, actor);
            var label = KindLabel(kind);
            var answer = summary.TotalCount == 0
                ? $"Chưa có dữ liệu {label.ToLowerInvariant()} trong {scope}."
                : $"{label} trong {scope}: tổng {summary.TotalCount}, đang chờ {summary.PendingCount}, đã duyệt {summary.ApprovedCount}, đã hoàn thành {summary.CompletedCount}.";
            return Reply(answer, scope, $"/Work?kind={Uri.EscapeDataString(kind)}", $"Mở {label}", suggestions);
        }

        if (ContainsAny(normalized, "nhan vien", "nhan su", "bao nhieu nguoi", "quan ly phong"))
        {
            if (actor.RoleCode == HrmRoles.Employee)
                return Reply("Tài khoản nhân viên không có quyền tra cứu danh sách nhân sự. Tôi chỉ có thể hỗ trợ dữ liệu cá nhân của bạn.", scope, "/Home/MyProfile", "Mở hồ sơ của tôi", suggestions);
            var users = _hrm.GetVisibleUsers(actor);
            return Reply($"Có {users.Count(x => x.IsActive)} nhân viên đang hoạt động trong {scope}.", scope, "/Home/EmployeeInformation", "Mở quản lý nhân sự", suggestions);
        }

        if (ContainsAny(normalized, "cho duyet", "can duyet", "phe duyet"))
        {
            if (!HrmRoles.CanManagePeople(actor.RoleCode))
                return Reply("Tài khoản của bạn không có quyền phê duyệt yêu cầu.", scope, suggestions: suggestions);
            var leaves = _hrm.GetLeaveRequests(actor).Count(x => CanSeeEmployee(actor, x.UserId, x.DepartmentId) && x.StatusCode is "PENDING_MANAGER" or "PENDING_HR");
            var work = _work.GetPendingApprovals(actor).Count;
            return Reply($"Bạn có {leaves + work} yêu cầu đang chờ xử lý, gồm {leaves} đơn nghỉ phép và {work} yêu cầu nghiệp vụ khác.", scope, "/Home/Approvals", "Mở phê duyệt", suggestions);
        }

        return Reply("Tôi chưa nhận diện được câu hỏi này. Hãy hỏi ngắn gọn về chấm công, nghỉ phép, lịch làm việc, KPI, tài sản, Helpdesk, đào tạo, tăng ca, đặt phòng hoặc đặt xe.", scope, suggestions: suggestions);
    }

    private static AssistantAnswer Reply(string answer, string scope, string linkUrl = null, string linkLabel = null, IReadOnlyList<string> suggestions = null) => new()
    {
        Answer = answer,
        Scope = scope,
        LinkUrl = linkUrl,
        LinkLabel = linkLabel,
        Suggestions = suggestions ?? Array.Empty<string>()
    };

    private static IReadOnlyList<string> Suggestions(HrmUserAccountModel actor)
    {
        var items = new List<string> { "Chấm công hôm nay", "Tôi còn bao nhiêu ngày phép?", "Lịch làm việc của tôi", "KPI của tôi" };
        if (HrmRoles.CanManagePeople(actor.RoleCode)) items.Add("Có bao nhiêu yêu cầu chờ duyệt?");
        return items;
    }

    private static bool CanSeeEmployee(HrmUserAccountModel actor, int userId, int? departmentId) =>
        actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director ||
        userId == actor.Id ||
        actor.RoleCode == HrmRoles.Manager && departmentId == actor.DepartmentId;

    private static string ScopeLabel(HrmUserAccountModel actor) => actor.RoleCode switch
    {
        HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director => "toàn công ty",
        HrmRoles.Manager => $"phòng {Value(actor.DepartmentName)}",
        _ => "dữ liệu cá nhân của bạn"
    };

    private static string DetectWorkKind(string text)
    {
        if (ContainsAny(text, "kpi", "chi tieu")) return "kpi";
        if (ContainsAny(text, "tai san", "thiet bi")) return "assets";
        if (ContainsAny(text, "helpdesk", "ho tro it", "su co it")) return "helpdesk";
        if (ContainsAny(text, "dao tao", "khoa hoc")) return "training";
        if (ContainsAny(text, "tang ca", "lam them")) return "overtime";
        if (ContainsAny(text, "phong hop", "lich hop")) return "meeting";
        if (ContainsAny(text, "dat xe", "chuyen xe")) return "vehicle";
        if (ContainsAny(text, "cong tac")) return "business-trip";
        if (ContainsAny(text, "nghi viec", "thoi viec")) return "resignation";
        if (ContainsAny(text, "luong", "phieu luong")) return "payroll";
        return null;
    }

    private static string KindLabel(string kind) => kind switch
    {
        "kpi" => "KPI", "assets" => "Tài sản", "helpdesk" => "Helpdesk IT", "training" => "Đào tạo",
        "overtime" => "Tăng ca", "meeting" => "Đặt phòng họp", "vehicle" => "Đặt xe",
        "business-trip" => "Phân công công tác", "resignation" => "Nghỉ việc và thôi việc", _ => "Nghiệp vụ"
    };

    private static string AttendanceStatus(string status) => status switch
    {
        "ON_TIME" => "đúng giờ", "LATE" => "đi muộn", "EARLY" => "về sớm", "LATE_EARLY" => "đi muộn và về sớm",
        "MISSING_CHECK" => "thiếu lượt chấm", "MISSING_SCHEDULE" => "chưa có ca làm", _ => "chưa xác định"
    };

    private static string Time(DateTime? value) => value?.ToString("HH:mm") ?? "chưa có";
    private static string Value(string value) => string.IsNullOrWhiteSpace(value) ? "chưa cập nhật" : value;
    private static bool ContainsAny(string text, params string[] values) => values.Any(text.Contains);

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var source = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(source.Length);
        foreach (var c in source)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c == 'đ' ? 'd' : c);
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
