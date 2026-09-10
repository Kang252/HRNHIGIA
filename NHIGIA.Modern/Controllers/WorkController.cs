using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[Authorize]
public sealed class WorkController : Controller
{
    private static readonly string[] MeetingRooms = ["Phòng họp 1 · 8 người", "Phòng họp 2 · 16 người", "Phòng đào tạo · 30 người"];
    private static readonly string[] VehicleTypes = ["Xe 4 chỗ", "Xe 7 chỗ", "Xe 16 chỗ", "Xe tải", "Khác"];
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
            CanManage = kind is "kpi" or "training" or "overtime" or "resignation" or "assets" or "vehicle" or "meeting" or "business-trip" or "offboarding" ? managePeople : configure,
            CanCreate = kind switch
            {
                "helpdesk" or "overtime" or "resignation" or "vehicle" or "meeting" => true,
                "training" or "business-trip" => managePeople,
                "offboarding" => configure,
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
            "resignation" => ("Quản lý nghỉ việc và thôi việc", "Tiếp nhận đề nghị nghỉ việc, phê duyệt và theo dõi thủ tục bàn giao trên cùng một màn hình."),
            "transfer" => ("Điều chuyển nhân sự", "Theo dõi đề nghị điều chuyển. Hồ sơ nhân viên chỉ thay đổi sau quy trình phê duyệt riêng."),
            "assets" => ("Quản lý tài sản", "Danh mục tài sản, người sử dụng và tình trạng bàn giao."),
            "helpdesk" => ("Helpdesk IT", "Gửi yêu cầu hỗ trợ và theo dõi các yêu cầu của bạn."),
            "vehicle" => ("Đặt xe", "Đăng ký xe phục vụ công việc và theo dõi trạng thái điều phối."),
            "meeting" => ("Đặt phòng họp", "Đặt phòng theo khung giờ; mỗi lịch sử dụng cùng phòng phải cách nhau ít nhất 10 phút."),
            "business-trip" => ("Phân công công tác", "Phân công nhân viên, địa điểm và thời gian thực hiện công tác."),
            "offboarding" => ("Thủ tục thôi việc", "Theo dõi bàn giao công việc, tài sản và hồ sơ khi nhân viên thôi việc."),
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
        if (kind == "offboarding") return RedirectToAction(nameof(Index), new { kind = "resignation", q });
        var page = Page(kind, q);
        if (page == null) return NotFound();
        if (kind is "transfer" or "recruitment" && !page.CanManage) return Forbid();
        page.EditId = editId;
        Load(page);
        if (kind is "assets" or "offboarding" && page.CanCreate && page.Available)
            page.Draft.Reference = kind == "assets" ? _store.GetNextAssetReference() : _store.GetNextOffboardingReference();
        if (kind == "resignation" && page.Available && (User.IsInRole(HrmRoles.Admin) || User.IsInRole(HrmRoles.Hr) || User.IsInRole(HrmRoles.Director)))
            page.NextOffboardingReference = _store.GetNextOffboardingReference();
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
        return View(kind == "assets" ? "Assets" : "Index", page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,StartAt,EndAt,Location,Destination,Target,Actual,Weight,Priority,ParticipantIds")] WorkItem draft)
    {
        var page = Page(draft.Kind);
        if (page == null) return NotFound();
        if (!page.CanCreate) return Forbid();
        page.Draft = draft;
        Load(page);
        if (draft.Kind is "assets" or "offboarding")
            draft.Reference = draft.Kind == "assets" ? _store.GetNextAssetReference() : _store.GetNextOffboardingReference();
        if (draft.EmployeeId.HasValue && !page.People.Any(x => x.Id == draft.EmployeeId))
            ModelState.AddModelError("", "Nhân viên được chọn không hợp lệ.");
        if (draft.DepartmentId.HasValue && !page.Departments.Any(x => x.Id == draft.DepartmentId))
            ModelState.AddModelError("", "Phòng ban được chọn không hợp lệ.");
        if (draft.Kind is "transfer" or "payroll" or "overtime" or "resignation" && !draft.EmployeeId.HasValue && page.CanManage)
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
        if (draft.Kind == "assets" && (string.IsNullOrWhiteSpace(draft.Reference) || string.IsNullOrWhiteSpace(draft.Category)
            || !draft.DueDate.HasValue || !draft.Target.HasValue || draft.Target <= 0 || draft.Target != decimal.Truncate(draft.Target.Value)
            || !draft.Actual.HasValue || draft.Actual < 0))
            ModelState.AddModelError("", "Tài sản cần mã, loại, ngày mua, số lượng nguyên và nguyên giá hợp lệ.");
        if (draft.Kind == "helpdesk" && (string.IsNullOrWhiteSpace(draft.Description) || !new[] { "LOW", "NORMAL", "HIGH", "URGENT" }.Contains(draft.Priority)))
            ModelState.AddModelError("", "Vui lòng nhập mô tả và mức ưu tiên hợp lệ.");
        if (draft.Kind is "vehicle" or "meeting" or "business-trip")
        {
            if (!draft.StartAt.HasValue || !draft.EndAt.HasValue || draft.EndAt <= draft.StartAt)
                ModelState.AddModelError("", "Vui lòng chọn thời gian bắt đầu và kết thúc hợp lệ.");
            if (draft.StartAt < DateTime.Now.AddMinutes(-5))
                ModelState.AddModelError("", "Thời gian bắt đầu không được ở trong quá khứ.");
        }
        if (draft.Kind == "vehicle" && (string.IsNullOrWhiteSpace(draft.Location) || string.IsNullOrWhiteSpace(draft.Destination) || string.IsNullOrWhiteSpace(draft.Category) || !draft.Target.HasValue || draft.Target <= 0))
            ModelState.AddModelError("", "Đặt xe cần loại xe, số người, điểm đón và điểm đến.");
        if (draft.Kind == "vehicle" && !VehicleTypes.Contains(draft.Category))
            ModelState.AddModelError("", "Loại xe được chọn không hợp lệ.");
        if (draft.Kind == "vehicle" && draft.Target > 300)
            ModelState.AddModelError("", "Mỗi yêu cầu đặt xe hỗ trợ tối đa 300 nhân viên.");
        if (draft.Kind == "meeting" && (string.IsNullOrWhiteSpace(draft.Location) || !draft.Target.HasValue || draft.Target <= 0))
            ModelState.AddModelError("", "Đặt phòng họp cần phòng, số người và khung giờ sử dụng.");
        var usesParticipantList = draft.Kind is "meeting" or "vehicle";
        var participantIds = usesParticipantList ? (draft.ParticipantIds ?? new List<int>()).Distinct().ToList() : new List<int>();
        if (usesParticipantList && participantIds.Count == 0)
            ModelState.AddModelError("", draft.Kind == "vehicle" ? "Vui lòng chọn ít nhất một nhân viên đi xe." : "Vui lòng chọn ít nhất một nhân viên tham dự.");
        if (usesParticipantList && participantIds.Any(id => !page.People.Any(person => person.Id == id)))
            ModelState.AddModelError("", "Danh sách nhân viên được chọn không hợp lệ.");
        if (usesParticipantList && draft.Target.HasValue && (draft.Target != decimal.Truncate(draft.Target.Value) || participantIds.Count != (int)draft.Target.Value))
            ModelState.AddModelError("", $"Vui lòng chọn đủ {(int)draft.Target.Value} nhân viên tương ứng với số người.");
        if (draft.Kind == "meeting" && !MeetingRooms.Contains(draft.Location))
            ModelState.AddModelError("", "Phòng họp được chọn không hợp lệ.");
        var roomCapacity = draft.Location switch { "Phòng họp 1 · 8 người" => 8, "Phòng họp 2 · 16 người" => 16, "Phòng đào tạo · 30 người" => 30, _ => 0 };
        if (draft.Kind == "meeting" && draft.Target > roomCapacity && roomCapacity > 0)
            ModelState.AddModelError("", $"Số người tham dự vượt sức chứa {roomCapacity} người của phòng.");
        if (draft.Kind == "business-trip" && (!draft.EmployeeId.HasValue || string.IsNullOrWhiteSpace(draft.Destination)))
            ModelState.AddModelError("", "Phân công công tác cần nhân viên, nơi công tác và thời gian.");
        if (draft.Kind == "offboarding" && (!draft.EmployeeId.HasValue || !draft.DueDate.HasValue || string.IsNullOrWhiteSpace(draft.Description)))
            ModelState.AddModelError("", "Thủ tục thôi việc cần nhân viên, ngày nghỉ và nội dung bàn giao.");
        if (draft.Kind == "meeting" && draft.StartAt.HasValue && draft.EndAt.HasValue && !string.IsNullOrWhiteSpace(draft.Location)
            && page.Available && _store.HasMeetingConflict(draft.Location, draft.StartAt.Value, draft.EndAt.Value))
            ModelState.AddModelError("", "Phòng họp đã có lịch hoặc chưa đủ khoảng nghỉ 10 phút. Vui lòng chọn thời gian hoặc phòng khác.");
        if (!page.Available || !ModelState.IsValid)
            return draft.Kind == "offboarding" ? View("Index", CombinedResignationPage(draft)) : View(draft.Kind == "assets" ? "Assets" : "Index", page);
        draft.CreatedBy = _user.Current.Id;
        draft.Status = draft.Kind switch
        {
            "assets" => draft.EmployeeId.HasValue ? "ASSIGNED" : "AVAILABLE",
            "meeting" when page.CanManage => "APPROVED",
            "transfer" or "overtime" or "resignation" or "vehicle" or "meeting" or "business-trip" or "offboarding" => "PENDING",
            "kpi" => "TRACKING",
            "payroll" => "DRAFT",
            "training" => "PLANNED",
            _ => "OPEN"
        };
        if (draft.Kind == "assets" && draft.EmployeeId.HasValue) draft.AssetInUse = 1;
        if (draft.Kind == "helpdesk" || (!page.CanManage && draft.Kind is "overtime" or "resignation"))
            draft.EmployeeId = _user.Current.Id;
        if (!draft.EmployeeId.HasValue && draft.Kind is "vehicle" or "meeting")
            draft.EmployeeId = _user.Current.Id;
        if (draft.Kind is "vehicle" or "meeting" or "business-trip")
            draft.DueDate = draft.StartAt?.Date;
        try
        {
            var id = draft.Kind == "assets" ? _store.CreateAsset(draft) : _store.Create(draft, participantIds);
            TempData["WorkSuccess"] = $"Đã lưu {WorkItem.FormatCode(draft.Kind, id)} vào hệ thống.";
            return RedirectToAction("Index", new { kind = draft.Kind == "offboarding" ? "resignation" : draft.Kind });
        }
        catch (SqlException ex) when (draft.Kind == "assets" && ex.Number is 2601 or 2627)
        {
            _logger.LogWarning(ex, "Duplicate asset reference {Reference}", draft.Reference);
            draft.Reference = _store.GetNextAssetReference();
            ModelState.AddModelError("Reference", "Hệ thống vừa cấp mã này cho tài sản khác. Mã mới đã được tạo, vui lòng lưu lại.");
            return View("Assets", page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot save work item");
            ModelState.AddModelError("", "Không thể lưu dữ liệu. Vui lòng thử lại.");
            return draft.Kind == "offboarding" ? View("Index", CombinedResignationPage(draft)) : View(draft.Kind == "assets" ? "Assets" : "Index", page);
        }
    }

    private WorkPage CombinedResignationPage(WorkItem offboardingDraft)
    {
        var combined = Page("resignation");
        Load(combined);
        combined.OffboardingDraft = offboardingDraft;
        combined.NextOffboardingReference = string.IsNullOrWhiteSpace(offboardingDraft.Reference) && combined.Available
            ? _store.GetNextOffboardingReference()
            : offboardingDraft.Reference;
        return combined;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AssetAction(List<int> ids, string command, int? employeeId, string note)
    {
        var page = Page("assets");
        if (page == null || !page.CanManage) return Forbid();
        Load(page);
        var selected = (ids ?? []).Distinct().Where(id => page.Items.Any(item => item.Id == id)).ToList();
        if (selected.Count == 0)
        {
            TempData["WorkError"] = "Vui lòng chọn ít nhất một tài sản.";
            return RedirectToAction("Index", new { kind = "assets" });
        }
        if (command == "allocate" && (!employeeId.HasValue || !page.People.Any(person => person.Id == employeeId)))
        {
            TempData["WorkError"] = "Vui lòng chọn nhân viên nhận tài sản hợp lệ.";
            return RedirectToAction("Index", new { kind = "assets" });
        }
        if (command is not ("allocate" or "recover" or "maintenance" or "damaged" or "lost" or "dispose")) return Forbid();
        var changed = _store.UpdateAssets(selected, command, employeeId, note, _user.Current.Id, ClientIp);
        TempData[changed > 0 ? "WorkSuccess" : "WorkError"] = changed > 0
            ? $"Đã cập nhật {changed} tài sản."
            : "Không có tài sản nào phù hợp với thao tác đã chọn.";
        return RedirectToAction("Index", new { kind = "assets" });
    }

    [HttpGet]
    public IActionResult Notifications()
    {
        ViewBag.Title = "Thông báo";
        try { return View(_store.GetNotifications(_user.Current.Id)); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load notifications");
            return View(Array.Empty<WorkNotification>());
        }
    }

    [HttpGet]
    public IActionResult NotificationCount()
    {
        try { return Json(new { Count = _store.GetUnreadNotificationCount(_user.Current.Id) }); }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Cannot load notification count");
            return Json(new { Count = 0 });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult OpenNotification(long id)
    {
        var link = _store.ReadNotification(id, _user.Current.Id);
        return Redirect(Url.IsLocalUrl(link) ? link : Url.Action(nameof(Notifications))!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BookingAction(int id, string kind, string command, string note)
    {
        if (kind is not ("vehicle" or "meeting" or "business-trip" or "offboarding")) return NotFound();
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
    public IActionResult UpdateKpi([Bind("Id,Kind,Title,Description,Category,Reference,EmployeeId,DepartmentId,DueDate,Target,Actual,Weight")] WorkItem draft)
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
        TempData["WorkSuccess"] = $"Đã cập nhật {WorkItem.FormatCode("kpi", draft.Id)}.";
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
        TempData["WorkSuccess"] = $"Đã cập nhật {WorkItem.FormatCode("payroll", draft.Id)}.";
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
            TempData["WorkSuccess"] = $"Đã cập nhật trạng thái {WorkItem.FormatCode("payroll", id)}.";
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
        if (!draft.EmployeeId.HasValue && !draft.DepartmentId.HasValue)
            ModelState.AddModelError("", "Vui lòng chọn nhân viên hoặc phòng ban nhận KPI.");
        if (draft.EmployeeId.HasValue && draft.DepartmentId.HasValue)
            ModelState.AddModelError("", "Mỗi tiêu chí KPI chỉ giao cho một nhân viên hoặc một phòng ban.");
        if (!draft.Target.HasValue || draft.Target <= 0 || !draft.Weight.HasValue || !draft.DueDate.HasValue
            || string.IsNullOrWhiteSpace(draft.Reference) || string.IsNullOrWhiteSpace(draft.Category) || string.IsNullOrWhiteSpace(draft.Description))
            ModelState.AddModelError("", "KPI cần mã KPI, đơn vị đo, chỉ tiêu, tỷ trọng, hạn hoàn thành và cách đo kết quả.");
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
}
