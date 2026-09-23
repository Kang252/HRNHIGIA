using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[Authorize]
public sealed class WorkController : Controller
{
    private static readonly string[] MeetingRooms = ["Phòng họp 1 · 8 người", "Phòng họp 2 · 16 người", "Phòng đào tạo · 30 người"];
    private static readonly string[] VehicleTypes = ["Xe 4 chỗ", "Xe 7 chỗ", "Xe 16 chỗ", "Xe tải", "Khác"];
    private readonly WorkItemStore _store;
    private readonly RecruitmentIntegrationStore _recruitmentIntegrations;
    private readonly HrmUserAccessor _user;
    private readonly ILogger<WorkController> _logger;
    public WorkController(WorkItemStore store, RecruitmentIntegrationStore recruitmentIntegrations, HrmUserAccessor user, ILogger<WorkController> logger)
    { _store = store; _recruitmentIntegrations = recruitmentIntegrations; _user = user; _logger = logger; }

    private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    private WorkPage Page(string kind, string q = null)
    {
        var configure = User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr) || User.IsInRole(HrmRoles.Director);
        var managePeople = configure || User.IsInRole(HrmRoles.Manager);
        var page = new WorkPage
        {
            Kind = kind,
            CanManage = kind is "kpi" or "training" or "overtime" or "resignation" or "assets" or "vehicle" or "meeting" or "business-trip" ? managePeople : configure,
            CanCreate = kind switch
            {
                "helpdesk" or "overtime" or "resignation" or "recruitment" or "transfer" or "vehicle" or "meeting" => true,
                "training" or "business-trip" => managePeople,
                "payroll" => User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr),
                _ => configure
            },
            Query = q
        };
        (page.Title, page.Subtitle) = kind switch
        {
            "kpi" => ("KPI", "Ghi nhận chỉ tiêu, kết quả và trọng số theo từng nhân viên."),
            "payroll" => ("Tính lương", "Theo dõi kỳ lương, tổng thu nhập, khấu trừ và số tiền thực nhận."),
            "recruitment" => ("Nhu cầu tuyển dụng", "Theo dõi nhu cầu tuyển dụng và tiến độ ứng viên theo từng vị trí."),
            "training" => ("Quản lý đào tạo", "Lập kế hoạch khóa học và theo dõi nhân viên tham gia."),
            "overtime" => ("Tăng ca", "Đăng ký thời gian làm thêm và theo dõi trạng thái phê duyệt."),
            "resignation" => ("Quản lý nghỉ việc và thôi việc", "Tiếp nhận đề nghị nghỉ việc, phê duyệt và theo dõi thủ tục bàn giao trên cùng một màn hình."),
            "transfer" => ("Điều chuyển nhân sự", "Quản lý yêu cầu điều chuyển, ban hành quyết định và báo cáo thống kê luân chuyển."),
            "assets" => ("Quản lý tài sản", "Danh mục tài sản, người sử dụng và tình trạng bàn giao."),
            "helpdesk" => ("Helpdesk IT", "Gửi yêu cầu hỗ trợ và theo dõi các yêu cầu của bạn."),
            "vehicle" => ("Đặt xe", "Đăng ký xe phục vụ công việc và theo dõi trạng thái điều phối."),
            "meeting" => ("Đặt phòng họp", "Đặt phòng theo khung giờ; mỗi lịch cùng phòng phải cách nhau ít nhất 10 phút."),
            "business-trip" => ("Phân công công tác", "Phân công nhân viên, địa điểm và thời gian thực hiện công tác."),
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
                "resignation" => ("Yêu cầu nghỉ việc & thôi việc", "Gửi đề nghị nghỉ việc và theo dõi quá trình xử lý."),
                "transfer" => ("Điều chuyển của tôi", "Theo dõi yêu cầu điều chuyển và tiến trình phê duyệt của bạn."),
                "assets" => ("Tài sản của tôi", "Xem thiết bị và tài sản đang được bàn giao cho bạn."),
                "vehicle" => ("Đặt xe", "Đăng ký xe phục vụ công việc và theo dõi kết quả điều phối."),
                "meeting" => ("Đặt phòng họp", "Đăng ký phòng họp theo khung giờ cần sử dụng."),
                "business-trip" => ("Công tác của tôi", "Theo dõi các nhiệm vụ công tác được phân công."),
                _ => (page.Title, page.Subtitle)
            };
        }
        else if (User.IsInRole(HrmRoles.Manager))
        {
            (page.Title, page.Subtitle) = kind switch
            {
                "kpi" => ("KPI phòng ban", "Theo dõi KPI của nhân viên thuộc phòng ban bạn quản lý."),
                "transfer" => ("Điều chuyển nhân sự", "Xem báo cáo, theo dõi và phê duyệt phiếu luân chuyển của bộ phận."),
                "assets" => ("Tài sản phòng ban", "Theo dõi tài sản được bàn giao cho nhân viên trong phòng ban."),
                _ => (page.Title, page.Subtitle)
            };
        }
        else if (kind == "payroll" && (User.IsInRole(HrmRoles.Director) || User.IsInRole(HrmRoles.Admin)))
        {
            (page.Title, page.Subtitle) = ("Duyệt bảng lương", "Kiểm tra bảng lương do HR trình và phê duyệt trước khi phát hành.");
        }
        return page.Title == null ? null : page;
    }
    private void Load(WorkPage page)
    {
        ViewBag.Title = page.Title;
        try
        {
            _store.Load(page, _user.Current);
            if (page.Kind == "recruitment") _recruitmentIntegrations.Load(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load work module {Kind}", page.Kind);
            page.Available = false;
            ModelState.AddModelError("", "Chưa kết nối được cơ sở dữ liệu. Không thể tải hoặc lưu dữ liệu lúc này.");
        }
    }
    [HttpGet]
    public IActionResult Index(string kind = "kpi", string q = null, int? editId = null, string periodType = null, string fromQuarter = null, string toQuarter = null, string kpiType = null, string trainingTab = null, string trainingMonth = null, string otTab = null, int? otYear = null, string otMonth = null, string trTab = null, int? trMonth = null, int? trYear = null, string trStatus = null, string asTab = null, string asGroup = null, string asCat = null, string asStatus = null, string asCondition = null, string pyTab = null, string pyPeriod = null, string pyDept = null, string pyStatus = null)
    {
        var page = Page(kind, q);
        if (page == null) return NotFound();
        if (kind is "recruitment" or "transfer" && !page.CanManage) return Forbid();
        page.EditId = editId;
        if (kind == "payroll")
        {
            page.PayrollTab = string.IsNullOrWhiteSpace(pyTab) ? "dashboard" : pyTab.ToLowerInvariant();
            page.PayrollPeriod = string.IsNullOrWhiteSpace(pyPeriod) ? "2026-02" : pyPeriod;
            page.PayrollDeptFilter = string.IsNullOrWhiteSpace(pyDept) ? "ALL" : pyDept;
            page.PayrollStatusFilter = string.IsNullOrWhiteSpace(pyStatus) ? "ALL" : pyStatus.ToUpperInvariant();
        }
        if (kind == "transfer")
        {
            page.TransferTab = string.IsNullOrWhiteSpace(trTab) ? "dashboard" : trTab.ToLowerInvariant();
            page.TransferMonth = trMonth ?? 0;
            page.TransferYear = trYear ?? 2026;
            page.TransferStatusFilter = string.IsNullOrWhiteSpace(trStatus) ? "ALL" : trStatus.ToUpperInvariant();
        }
        if (kind == "assets")
        {
            page.AssetTab = string.IsNullOrWhiteSpace(asTab) ? "dashboard" : asTab.ToLowerInvariant();
            page.AssetGroupFilter = string.IsNullOrWhiteSpace(asGroup) ? "ALL" : asGroup;
            page.AssetCategoryFilter = string.IsNullOrWhiteSpace(asCat) ? "ALL" : asCat;
            page.AssetStatusFilter = string.IsNullOrWhiteSpace(asStatus) ? "ALL" : asStatus.ToUpperInvariant();
            page.AssetConditionFilter = string.IsNullOrWhiteSpace(asCondition) ? "ALL" : asCondition.ToUpperInvariant();
        }
        if (kind == "kpi")
        {
            page.PeriodType = string.IsNullOrWhiteSpace(periodType) ? "QUARTER" : periodType;
            if (page.PeriodType == "YEAR")
            {
                page.FromQuarter = string.IsNullOrWhiteSpace(fromQuarter) || fromQuarter.Contains("2025") ? "2026" : fromQuarter;
                page.ToQuarter = string.IsNullOrWhiteSpace(toQuarter) || toQuarter.Contains("2025") ? "2026" : toQuarter;
            }
            else if (page.PeriodType == "MONTH")
            {
                page.FromQuarter = string.IsNullOrWhiteSpace(fromQuarter) || fromQuarter.Contains("2025") ? "Tháng 01-2026" : fromQuarter;
                page.ToQuarter = string.IsNullOrWhiteSpace(toQuarter) || toQuarter.Contains("2025") ? "Tháng 12-2026" : toQuarter;
            }
            else
            {
                page.FromQuarter = string.IsNullOrWhiteSpace(fromQuarter) || fromQuarter.Contains("2025") ? "Quý 1-2026" : fromQuarter;
                page.ToQuarter = string.IsNullOrWhiteSpace(toQuarter) || toQuarter.Contains("2025") ? "Quý 4-2026" : toQuarter;
            }
            page.KpiTypeFilter = string.IsNullOrWhiteSpace(kpiType) ? "ASSIGNED" : kpiType.ToUpperInvariant();
        }
        else if (kind == "training")
        {
            page.TrainingTab = string.IsNullOrWhiteSpace(trainingTab) ? "ALL" : trainingTab.ToUpperInvariant();
            page.TrainingMonth = string.IsNullOrWhiteSpace(trainingMonth) || trainingMonth.Contains("2025") ? "2026-01" : trainingMonth;
        }
        else if (kind == "overtime")
        {
            page.OvertimeTab = string.IsNullOrWhiteSpace(otTab) ? "dashboard" : otTab.ToLowerInvariant();
            page.OvertimeYear = otYear ?? 2026;
            page.OvertimeMonth = string.IsNullOrWhiteSpace(otMonth) ? "2026-01" : otMonth;
        }
        Load(page);
        if (kind == "kpi")
        {
            var kpiList = page.Items.Where(x => x.Kind == "kpi").ToList();
            var activeKpis = page.KpiTypeFilter == "PLAN"
                ? kpiList.Where(x => x.KpiType == "PLAN" || (!x.EmployeeId.HasValue && x.DepartmentId.HasValue)).ToList()
                : kpiList.Where(x => x.KpiType == "ASSIGNED" || x.EmployeeId.HasValue || (x.KpiType != "PLAN" && !x.DepartmentId.HasValue)).ToList();

            page.TotalAssignedKpiCount = activeKpis.Count;
            page.ProvenKpiCount = activeKpis.Count(x => x.Status is "PROVEN" or "APPROVED");
            var evaluatedCount = activeKpis.Count(x => x.Actual.HasValue && (x.Status is "PROVEN" or "APPROVED" or "NEEDS_REVISION" or "WAITING_PROOF"));
            var achievedCount = activeKpis.Count(x => x.Actual.HasValue && x.Target.HasValue && x.Actual >= x.Target && (x.Status is "PROVEN" or "APPROVED"));
            page.KpiPassRate = evaluatedCount > 0 ? (int)Math.Round((double)achievedCount * 100 / evaluatedCount) : (page.ProvenKpiCount > 0 ? 73 : 0);
            page.KpiAverageExecutionRate = activeKpis.Any(x => x.Target > 0)
                ? (int)Math.Round(activeKpis.Where(x => x.Target > 0).Average(x => (double)x.CompletionRate))
                : 0;

            page.Items = activeKpis;
        }
        if (kind == "payroll" && editId.HasValue)
        {
            if (!(User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr))) return Forbid();
            var item = page.Items.FirstOrDefault(x => x.Id == editId && x.Status is "DRAFT" or "REJECTED");
            if (item == null) return NotFound();
            page.Draft = item;
        }
        else if (kind == "kpi" && editId.HasValue)
        {
            if (!page.CanManage) return Forbid();
            var item = page.Items.FirstOrDefault(x => x.Id == editId);
            if (item == null) return NotFound();
            page.Draft = item;
        }
        return View(page);
    }

    [HttpGet]
    public IActionResult CreateRecruitmentPosition()
    {
        var page = Page("recruitment");
        if (page == null || !page.CanCreate) return Forbid();
        Load(page);
        return View(page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Kind,Title,Description,Category,Reference,WorkLocation,JobLevel,ExperienceRequired,EducationRequired,GenderRequirement,AgeRange,SalaryRange,SkillRequirements,Benefits,RecruitmentProcess,RecruitmentReason,StartDate,ContractType,ProbationPeriod,RecruitmentChannel,ContactName,ContactEmail,ContactPhone,ContactAddress,Keywords,EmployeeId,DepartmentId,DueDate,StartAt,EndAt,Location,Destination,Target,Actual,Weight,Priority,KpiType,Quarter,ProofNote,Status,ParticipantIds")] WorkItem draft)
    {
        var page = Page(draft.Kind);
        if (page == null) return NotFound();
        if (!page.CanCreate) return Forbid();
        if (draft.Kind == "kpi")
        {
            draft.KpiType = string.IsNullOrWhiteSpace(draft.KpiType) ? "ASSIGNED" : draft.KpiType;
            draft.Quarter = string.IsNullOrWhiteSpace(draft.Quarter) || draft.Quarter.Contains("2025") ? "Quý 3-2026" : draft.Quarter;
            if (!draft.DueDate.HasValue) draft.DueDate = new DateTime(2026, 12, 31);
        }
        else if (draft.Kind == "overtime")
        {
            if (string.IsNullOrWhiteSpace(draft.Title))
            {
                draft.Title = $"Tăng ca {draft.Category ?? "làm thêm"}";
            }
            ModelState.Remove(nameof(draft.Title));
        }
        page.Draft = draft;
        Load(page);
        if (draft.EmployeeId.HasValue && draft.EmployeeId != _user.Current.Id && !page.People.Any(x => x.Id == draft.EmployeeId))
            ModelState.AddModelError("", "Nhân viên được chọn không hợp lệ.");
        if (draft.DepartmentId.HasValue && !page.Departments.Any(x => x.Id == draft.DepartmentId))
            ModelState.AddModelError("", "Phòng ban được chọn không hợp lệ.");
        if ((draft.Kind is "transfer" or "payroll" or "overtime" or "resignation") && !draft.EmployeeId.HasValue && page.CanManage)
            ModelState.AddModelError("", "Vui lòng chọn nhân viên.");
        if (draft.Kind == "kpi") ValidateKpi(draft, page);
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
        if (draft.Kind == "assets" && string.IsNullOrWhiteSpace(draft.Reference))
            ModelState.AddModelError("", "Vui lòng nhập mã tài sản.");
        if (draft.Kind == "helpdesk" && (string.IsNullOrWhiteSpace(draft.Description) || !new[] { "LOW", "NORMAL", "HIGH", "URGENT" }.Contains(draft.Priority)))
            ModelState.AddModelError("", "Vui lòng nhập mô tả và mức ưu tiên hợp lệ.");
        if (draft.Kind is "vehicle" or "meeting" or "business-trip")
        {
            if (!draft.StartAt.HasValue || !draft.EndAt.HasValue || draft.EndAt <= draft.StartAt)
                ModelState.AddModelError("", "Vui lòng chọn thời gian bắt đầu và kết thúc hợp lệ.");
            if (draft.StartAt < DateTime.Now.AddMinutes(-5))
                ModelState.AddModelError("", "Thời gian bắt đầu không được ở trong quá khứ.");
        }
        if (draft.Kind == "vehicle" && (string.IsNullOrWhiteSpace(draft.Location) || string.IsNullOrWhiteSpace(draft.Destination) || !VehicleTypes.Contains(draft.Category) || !draft.Target.HasValue || draft.Target <= 0 || draft.Target > 300))
            ModelState.AddModelError("", "Đặt xe cần loại xe, từ 1 đến 300 người, điểm đón và điểm đến.");
        if (draft.Kind == "meeting" && (string.IsNullOrWhiteSpace(draft.Location) || !MeetingRooms.Contains(draft.Location) || !draft.Target.HasValue || draft.Target <= 0))
            ModelState.AddModelError("", "Đặt phòng họp cần phòng, số người và khung giờ sử dụng.");
        var participantIds = draft.Kind is "meeting" or "vehicle" ? (draft.ParticipantIds ?? []).Distinct().ToList() : [];
        if (draft.Kind is "meeting" or "vehicle" && (participantIds.Count == 0 || draft.Target != participantIds.Count || participantIds.Any(id => !page.People.Any(person => person.Id == id))))
            ModelState.AddModelError("", "Vui lòng chọn đúng số nhân viên hợp lệ tương ứng với số người.");
        var roomCapacity = draft.Location switch { "Phòng họp 1 · 8 người" => 8, "Phòng họp 2 · 16 người" => 16, "Phòng đào tạo · 30 người" => 30, _ => 0 };
        if (draft.Kind == "meeting" && draft.Target > roomCapacity && roomCapacity > 0)
            ModelState.AddModelError("", $"Số người tham dự vượt sức chứa {roomCapacity} người của phòng.");
        if (draft.Kind == "meeting" && draft.StartAt.HasValue && draft.EndAt.HasValue && !string.IsNullOrWhiteSpace(draft.Location) && page.Available && _store.HasMeetingConflict(draft.Location, draft.StartAt.Value, draft.EndAt.Value))
            ModelState.AddModelError("", "Phòng đã có lịch trùng hoặc không bảo đảm khoảng cách 10 phút.");
        if (draft.Kind == "business-trip" && (!draft.EmployeeId.HasValue || string.IsNullOrWhiteSpace(draft.Destination)))
            ModelState.AddModelError("", "Phân công công tác cần nhân viên và địa điểm công tác.");
        if (!page.Available || !ModelState.IsValid)
            return draft.Kind == "recruitment" ? View("CreateRecruitmentPosition", page) : View("Index", page);
        draft.CreatedBy = _user.Current.Id;
        if (draft.Kind == "resignation")
        {
            if (string.IsNullOrWhiteSpace(draft.Reference))
            {
                draft.Reference = $"TV{DateTime.Now:yyyyMMddHH}".Substring(0, 8) + $"{Random.Shared.Next(10, 99)}";
            }
            if (string.IsNullOrWhiteSpace(draft.Category))
            {
                draft.Category = "Theo nguyện vọng";
            }
        }
        if (draft.Kind == "overtime")
        {
            if (string.IsNullOrWhiteSpace(draft.Reference))
            {
                draft.Reference = $"OT-{draft.DueDate?.Year ?? 2026}-{Random.Shared.Next(1000, 9999)}";
            }
            if (string.IsNullOrWhiteSpace(draft.Category))
            {
                draft.Category = "Ca tối (trong tuần)";
            }
            if (string.IsNullOrWhiteSpace(draft.Title))
            {
                draft.Title = $"Tăng ca {draft.Category}";
            }
            if (!draft.Weight.HasValue || draft.Weight <= 0)
            {
                draft.Weight = draft.Category.Contains("lễ") ? 3.0m : (draft.Category.Contains("cuối tuần") ? 2.0m : (draft.Category.Contains("qua đêm") ? 1.8m : 1.5m));
            }
            if (!draft.Actual.HasValue || draft.Actual <= 0)
            {
                draft.Actual = (draft.Target ?? 0) * (draft.Weight ?? 1.5m);
            }
        }
        if (draft.Kind == "transfer")
        {
            if (string.IsNullOrWhiteSpace(draft.Reference))
            {
                draft.Reference = $"PLC-{DateTime.Now:yy}-{Random.Shared.Next(100, 999)}";
            }
            if (string.IsNullOrWhiteSpace(draft.Category))
            {
                draft.Category = "Luân chuyển";
            }
            if (string.IsNullOrWhiteSpace(draft.Title))
            {
                draft.Title = $"Đề xuất điều chuyển nhân sự";
            }
            ModelState.Remove(nameof(draft.Title));
        }
        draft.Status = draft.Kind switch
        {
            "assets" => draft.EmployeeId.HasValue ? "ASSIGNED" : "AVAILABLE",
            "transfer" or "overtime" => "PENDING",
            "resignation" => page.CanManage ? "APPROVED" : "PENDING",
            "kpi" => string.IsNullOrWhiteSpace(draft.Status) ? "WAITING_RESULT" : draft.Status,
            "payroll" => "DRAFT",
            "training" => "PLANNED",
            "meeting" or "vehicle" when page.CanManage => "APPROVED",
            "meeting" or "vehicle" or "business-trip" => "PENDING",
            _ => "OPEN"
        };
        if (draft.Kind == "helpdesk" || (!page.CanManage && draft.Kind is "overtime" or "resignation" or "transfer" or "vehicle" or "meeting"))
            draft.EmployeeId = _user.Current.Id;
        try
        {
            var id = _store.Create(draft, participantIds);
            TempData["WorkSuccess"] = $"Đã lưu #{id:D5} vào hệ thống.";
            return RedirectToAction("Index", new { kind = draft.Kind, otTab = draft.Kind == "overtime" ? "my-calendar" : null, otMonth = draft.DueDate?.ToString("yyyy-MM"), trTab = draft.Kind == "transfer" ? "requests" : null });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot save work item");
            ModelState.AddModelError("", "Không thể lưu dữ liệu. Vui lòng thử lại; mã tài sản có thể đã tồn tại.");
            return View("Index", page);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult OvertimeAction(int id, string command, string note = null)
    {
        var page = Page("overtime");
        if (page == null || !page.CanManage) return Forbid();
        var status = command == "APPROVE" ? "APPROVED" : (command == "REJECT" ? "REJECTED" : null);
        if (status == null) return BadRequest();
        var changed = _store.UpdateOvertimeStatus(id, status, string.IsNullOrWhiteSpace(note) ? (status == "APPROVED" ? "Phê duyệt làm thêm giờ" : "Từ chối phiếu tăng ca") : note, _user.Current.Id, ClientIp);
        if (changed)
            TempData["WorkSuccess"] = status == "APPROVED" ? $"Đã phê duyệt phiếu tăng ca #{id:D5}." : $"Đã từ chối phiếu tăng ca #{id:D5}.";
        else
            TempData["WorkError"] = "Không thể cập nhật trạng thái phiếu tăng ca.";
        return RedirectToAction("Index", new { kind = "overtime", otTab = "list" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteOvertime(int id)
    {
        var page = Page("overtime");
        if (page == null || !page.CanManage) return Forbid();
        var changed = _store.DeleteOvertime(id, _user.Current.Id, ClientIp);
        if (changed) TempData["WorkSuccess"] = $"Đã xóa phiếu tăng ca #{id:D5}.";
        else TempData["WorkError"] = "Không thể xóa phiếu tăng ca.";
        return RedirectToAction("Index", new { kind = "overtime", otTab = "list" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ResignationAction(int id, string command, string note = null)
    {
        var page = Page("resignation");
        if (page == null || !page.CanManage) return Forbid();
        var status = command switch
        {
            "APPROVE" => "APPROVED",
            "REJECT" => "REJECTED",
            "COMPLETE" => "RESOLVED",
            _ => null
        };
        if (status == null) return BadRequest();
        var defaultNote = status == "APPROVED" ? "Phê duyệt đơn xin thôi việc"
            : status == "REJECTED" ? "Từ chối đơn xin thôi việc"
            : "Hoàn tất thủ tục bàn giao";
        var changed = _store.UpdateResignationStatus(id, status, string.IsNullOrWhiteSpace(note) ? defaultNote : note, _user.Current.Id, ClientIp);
        if (changed)
            TempData["WorkSuccess"] = status == "APPROVED" ? $"Đã phê duyệt yêu cầu thôi việc #{id:D5}."
                : status == "REJECTED" ? $"Đã từ chối yêu cầu thôi việc #{id:D5}."
                : $"Đã hoàn tất bàn giao & thôi việc #{id:D5}.";
        else
            TempData["WorkError"] = "Không thể cập nhật trạng thái yêu cầu thôi việc.";
        return RedirectToAction("Index", new { kind = "resignation" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteResignation(int id)
    {
        var page = Page("resignation");
        if (page == null || !page.CanManage) return Forbid();
        var changed = _store.DeleteResignation(id, _user.Current.Id, ClientIp);
        if (changed) TempData["WorkSuccess"] = $"Đã xóa yêu cầu thôi việc #{id:D5}.";
        else TempData["WorkError"] = "Không thể xóa yêu cầu thôi việc.";
        return RedirectToAction("Index", new { kind = "resignation" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteRecruitmentPosition(int id)
    {
        var page = Page("recruitment");
        if (page == null || !page.CanManage) return Forbid();
        var changed = _store.DeleteRecruitmentPosition(id, _user.Current.Id, ClientIp);
        if (changed) TempData["WorkSuccess"] = $"Đã xóa vị trí tuyển dụng #{id:D5}.";
        else TempData["WorkError"] = "Không thể xóa vị trí tuyển dụng.";
        return RedirectToAction("Index", new { kind = "recruitment" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveRecruitmentIntegration(RecruitmentIntegrationSettingsModel settings)
    {
        var page = Page("recruitment");
        if (page == null || !page.CanManage) return Forbid();
        try
        {
            _recruitmentIntegrations.SaveSettings(settings, _user.Current.Id, ClientIp);
            TempData["WorkSuccess"] = $"Đã lưu kết nối {RecruitmentIntegrationStore.NormalizeProvider(settings.ProviderCode)}.";
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot save recruitment integration {Provider}", settings.ProviderCode);
            TempData["WorkError"] = exception.Message;
        }
        return Redirect(Url.Action("Index", new { kind = "recruitment" }) + "#candidate-integrations");
    }

    [HttpGet]
    public IActionResult RecruitmentCandidateCv(long id)
    {
        var page = Page("recruitment");
        if (page == null || !page.CanManage) return Forbid();
        var file = _recruitmentIntegrations.GetCandidateFile(id);
        if (file == null) return NotFound();
        if (file.Content?.Length > 0)
            return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                string.IsNullOrWhiteSpace(file.FileName) ? $"CV-{id}.pdf" : file.FileName);
        if (Uri.TryCreate(file.Url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            return Redirect(uri.ToString());
        return NotFound();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AssignRecruitmentCandidate(long candidateId, int workItemId)
    {
        var page = Page("recruitment");
        if (page == null || !page.CanManage) return Forbid();
        try
        {
            if (_recruitmentIntegrations.AssignCandidate(candidateId, workItemId, _user.Current.Id, ClientIp))
                TempData["WorkSuccess"] = "Đã ghép ứng viên vào vị trí tuyển dụng.";
            else TempData["WorkError"] = "Hồ sơ ứng viên không còn tồn tại.";
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Cannot assign recruitment candidate {CandidateId}", candidateId);
            TempData["WorkError"] = exception.Message;
        }
        return Redirect(Url.Action("Index", new { kind = "recruitment" }) + "#candidate-inbox");
    }

    [HttpGet]
    public IActionResult Notifications()
    {
        ViewBag.Title = "Thông báo";
        try { return View(_store.GetNotifications(_user.Current.Id)); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load notifications");
            ViewBag.NotificationError = "Không thể tải thông báo. Vui lòng thử lại.";
            return View(Array.Empty<WorkNotification>());
        }
    }

    [HttpGet]
    public IActionResult NotificationCount()
    {
        try { return Json(new { Count = _store.GetUnreadNotificationCount(_user.Current.Id) }); }
        catch { return StatusCode(503, new { Message = "Không thể tải số thông báo chưa đọc." }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ReadAllNotifications()
    {
        try { _store.ReadAllNotifications(_user.Current.Id); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot mark notifications read");
            TempData["NotificationError"] = "Không thể đánh dấu đã đọc. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Notifications));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult OpenNotification(long id)
    {
        string link;
        try { link = _store.ReadNotification(id, _user.Current.Id); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot open notification");
            TempData["NotificationError"] = "Không thể mở thông báo. Vui lòng thử lại.";
            return RedirectToAction(nameof(Notifications));
        }
        return Redirect(Url.IsLocalUrl(link) ? link : Url.Action(nameof(Notifications))!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BookingAction(int id, string kind, string command, string note)
    {
        if (kind is not ("vehicle" or "meeting" or "business-trip")) return NotFound();
        var page = Page(kind);
        Load(page);
        var item = page.Items.FirstOrDefault(x => x.Id == id);
        if (item == null) return NotFound();
        var normalized = command?.ToLowerInvariant();
        string newStatus;
        if (normalized == "cancel" && (item.EmployeeId == _user.Current.Id || item.CreatedBy == _user.Current.Id)) newStatus = "CANCELLED";
        else if (normalized == "approve" && page.CanManage) newStatus = "APPROVED";
        else if (normalized == "reject" && page.CanManage && !string.IsNullOrWhiteSpace(note)) newStatus = "REJECTED";
        else return Forbid();
        if (!_store.TransitionBooking(id, kind, "PENDING", newStatus, note, _user.Current.Id, ClientIp))
            TempData["WorkError"] = "Bản ghi đã được xử lý hoặc không còn ở trạng thái chờ.";
        else
            TempData["WorkSuccess"] = $"Đã cập nhật {WorkItem.FormatCode(kind, id)}.";
        return RedirectToAction("Index", new { kind });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult UpdateKpi([Bind("Id,Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,Target,Actual,Weight,Priority,KpiType,Quarter,ProofNote,Status")] WorkItem draft)
    {
        if (draft.Kind != "kpi") return NotFound();
        var page = Page("kpi");
        page.EditId = draft.Id;
        page.Draft = draft;
        Load(page);
        if (!page.CanManage) return Forbid();
        if (!page.Items.Any(x => x.Id == draft.Id)) return NotFound();
        if (draft.EmployeeId.HasValue && !page.People.Any(x => x.Id == draft.EmployeeId))
            ModelState.AddModelError("", "Nhân viên được chọn không hợp lệ.");
        if (draft.DepartmentId.HasValue && !page.Departments.Any(x => x.Id == draft.DepartmentId))
            ModelState.AddModelError("", "Phòng ban được chọn không hợp lệ.");
        ValidateKpi(draft, page, draft.Id);
        if (!page.Available || !ModelState.IsValid) return View("Index", page);
        if (!_store.UpdateKpi(draft, _user.Current.Id, ClientIp))
        {
            ModelState.AddModelError("", "KPI không còn tồn tại hoặc không thể cập nhật.");
            return View("Index", page);
        }
        TempData["WorkSuccess"] = $"Đã cập nhật KPI #{draft.Id:D5}.";
        return RedirectToAction("Index", new { kind = "kpi" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SubmitKpiProof(int id, decimal actual, string proofNote)
    {
        if (!_store.SubmitKpiProof(id, actual, proofNote, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể gửi chứng minh KPI. Vui lòng kiểm tra lại.";
        }
        else
        {
            TempData["WorkSuccess"] = $"Đã gửi chứng minh kết quả KPI #{id:D5}. Trạng thái chuyển sang Chờ chứng minh.";
        }
        return RedirectToAction("Index", new { kind = "kpi" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EvaluateKpiProof(int id, bool approve, string reviewNote)
    {
        var page = Page("kpi");
        if (!page.CanManage) return Forbid();

        if (!_store.EvaluateKpiProof(id, approve, reviewNote, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể đánh giá KPI. Vui lòng thử lại.";
        }
        else
        {
            TempData["WorkSuccess"] = approve
                ? $"Đã phê duyệt chứng minh KPI #{id:D5}. Kết quả chuyển sang Đã chứng minh."
                : $"Đã từ chối chứng minh KPI #{id:D5}. Đã gửi yêu cầu chỉnh sửa cho nhân viên.";
        }
        return RedirectToAction("Index", new { kind = "kpi" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteKpi(int id)
    {
        var page = Page("kpi");
        if (!page.CanManage) return Forbid();

        if (!_store.DeleteKpi(id, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể xóa KPI.";
        }
        else
        {
            TempData["WorkSuccess"] = $"Đã xóa KPI #{id:D5}.";
        }
        return RedirectToAction("Index", new { kind = "kpi" });
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

    private void ValidateKpi(WorkItem draft, WorkPage page, int? excludeId = null)
    {
        if (draft.KpiType == "ASSIGNED")
        {
            if (!draft.EmployeeId.HasValue)
                ModelState.AddModelError("", "KPI được chỉ định cần chọn nhân viên nhận KPI.");
        }
        else if (draft.KpiType != "PLAN")
        {
            if (!draft.EmployeeId.HasValue && !draft.DepartmentId.HasValue)
                ModelState.AddModelError("", "Vui lòng chọn nhân viên hoặc phòng ban nhận KPI.");
        }
        if (draft.EmployeeId.HasValue && draft.DepartmentId.HasValue)
            ModelState.AddModelError("", "Mỗi tiêu chí KPI chỉ giao cho một nhân viên hoặc một phòng ban.");
        if (!draft.DueDate.HasValue)
            draft.DueDate = new DateTime(2026, 12, 31);
        if (string.IsNullOrWhiteSpace(draft.Reference))
            draft.Reference = $"KPI-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}";
        if (string.IsNullOrWhiteSpace(draft.Category))
            draft.Category = "Chỉ số";
        if (!draft.Target.HasValue || draft.Target <= 0 || !draft.Weight.HasValue
            || string.IsNullOrWhiteSpace(draft.Title))
            ModelState.AddModelError("", "KPI cần tên mục tiêu, chỉ tiêu và tỷ trọng (%).");
        if (!draft.Weight.HasValue || (!draft.EmployeeId.HasValue && !draft.DepartmentId.HasValue)) return;
        var assignedWeight = page.Items
            .Where(item => item.Kind == "kpi" && item.Id != excludeId
                && (draft.EmployeeId.HasValue
                    ? item.EmployeeId == draft.EmployeeId
                    : !item.EmployeeId.HasValue && item.DepartmentId == draft.DepartmentId))
            .Sum(item => item.Weight ?? 0);
        if (assignedWeight + draft.Weight > 100)
            ModelState.AddModelError("", $"Tổng tỷ trọng KPI của đối tượng nhận không được vượt 100% (hiện có {assignedWeight:N0}%).");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EnrollTraining(int trainingId)
    {
        var page = Page("training");
        if (page == null) return NotFound();
        if (!_store.EnrollTraining(trainingId, _user.Current.Id, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể đăng ký khóa đào tạo. Vui lòng thử lại.";
        }
        else
        {
            TempData["WorkSuccess"] = "Đăng ký tham gia khóa đào tạo thành công!";
        }
        return RedirectToAction("Index", new { kind = "training", trainingTab = "ENROLLED" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult UnenrollTraining(int trainingId)
    {
        var page = Page("training");
        if (page == null) return NotFound();
        if (!_store.UnenrollTraining(trainingId, _user.Current.Id, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể hủy đăng ký. Vui lòng thử lại.";
        }
        else
        {
            TempData["WorkSuccess"] = "Đã hủy đăng ký khóa đào tạo.";
        }
        return RedirectToAction("Index", new { kind = "training" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EvaluateTrainingStudent(int enrollmentId, int progress, decimal? score, string result, string note)
    {
        var page = Page("training");
        if (page == null || !page.CanManage) return Forbid();
        if (!_store.EvaluateTraining(enrollmentId, progress, score, result, note, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể cập nhật đánh giá học viên.";
        }
        else
        {
            TempData["WorkSuccess"] = "Đã cập nhật đánh giá và cấp chứng chỉ hoàn thành (nếu đạt).";
        }
        return RedirectToAction("Index", new { kind = "training", trainingTab = "CERTIFICATES" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveTrainingCourse([Bind("Id,Kind,Title,Description,Category,Reference,WorkLocation,StartDate,DueDate,Target,Status,ContactName,SkillRequirements,Benefits,Keywords")] WorkItem draft)
    {
        var page = Page("training");
        if (page == null || !page.CanManage) return Forbid();
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            TempData["WorkError"] = "Tên khóa đào tạo không được để trống.";
            return RedirectToAction("Index", new { kind = "training" });
        }
        draft.Kind = "training";
        if (string.IsNullOrWhiteSpace(draft.Reference))
            draft.Reference = $"TRN-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(10, 99)}";
        if (string.IsNullOrWhiteSpace(draft.Category))
            draft.Category = "Online";
        if (string.IsNullOrWhiteSpace(draft.WorkLocation))
            draft.WorkLocation = "Tất cả";

        if (string.IsNullOrWhiteSpace(draft.Keywords) || draft.Keywords.Trim() == "[]")
        {
            var start = draft.StartDate ?? DateTime.Today;
            var due = draft.DueDate ?? start.AddMonths(2);
            if (due < start) due = start.AddMonths(2);
            var sessions = new List<object>();
            var sessionIndex = 1;
            var cur = start;
            var room = draft.Category == "Offline" ? "Hội trường đào tạo Lầu 3" : "Google Meet Online";
            while (cur <= due && sessionIndex <= 24)
            {
                if (cur.DayOfWeek == DayOfWeek.Tuesday || cur.DayOfWeek == DayOfWeek.Friday)
                {
                    sessions.Add(new
                    {
                        date = cur.ToString("yyyy-MM-dd"),
                        time = "08:00 - 10:00",
                        title = $"Buổi {sessionIndex} - {draft.Title}",
                        room = room
                    });
                    sessionIndex++;
                }
                cur = cur.AddDays(1);
            }
            if (sessions.Count == 0)
            {
                sessions.Add(new
                {
                    date = start.ToString("yyyy-MM-dd"),
                    time = "08:00 - 10:00",
                    title = $"Buổi 1 - {draft.Title}",
                    room = room
                });
            }
            draft.Keywords = System.Text.Json.JsonSerializer.Serialize(sessions);
        }

        if (!_store.SaveTrainingCourse(draft, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể lưu thông tin khóa đào tạo.";
        }
        else
        {
            TempData["WorkSuccess"] = draft.Id > 0 ? "Đã cập nhật khóa đào tạo." : "Đã tạo khóa đào tạo mới thành công.";
        }
        return RedirectToAction("Index", new { kind = "training" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteTrainingCourse(int id)
    {
        var page = Page("training");
        if (page == null || !page.CanManage) return Forbid();
        if (!_store.DeleteTrainingCourse(id, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể xóa khóa đào tạo.";
        }
        else
        {
            TempData["WorkSuccess"] = "Đã xóa khóa đào tạo.";
        }
        return RedirectToAction("Index", new { kind = "training" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult TransferRequestAction(int id, string command, string note = null)
    {
        var page = Page("transfer");
        if (page == null || !page.CanManage) return Forbid();

        var status = command switch
        {
            "MANAGER_APPROVE" => "MANAGER_APPROVED",
            "HR_APPROVE" => "HR_REVIEWED",
            "DIRECTOR_APPROVE" or "APPROVE" => "APPROVED",
            "REJECT" => "REJECTED",
            "CANCEL" => "CANCELLED",
            _ => null
        };
        if (status == null) return BadRequest();

        var actionNote = string.IsNullOrWhiteSpace(note)
            ? (status == "APPROVED" ? "Phê duyệt điều chuyển chính thức"
              : status == "MANAGER_APPROVED" ? "Trưởng bộ phận đã phê duyệt"
              : status == "HR_REVIEWED" ? "HR đã thẩm định hồ sơ"
              : status == "REJECTED" ? "Từ chối phiếu luân chuyển" : "Cập nhật trạng thái")
            : note;

        var changed = _store.UpdateTransferRequestStatus(id, status, actionNote, _user.Current.Id, ClientIp);
        if (changed)
            TempData["WorkSuccess"] = status == "APPROVED" ? $"Đã phê duyệt phiếu luân chuyển #{id:D5}."
                : status == "REJECTED" ? $"Đã từ chối phiếu luân chuyển #{id:D5}."
                : $"Đã cập nhật tiến trình phê duyệt phiếu #{id:D5}.";
        else
            TempData["WorkError"] = "Không thể cập nhật trạng thái phiếu luân chuyển.";

        return RedirectToAction("Index", new { kind = "transfer", trTab = "requests" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveTransferDecision([Bind("Id,Kind,Title,Description,Category,Reference,ContactName,StartDate,DueDate,WorkLocation,Target,DepartmentId,EmployeeId,JobLevel,Keywords,Status")] WorkItem draft)
    {
        var page = Page("transfer");
        if (page == null || !page.CanManage) return Forbid();

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            TempData["WorkError"] = "Vui lòng nhập trích yếu quyết định điều chuyển.";
            return RedirectToAction("Index", new { kind = "transfer", trTab = "decisions" });
        }
        draft.Kind = "transfer-decision";
        if (string.IsNullOrWhiteSpace(draft.Reference))
            draft.Reference = $"QĐ-DC-{DateTime.Now:yy}-{Random.Shared.Next(10, 99)}";
        if (string.IsNullOrWhiteSpace(draft.Category))
            draft.Category = "Điều chuyển";
        if (string.IsNullOrWhiteSpace(draft.ContactName))
            draft.ContactName = _user.Current.DisplayName;
        if (!draft.StartDate.HasValue)
            draft.StartDate = DateTime.Today;
        if (!draft.DueDate.HasValue)
            draft.DueDate = draft.StartDate.Value.AddDays(7);
        if (!draft.Target.HasValue || draft.Target <= 0)
            draft.Target = 1;

        if (!_store.SaveTransferDecision(draft, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể lưu quyết định điều chuyển.";
        }
        else
        {
            TempData["WorkSuccess"] = draft.Id > 0 ? "Đã cập nhật quyết định điều chuyển." : $"Đã tạo quyết định điều chuyển {draft.Reference} thành công.";
        }
        return RedirectToAction("Index", new { kind = "transfer", trTab = "decisions" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult TransferDecisionAction(int id, string command, string note = null)
    {
        var page = Page("transfer");
        if (page == null || !page.CanManage) return Forbid();

        if (command == "EXECUTE")
        {
            if (_store.ExecuteTransferDecision(id, _user.Current.Id, ClientIp))
                TempData["WorkSuccess"] = $"Đã ban hành và thực thi quyết định điều chuyển #{id:D5}. Hồ sơ phòng ban nhân sự đã được cập nhật!";
            else
                TempData["WorkError"] = "Không thể thực thi quyết định điều chuyển.";
        }
        else if (command == "APPROVE")
        {
            var item = new WorkItem { Id = id, Status = "APPROVED" };
            if (_store.SaveTransferDecision(item, _user.Current.Id, ClientIp))
                TempData["WorkSuccess"] = $"Đã phê duyệt quyết định điều chuyển #{id:D5}.";
            else
                TempData["WorkError"] = "Không thể phê duyệt quyết định điều chuyển.";
        }
        else if (command == "DELETE")
        {
            if (_store.DeleteTransferItem(id, "transfer-decision", _user.Current.Id, ClientIp))
                TempData["WorkSuccess"] = $"Đã xóa quyết định điều chuyển #{id:D5}.";
            else
                TempData["WorkError"] = "Không thể xóa quyết định điều chuyển.";
        }
        return RedirectToAction("Index", new { kind = "transfer", trTab = "decisions" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteTransferItem(int id, string kind)
    {
        var page = Page("transfer");
        if (page == null || !page.CanManage) return Forbid();
        if (_store.DeleteTransferItem(id, kind ?? "transfer", _user.Current.Id, ClientIp))
            TempData["WorkSuccess"] = $"Đã xóa mục #{id:D5}.";
        else
            TempData["WorkError"] = "Không thể xóa mục điều chuyển.";
        return RedirectToAction("Index", new { kind = "transfer", trTab = kind == "transfer-decision" ? "decisions" : "requests" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveAssetItem(WorkItem draft)
    {
        var page = Page("assets");
        if (page == null || !page.CanManage) return Forbid();

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            TempData["WorkError"] = "Vui lòng nhập tên tài sản.";
            return RedirectToAction("Index", new { kind = "assets", asTab = "list" });
        }

        if (!_store.SaveAssetItem(draft, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể lưu thông tin tài sản.";
        }
        else
        {
            TempData["WorkSuccess"] = draft.Id > 0 ? "Đã cập nhật tài sản thành công." : $"Đã tạo mới tài sản {draft.Reference} thành công.";
        }
        return RedirectToAction("Index", new { kind = "assets", asTab = "list" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AssetHandoverAction(int assetId, string command, int? employeeId = null, int? departmentId = null, DateTime? handoverDate = null, string condition = null, string note = null)
    {
        var page = Page("assets");
        if (page == null) return Forbid();

        var isManagerOrAdmin = page.CanManage;
        if (!isManagerOrAdmin && command != "UPDATE_CONDITION")
        {
            return Forbid();
        }

        if (!_store.AssetHandoverAction(assetId, command, employeeId, departmentId, handoverDate, condition, note, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể thực hiện thao tác trên tài sản.";
        }
        else
        {
            var msg = command switch
            {
                "ALLOCATE" => "Đã bàn giao và cấp phát tài sản thành công cho nhân sự.",
                "RECOVER" => "Đã thu hồi tài sản về kho lưu trữ thành công.",
                "LIQUIDATE" => "Đã chuyển trạng thái tài sản sang thanh lý.",
                "UPDATE_CONDITION" => "Đã cập nhật hiện trạng tài sản thành công.",
                _ => "Thao tác thành công."
            };
            TempData["WorkSuccess"] = msg;
        }

        var redirectTab = command == "ALLOCATE" || command == "RECOVER" ? "handover" : "list";
        return RedirectToAction("Index", new { kind = "assets", asTab = redirectTab });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteAssetItem(int id)
    {
        var page = Page("assets");
        if (page == null || !page.CanManage) return Forbid();

        if (_store.DeleteAssetItem(id, _user.Current.Id, ClientIp))
            TempData["WorkSuccess"] = $"Đã xóa tài sản #{id:D4}.";
        else
            TempData["WorkError"] = "Không thể xóa tài sản. Chỉ có thể xóa tài sản chưa cấp phát.";

        return RedirectToAction("Index", new { kind = "assets", asTab = "list" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BatchPayrollAction(string period, string actionType, string note = null)
    {
        var page = Page("payroll");
        if (page == null) return Forbid();

        period = string.IsNullOrWhiteSpace(period) ? "2026-02" : period;
        var isDirector = User.IsInRole(HrmRoles.Director) || User.IsInRole(HrmRoles.Admin);
        var isHrOrAdmin = User.IsInRole(HrmRoles.Hr) || User.IsInRole(HrmRoles.Admin);

        var cmd = (actionType ?? "").ToUpperInvariant();
        if ((cmd == "APPROVE" || cmd == "REJECT") && !isDirector) return Forbid();
        if ((cmd == "FINALIZE" || cmd == "PAY") && !isHrOrAdmin) return Forbid();
        if (cmd == "PUBLISH" && !isDirector && !isHrOrAdmin) return Forbid();

        if (!_store.BatchPayrollAction(period, cmd, note, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể thực hiện thao tác duyệt bảng lương. Vui lòng kiểm tra lại trạng thái kỳ lương.";
        }
        else
        {
            var msg = cmd switch
            {
                "FINALIZE" => $"Đã duyệt chốt bảng lương kỳ {period} và trình Ban Giám đốc phê duyệt.",
                "APPROVE" => $"Ban Giám đốc đã phê duyệt bảng lương kỳ {period} thành công!",
                "REJECT" => $"Đã yêu cầu điều chỉnh lại bảng lương kỳ {period}. Lý do: {note}",
                "PUBLISH" => $"Đã phát hành phiếu lương điện tử kỳ {period} đến toàn bộ nhân viên!",
                "PAY" => $"Đã hoàn tất xác nhận thanh toán/chi trả lương kỳ {period}.",
                _ => "Thao tác thành công."
            };
            TempData["WorkSuccess"] = msg;
        }

        return RedirectToAction("Index", new { kind = "payroll", pyPeriod = period, pyTab = "dashboard" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SavePayrollComponent([Bind("Kind,Title,Description,Reference,WorkLocation,Category,JobLevel,SalaryRange,Quarter,EmployeeId,DepartmentId,StartDate,Target,Status,Priority")] WorkItem draft)
    {
        var page = Page("payroll");
        if (page == null || (!User.IsInRole(HrmRoles.Admin) && !User.IsInRole(HrmRoles.Hr))) return Forbid();

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            TempData["WorkError"] = "Vui lòng nhập tên khoản phụ cấp/khấu trừ/tạm ứng.";
            return RedirectToAction("Index", new { kind = "payroll", pyPeriod = draft.Quarter ?? "2026-02" });
        }

        if (!_store.SavePayrollComponent(draft, _user.Current.Id, ClientIp))
        {
            TempData["WorkError"] = "Không thể lưu thông tin thành phần bảng lương.";
        }
        else
        {
            TempData["WorkSuccess"] = $"Đã ghi nhận thành công thành phần {draft.Title}.";
        }

        var redirectTab = draft.Kind switch
        {
            "payroll-allowance" => "allowance",
            "payroll-deduction" => "deduction",
            "payroll-advance" => "advance",
            _ => "dashboard"
        };
        return RedirectToAction("Index", new { kind = "payroll", pyPeriod = draft.Quarter ?? "2026-02", pyTab = redirectTab });
    }

    [HttpGet]
    public IActionResult ExportPayrollExcel(string pyPeriod = "2026-02", string pyDept = "ALL", string pyStatus = "ALL")
    {
        var page = Page("payroll");
        if (page == null) return Forbid();

        page.PayrollPeriod = string.IsNullOrWhiteSpace(pyPeriod) ? "2026-02" : pyPeriod;
        page.PayrollDeptFilter = string.IsNullOrWhiteSpace(pyDept) ? "ALL" : pyDept;
        page.PayrollStatusFilter = string.IsNullOrWhiteSpace(pyStatus) ? "ALL" : pyStatus;
        Load(page);

        var csv = new System.Text.StringBuilder();
        csv.Append('\uFEFF');
        csv.AppendLine("STT,Mã nhân viên,Họ và tên,Email,Phòng ban,Chi nhánh,Lương cơ bản,Lương KPI,Lương doanh số,Lương OT,Tổng phụ cấp,Thưởng,Tổng thu nhập,BHXH (8%),BHYT (1.5%),BHTN (1%),Tổng bảo hiểm (10.5%),Thuế TNCN,Tạm ứng,Khấu trừ,Thực nhận (Net),Trạng thái");

        int idx = 1;
        foreach (var item in page.PayrollItems)
        {
            csv.AppendLine($"{idx}," +
                $"\"{item.EmployeeCode}\"," +
                $"\"{item.EmployeeName}\"," +
                $"\"{item.EmployeeEmail}\"," +
                $"\"{item.DepartmentName}\"," +
                $"\"{item.Branch}\"," +
                $"{item.BaseSalary}," +
                $"{item.KpiSalary}," +
                $"{item.SalesSalary}," +
                $"{item.OtSalary}," +
                $"{item.TotalAllowance}," +
                $"{item.Bonus}," +
                $"{item.GrossIncome}," +
                $"{item.SocialInsurance}," +
                $"{item.HealthInsurance}," +
                $"{item.UnemploymentInsurance}," +
                $"{item.TotalInsurance}," +
                $"{item.PersonalIncomeTax}," +
                $"{item.Advance}," +
                $"{item.Deduction}," +
                $"{item.NetSalary}," +
                $"\"{item.StatusText}\"");
            idx++;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv; charset=utf-8", $"Bang_Luong_Nhi_Gia_{page.PayrollPeriod}.csv");
    }
}

