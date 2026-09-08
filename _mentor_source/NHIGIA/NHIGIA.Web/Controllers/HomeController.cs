using System.Web.Mvc;

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