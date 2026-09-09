using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[Authorize]
public sealed class WorkController : Controller
{
    private readonly WorkItemStore _store;
    private readonly HrmUserAccessor _user;
    private readonly ILogger<WorkController> _logger;
    public WorkController(WorkItemStore store, HrmUserAccessor user, ILogger<WorkController> logger)
    { _store = store; _user = user; _logger = logger; }

    private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    private WorkPage Page(string kind, string q = null)
    {
        var configure = User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr) || User.IsInRole(HrmRoles.Director);
        var managePeople = configure || User.IsInRole(HrmRoles.Manager);
        var page = new WorkPage
        {
            Kind = kind,
            CanManage = kind is "kpi" or "training" or "overtime" or "resignation" or "assets" ? managePeople : configure,
            CanCreate = kind switch
            {
                "helpdesk" or "overtime" or "resignation" => true,
                "training" => managePeople,
                "payroll" => User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr),
                _ => configure
            },
            Query = q
        };
        (page.Title, page.Subtitle) = kind switch
        {
            "kpi" => ("KPI", "Ghi nhận chỉ tiêu, kết quả và trọng số theo từng nhân viên."),
            "payroll" => ("Tính lương", "Theo dõi kỳ lương, tổng thu nhập, khấu trừ và số tiền thực nhận."),
            "recruitment" => ("Tuyển dụng", "Quản lý nhu cầu tuyển dụng và tiến độ ứng viên theo từng vị trí."),
            "training" => ("Quản lý đào tạo", "Lập kế hoạch khóa học và theo dõi nhân viên tham gia."),
            "overtime" => ("Tăng ca", "Đăng ký thời gian làm thêm và theo dõi trạng thái phê duyệt."),
            "resignation" => ("Quản lý nghỉ việc", "Tiếp nhận đề nghị nghỉ việc và ngày làm việc cuối cùng dự kiến."),
            "transfer" => ("Điều chuyển nhân sự", "Theo dõi đề nghị điều chuyển. Hồ sơ nhân viên chỉ thay đổi sau quy trình phê duyệt riêng."),
            "assets" => ("Quản lý tài sản", "Danh mục tài sản, người sử dụng và tình trạng bàn giao."),
            "helpdesk" => ("Helpdesk IT", "Gửi yêu cầu hỗ trợ và theo dõi các yêu cầu của bạn."),
            _ => (null, null)
        };
        if (kind == "payroll" && !configure)
            (page.Title, page.Subtitle) = ("Phiếu lương", "Xem phiếu lương đã phát hành và gửi phản hồi khi có sai lệch.");
        if (User.IsInRole(HrmRoles.Employee))
        {
            (page.Title, page.Subtitle) = kind switch
            {
                "kpi" => ("KPI của tôi", "Theo dõi mục tiêu, kết quả và tiến độ KPI cá nhân."),
                "training" => ("Đào tạo của tôi", "Theo dõi các khóa đào tạo được phân công."),
                "overtime" => ("Đăng ký tăng ca", "Gửi đăng ký làm thêm và theo dõi trạng thái phê duyệt."),
                "resignation" => ("Yêu cầu nghỉ việc", "Gửi đề nghị nghỉ việc và theo dõi quá trình xử lý."),
                "assets" => ("Tài sản của tôi", "Xem thiết bị và tài sản đang được bàn giao cho bạn."),
                _ => (page.Title, page.Subtitle)
            };
        }
        else if (User.IsInRole(HrmRoles.Manager))
        {
            (page.Title, page.Subtitle) = kind switch
            {
                "kpi" => ("KPI phòng ban", "Theo dõi KPI của nhân viên thuộc phòng ban bạn quản lý."),
                "assets" => ("Tài sản phòng ban", "Theo dõi tài sản được bàn giao cho nhân viên trong phòng ban."),
                _ => (page.Title, page.Subtitle)
            };
        }
        else if (kind == "payroll" && User.IsInRole(HrmRoles.Director))
        {
            (page.Title, page.Subtitle) = ("Duyệt bảng lương", "Kiểm tra bảng lương do HR trình và phê duyệt trước khi phát hành.");
        }
        return page.Title == null ? null : page;
    }
    private void Load(WorkPage page)
    {
        ViewBag.Title = page.Title;
        try { _store.Load(page, _user.Current); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load work module {Kind}", page.Kind);
            page.Available = false;
            ModelState.AddModelError("", "Chưa kết nối được cơ sở dữ liệu. Không thể tải hoặc lưu dữ liệu lúc này.");
        }
    }
    [HttpGet]
    public IActionResult Index(string kind = "kpi", string q = null, int? editId = null)
    {
        var page = Page(kind, q);
        if (page == null) return NotFound();
        if (kind is "transfer" or "recruitment" && !page.CanManage) return Forbid();
        page.EditId = editId;
        Load(page);
        if (kind == "payroll" && editId.HasValue)
        {
            if (!(User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr))) return Forbid();
            var item = page.Items.FirstOrDefault(x => x.Id == editId && x.Status is "DRAFT" or "REJECTED");
            if (item == null) return NotFound();
            page.Draft = item;
        }
        return View(page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,Target,Actual,Weight,Priority")] WorkItem draft)
    {
        var page = Page(draft.Kind);
        if (page == null) return NotFound();
        if (!page.CanCreate) return Forbid();
        page.Draft = draft;
        Load(page);
        if (draft.EmployeeId.HasValue && !page.People.Any(x => x.Id == draft.EmployeeId))
            ModelState.AddModelError("", "Nhân viên được chọn không hợp lệ.");
        if (draft.Kind is "kpi" or "transfer" or "payroll" or "overtime" or "resignation" && !draft.EmployeeId.HasValue && page.CanManage)
            ModelState.AddModelError("", "Vui lòng chọn nhân viên.");
        if (draft.Kind == "kpi" && (!draft.Target.HasValue || draft.Target <= 0 || !draft.Weight.HasValue || !draft.DueDate.HasValue
            || string.IsNullOrWhiteSpace(draft.Reference) || string.IsNullOrWhiteSpace(draft.Category) || string.IsNullOrWhiteSpace(draft.Description)))
            ModelState.AddModelError("", "KPI cần mã KPI, đơn vị đo, chỉ tiêu, tỷ trọng, hạn hoàn thành và cách đo kết quả.");
        if (draft.Kind == "kpi" && draft.EmployeeId.HasValue && draft.Weight.HasValue)
        {
            var assignedWeight = page.Items
                .Where(item => item.Kind == "kpi" && item.EmployeeId == draft.EmployeeId)
                .Sum(item => item.Weight ?? 0);
            if (assignedWeight + draft.Weight > 100)
                ModelState.AddModelError("", $"Tổng tỷ trọng KPI của nhân viên không được vượt 100% (hiện có {assignedWeight:N0}%).");
        }
        if (draft.Kind == "transfer" && (!draft.DepartmentId.HasValue || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "Vui lòng chọn phòng ban mới và ngày dự kiến.");
        if (draft.Kind == "payroll" && (!draft.EmployeeId.HasValue || string.IsNullOrWhiteSpace(draft.Category) || !draft.Target.HasValue || !draft.Actual.HasValue || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "Kỳ lương cần nhân viên, kỳ tính lương, tổng thu nhập, thực nhận và ngày thanh toán.");
        if (draft.Kind == "payroll" && draft.Actual > draft.Target)
            ModelState.AddModelError("", "Số tiền thực nhận không được lớn hơn tổng thu nhập.");
        if (draft.Kind == "recruitment" && (!draft.DepartmentId.HasValue || !draft.Target.HasValue || draft.Target <= 0 || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "Nhu cầu tuyển dụng cần phòng ban, số lượng và hạn tuyển.");
        if (draft.Kind == "training" && !draft.DueDate.HasValue)
            ModelState.AddModelError("", "Vui lòng nhập ngày tổ chức đào tạo.");
        if (draft.Kind == "overtime" && (!draft.Target.HasValue || draft.Target <= 0 || !draft.DueDate.HasValue || string.IsNullOrWhiteSpace(draft.Description)))
            ModelState.AddModelError("", "Đăng ký tăng ca cần ngày, số giờ và lý do.");
        if (draft.Kind == "resignation" && (!draft.DueDate.HasValue || string.IsNullOrWhiteSpace(draft.Description)))
            ModelState.AddModelError("", "Đề nghị nghỉ việc cần ngày làm việc cuối cùng và lý do.");
        if (draft.DepartmentId.HasValue && !page.Departments.Any(x => x.Id == draft.DepartmentId))
            ModelState.AddModelError("", "Phòng ban không hợp lệ.");
        if (draft.Kind == "assets" && string.IsNullOrWhiteSpace(draft.Reference))
            ModelState.AddModelError("", "Vui lòng nhập mã tài sản.");
        if (draft.Kind == "helpdesk" && (string.IsNullOrWhiteSpace(draft.Description) || !new[] { "LOW", "NORMAL", "HIGH", "URGENT" }.Contains(draft.Priority)))
            ModelState.AddModelError("", "Vui lòng nhập mô tả và mức ưu tiên hợp lệ.");
        if (!page.Available || !ModelState.IsValid) return View("Index", page);
        draft.CreatedBy = _user.Current.Id;
        draft.Status = draft.Kind switch
        {
            "assets" => draft.EmployeeId.HasValue ? "ASSIGNED" : "AVAILABLE",
            "transfer" or "overtime" or "resignation" => "PENDING",
            "kpi" => "TRACKING",
            "payroll" => "DRAFT",
            "training" => "PLANNED",
            _ => "OPEN"
        };
        if (draft.Kind == "helpdesk" || (!page.CanManage && draft.Kind is "overtime" or "resignation"))
            draft.EmployeeId = _user.Current.Id;
        try
        {
            var id = _store.Create(draft);
            TempData["WorkSuccess"] = $"Đã lưu #{id:D5} vào hệ thống.";
            return RedirectToAction("Index", new { kind = draft.Kind });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot save work item");
            ModelState.AddModelError("", "Không thể lưu dữ liệu. Vui lòng thử lại; mã tài sản có thể đã tồn tại.");
            return View("Index", page);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult UpdatePayroll([Bind("Id,Kind,Title,Description,Category,Reference,EmployeeId,DueDate,Target,Actual")] WorkItem draft)
    {
        if (draft.Kind != "payroll") return NotFound();
        if (!(User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr))) return Forbid();
        var page = Page("payroll");
        page.EditId = draft.Id;
        page.Draft = draft;
        Load(page);
        ValidatePayroll(draft, page);
        if (!page.Available || !ModelState.IsValid) return View("Index", page);
        if (!_store.UpdatePayrollDraft(draft, _user.Current.Id, ClientIp))
        {
            ModelState.AddModelError("", "Bảng lương không còn ở trạng thái có thể chỉnh sửa.");
            return View("Index", page);
        }
        TempData["WorkSuccess"] = $"Đã cập nhật bảng lương #{draft.Id:D5}.";
        return RedirectToAction("Index", new { kind = "payroll" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PayrollAction(int id, string command, string note)
    {
        var role = _user.Current.RoleCode;
        var isAdmin = role == HrmRoles.Admin;
        var isHr = role == HrmRoles.Hr;
        var isDirector = role == HrmRoles.Director;
        var rule = command?.ToLowerInvariant() switch
        {
            "submit" when isAdmin || isHr => ("DRAFT", "PENDING_APPROVAL", "SUBMIT", (int?)null),
            "resubmit" when isAdmin || isHr => ("REJECTED", "PENDING_APPROVAL", "SUBMIT", (int?)null),
            "approve" when isAdmin || isDirector => ("PENDING_APPROVAL", "APPROVED", "APPROVE", (int?)null),
            "reject" when isAdmin || isDirector => ("PENDING_APPROVAL", "REJECTED", "REJECT", (int?)null),
            "lock" when isAdmin || isHr => ("APPROVED", "LOCKED", "LOCK", (int?)null),
            "publish" when isAdmin || isHr => ("LOCKED", "PUBLISHED", "PUBLISH", (int?)null),
            "pay" when isAdmin || isHr => ("PUBLISHED", "PAID", "PAY", (int?)null),
            "dispute" => ("PUBLISHED", "DISPUTED", "DISPUTE", (int?)_user.Current.Id),
            "disputepaid" => ("PAID", "DISPUTED", "DISPUTE", (int?)_user.Current.Id),
            "resolve" when isAdmin || isHr => ("DISPUTED", "RESOLVED", "RESOLVE", (int?)null),
            _ => ((string)null, null, null, (int?)null)
        };
        if (rule.Item1 == null) return Forbid();
        if ((command is "reject" or "dispute" or "disputepaid") && string.IsNullOrWhiteSpace(note))
        {
            TempData["WorkError"] = "Vui lòng nhập lý do trước khi xử lý.";
            return RedirectToAction("Index", new { kind = "payroll" });
        }
        if (!_store.TransitionPayroll(id, rule.Item1, rule.Item2, rule.Item3, note, _user.Current.Id, rule.Item4, ClientIp))
            TempData["WorkError"] = "Bảng lương đã đổi trạng thái hoặc bạn không có quyền xử lý.";
        else
            TempData["WorkSuccess"] = $"Đã cập nhật trạng thái bảng lương #{id:D5}.";
        return RedirectToAction("Index", new { kind = "payroll" });
    }

    private void ValidatePayroll(WorkItem draft, WorkPage page)
    {
        if (!draft.EmployeeId.HasValue || string.IsNullOrWhiteSpace(draft.Category) || !draft.Target.HasValue || !draft.Actual.HasValue || !draft.DueDate.HasValue)
            ModelState.AddModelError("", "Kỳ lương cần nhân viên, kỳ tính lương, tổng thu nhập, thực nhận và ngày thanh toán.");
        if (draft.EmployeeId.HasValue && !page.People.Any(x => x.Id == draft.EmployeeId))
            ModelState.AddModelError("", "Nhân viên được chọn không hợp lệ.");
        if (draft.Target < 0 || draft.Actual < 0 || draft.Actual > draft.Target)
            ModelState.AddModelError("", "Số tiền thực nhận phải từ 0 đến tổng thu nhập.");
    }
}
