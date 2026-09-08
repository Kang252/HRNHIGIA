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

    private WorkPage Page(string kind, string q = null)
    {
        var configure = User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr) || User.IsInRole(HrmRoles.Director);
        var managePeople = configure || User.IsInRole(HrmRoles.Manager);
        var page = new WorkPage
        {
            Kind = kind,
            CanManage = kind is "training" or "overtime" or "resignation" ? managePeople : configure,
            CanCreate = kind switch
            {
                "helpdesk" or "overtime" or "resignation" => true,
                "training" => managePeople,
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
        return page.Title == null ? null : page;
    }
    private void Load(WorkPage page)
    {
        ViewBag.Title = page.Title;
        try { _store.Load(page, _user.Current.Id); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load work module {Kind}", page.Kind);
            page.Available = false;
            ModelState.AddModelError("", "Chưa kết nối được cơ sở dữ liệu. Không thể tải hoặc lưu dữ liệu lúc này.");
        }
    }
    [HttpGet]
    public IActionResult Index(string kind = "kpi", string q = null)
    {
        var page = Page(kind, q);
        if (page == null) return NotFound();
        if (kind is "transfer" or "payroll" or "recruitment" && !page.CanManage) return Forbid();
        Load(page);
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
        if (draft.Kind == "kpi" && (!draft.Target.HasValue || draft.Target <= 0 || !draft.Weight.HasValue || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "KPI cần mục tiêu lớn hơn 0, trọng số và hạn hoàn thành.");
        if (draft.Kind == "transfer" && (!draft.DepartmentId.HasValue || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "Vui lòng chọn phòng ban mới và ngày dự kiến.");
        if (draft.Kind == "payroll" && (!draft.EmployeeId.HasValue || string.IsNullOrWhiteSpace(draft.Category) || !draft.Target.HasValue || !draft.Actual.HasValue || !draft.DueDate.HasValue))
            ModelState.AddModelError("", "Kỳ lương cần nhân viên, kỳ tính lương, tổng thu nhập, thực nhận và ngày thanh toán.");
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
}
