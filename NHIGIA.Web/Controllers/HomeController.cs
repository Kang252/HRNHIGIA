using System.Web.Mvc;

using NHIGIA.Web.Infrastructure;
using NHIGIA.Web.Models;
using System.Configuration;

namespace NHIGIA.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Trang chủ";
            return View();
        }

        public ActionResult ListCategory()
        {
            ViewBag.Title = "Danh mục";
            return View();
        }

        public ActionResult BulkUpload()
        {
            ViewBag.Title = "Import dữ liệu";
            return View();
        }

        public ActionResult UploadImage()
        {
            ViewBag.Title = "Upload hình ảnh";
            return View();
        }

        public ActionResult EmployeeInformation()
        {
            ViewBag.Title = "Hồ sơ";
            return View();
        }

        public ActionResult MyProfile()
        {
            ViewBag.Title = "Hồ sơ của tôi";
            var current = CurrentHrmUser;
            if (current == null) return RedirectToAction("Login", "Account");
            var profile = HrmDataStore.Instance.GetEmployeeProfile(current.Id);
            if (profile == null) return HttpNotFound();
            return View(profile);
        }

        public ActionResult Attendance()
        {
            ViewBag.Title = "Chấm công";
            return View();
        }

        public ActionResult WorkSchedules()
        {
            ViewBag.Title = "Lịch làm việc";
            return View();
        }

        public ActionResult LeaveRequests()
        {
            ViewBag.Title = "Yêu cầu nghỉ phép";
            return View();
        }

        public ActionResult InternalCommunications()
        {
            ViewBag.Title = "Truyền thông nội bộ";
            return View();
        }

        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public ActionResult Approvals()
        {
            ViewBag.Title = "Phê duyệt nghỉ phép";
            return View();
        }

        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public ActionResult HanetIntegration()
        {
            if (!string.Equals(ConfigurationManager.AppSettings["HanetIntegrationUiEnabled"], "true", System.StringComparison.OrdinalIgnoreCase)) return HttpNotFound();
            ViewBag.Title = "Tích hợp HANET";
            return View();
        }

        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public ActionResult Reports()
        {
            ViewBag.Title = "Báo cáo nhân sự";
            return View();
        }

        public ActionResult Evaluate()
        {
            ViewBag.Title = "Đánh giá";
            return View();
        }

        public ActionResult EmployeesOnBusinessTrip()
        {
            ViewBag.Title = "Nhân viên đi công tác";
            return View();
        }

        public ActionResult ResignationProcedures()
        {
            ViewBag.Title = "Thôi việc";
            return View();
        }

        public ActionResult Bonus()
        {
            ViewBag.Title = "Khen thưởng";
            return View();
        }

        public ActionResult Problem()
        {
            ViewBag.Title = "Sự cố";
            return View();
        }

    }
}
