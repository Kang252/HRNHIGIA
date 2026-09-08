using NHIGIA.Web.Infrastructure;
using NHIGIA.Web.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class HrmController : BaseController
    {
        private readonly HrmDataStore _store = HrmDataStore.Instance;

        private string ClientIp { get { return Request.UserHostAddress; } }

        private JsonResult Result(Func<object> action)
        {
            try { return Json(ApiResponse.Ok(action()), JsonRequestBehavior.AllowGet); }
            catch (Exception exception)
            {
                _logger.Error(exception);
                Response.StatusCode = 400;
                return Json(ApiResponse.Fail(exception.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Dashboard()
        {
            return Result(() => _store.GetDashboard(CurrentHrmUser));
        }

        [HttpGet]
        public JsonResult Users()
        {
            return Result(() => _store.GetVisibleUsers(CurrentHrmUser).Select(x => new
            {
                x.Id, x.Username, x.DisplayName, x.RoleCode, x.RoleLabel, x.DepartmentId, x.DepartmentName
            }));
        }

        [HttpGet]
        public JsonResult ShiftTemplates()
        {
            return Result(() => _store.GetShiftTemplates());
        }

        [HttpGet]
        public JsonResult Schedules()
        {
            return Result(() => _store.GetSchedules(CurrentHrmUser));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public JsonResult SaveSchedule(SaveScheduleRequest request)
        {
            return Result(() =>
            {
                if (request == null || request.UserId <= 0) throw new InvalidOperationException("Vui lòng chọn nhân viên.");
                if (!_store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == request.UserId)) throw new InvalidOperationException("Bạn không có quyền phân lịch cho nhân viên này.");
                TimeSpan start;
                TimeSpan end;
                if (!TimeSpan.TryParse(request.StartTime, out start) || !TimeSpan.TryParse(request.EndTime, out end)) throw new InvalidOperationException("Giờ bắt đầu hoặc kết thúc không hợp lệ.");
                if (request.EffectiveFrom == default(DateTime)) throw new InvalidOperationException("Vui lòng chọn ngày áp dụng.");
                if (request.EffectiveTo.HasValue && request.EffectiveTo.Value.Date < request.EffectiveFrom.Date) throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");
                request.ShiftName = string.IsNullOrWhiteSpace(request.ShiftName) ? "Ca cá nhân" : request.ShiftName.Trim();
                return new { Id = _store.SaveSchedule(request, CurrentHrmUser, ClientIp) };
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public JsonResult DeleteSchedule(int id)
        {
            return Result(() =>
            {
                if (!_store.DeleteSchedule(id, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Không tìm thấy lịch hoặc bạn không có quyền xóa.");
                return new { Id = id };
            });
        }

        [HttpGet]
        public JsonResult LeaveRequests()
        {
            return Result(() => _store.GetLeaveRequests(CurrentHrmUser));
        }

        [HttpGet]
        public JsonResult LeaveStats()
        {
            return Result(() => _store.GetLeaveStats(CurrentHrmUser));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateLeave(CreateLeaveRequest request)
        {
            return Result(() =>
            {
                if (request == null || string.IsNullOrWhiteSpace(request.LeaveType)) throw new InvalidOperationException("Vui lòng chọn loại nghỉ.");
                if (request.StartDate == default(DateTime) || request.EndDate == default(DateTime) || request.EndDate.Date < request.StartDate.Date) throw new InvalidOperationException("Khoảng ngày nghỉ không hợp lệ.");
                if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("Vui lòng nhập lý do nghỉ.");
                return _store.CreateLeave(request, CurrentHrmUser, ClientIp);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CancelLeave(int id)
        {
            return Result(() =>
            {
                if (!_store.CancelLeave(id, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Chỉ có thể hủy đơn của bạn khi đang chờ duyệt.");
                return new { Id = id };
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public JsonResult ApproveLeave(ApprovalRequest request)
        {
            return Result(() =>
            {
                if (request == null || request.Id <= 0) throw new InvalidOperationException("Đơn nghỉ không hợp lệ.");
                if (!_store.ApproveLeave(request, CurrentHrmUser, ClientIp)) throw new InvalidOperationException("Đơn đã được xử lý hoặc không thuộc phạm vi của bạn.");
                return new { request.Id, request.Approve };
            });
        }

        [HttpGet]
        public JsonResult Communications(string keyword = "", string category = "")
        {
            return Result(() => _store.GetCommunications(CurrentHrmUser, keyword, category));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director, HrmRoles.Manager)]
        public JsonResult CreateCommunication(CreateCommunicationRequest request)
        {
            return Result(() =>
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body)) throw new InvalidOperationException("Vui lòng nhập tiêu đề và nội dung.");
                request.ScopeCode = (request.ScopeCode ?? "DEPARTMENT").ToUpperInvariant();
                if (!new[] { "ALL", "DEPARTMENT", "MANAGER" }.Contains(request.ScopeCode)) throw new InvalidOperationException("Phạm vi đăng tin không hợp lệ.");
                if (request.ScopeCode == "ALL" && !HrmRoles.CanPublishCompanyWide(CurrentHrmUser.RoleCode)) throw new InvalidOperationException("Trưởng phòng chỉ được đăng trong phòng ban hoặc nhóm quản lý.");
                request.Category = string.IsNullOrWhiteSpace(request.Category) ? "Thông báo" : request.Category.Trim();
                return _store.CreateCommunication(request, CurrentHrmUser, ClientIp);
            });
        }

        [HttpGet]
        public JsonResult Attendance(DateTime? fromDate, DateTime? toDate)
        {
            return Result(() =>
            {
                var to = (toDate ?? DateTime.Today).Date;
                var from = (fromDate ?? to.AddDays(-30)).Date;
                if (to < from || (to - from).TotalDays > 366) throw new InvalidOperationException("Khoảng lọc tối đa là 366 ngày.");
                return _store.GetAttendance(CurrentHrmUser, from, to);
            });
        }

        [HttpGet]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public JsonResult HanetSettings()
        {
            return Result(() => _store.GetHanetSettings(false));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public JsonResult SaveHanetSettings(HanetSettingsModel settings)
        {
            return Result(() =>
            {
                Uri apiUri;
                Uri tokenUri;
                if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out apiUri) || apiUri.Scheme != Uri.UriSchemeHttps || !apiUri.Host.EndsWith("hanet.ai", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("API URL phải là địa chỉ HTTPS thuộc hanet.ai.");
                if (!Uri.TryCreate(settings.OAuthTokenUrl, UriKind.Absolute, out tokenUri) || tokenUri.Scheme != Uri.UriSchemeHttps || !tokenUri.Host.EndsWith("hanet.com", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("OAuth URL phải là địa chỉ HTTPS thuộc hanet.com.");
                _store.SaveHanetSettings(settings, CurrentHrmUser, ClientIp);
                return _store.GetHanetSettings(false);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public JsonResult SaveHanetPersonMap(HanetPersonMapRequest request)
        {
            return Result(() =>
            {
                if (request == null || request.UserId <= 0 || (string.IsNullOrWhiteSpace(request.PersonId) && string.IsNullOrWhiteSpace(request.AliasId))) throw new InvalidOperationException("Cần chọn nhân viên và nhập Person ID hoặc Alias ID.");
                if (!_store.GetVisibleUsers(CurrentHrmUser).Any(x => x.Id == request.UserId)) throw new InvalidOperationException("Nhân viên không hợp lệ.");
                _store.SaveHanetPersonMap(request, CurrentHrmUser, ClientIp);
                return new { request.UserId };
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [HrmAuthorize(HrmRoles.Admin, HrmRoles.Hr, HrmRoles.Director)]
        public async Task<JsonResult> TestHanet()
        {
            try
            {
                var settings = _store.GetHanetSettings(true);
                if (string.IsNullOrWhiteSpace(settings.AccessToken)) throw new InvalidOperationException("Chưa có access token HANET.");
                var endpoint = settings.ApiBaseUrl.TrimEnd('/') + "/place/getPlaces";
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) })
                using (var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string> { { "token", settings.AccessToken } })))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(body);
                    var code = Convert.ToString(json["returnCode"] ?? json["code"]);
                    var ok = response.IsSuccessStatusCode && (code == "1" || code == "200" || string.IsNullOrEmpty(code));
                    var message = ok ? "Kết nối HANET thành công." : "HANET từ chối yêu cầu: " + (Convert.ToString(json["returnMessage"] ?? json["message"]) ?? response.ReasonPhrase);
                    _store.UpdateHanetSyncStatus(ok ? "SUCCESS" : "FAILED", message);
                    if (!ok) { Response.StatusCode = 400; return Json(ApiResponse.Fail(message)); }
                    return Json(ApiResponse.Ok(json["data"], message));
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                _store.UpdateHanetSyncStatus("FAILED", exception.Message);
                Response.StatusCode = 400;
                return Json(ApiResponse.Fail(exception.Message));
            }
        }

        [HttpGet]
        public FileContentResult AttendanceCsv(DateTime? fromDate, DateTime? toDate)
        {
            var to = (toDate ?? DateTime.Today).Date;
            var from = (fromDate ?? to.AddDays(-30)).Date;
            var rows = _store.GetAttendance(CurrentHrmUser, from, to);
            var csv = new StringBuilder("Ngay,Nhan vien,Phong ban,Ca,Check-in,Check-out,Phut cong,Di muon,Ve som,Trang thai\r\n");
            foreach (var row in rows)
            {
                Func<string, string> q = value => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
                csv.AppendLine(string.Join(",", q(row.WorkDate.ToString("dd/MM/yyyy")), q(row.DisplayName), q(row.DepartmentName), q(row.ShiftName), q(row.CheckIn.HasValue ? row.CheckIn.Value.ToString("HH:mm") : ""), q(row.CheckOut.HasValue ? row.CheckOut.Value.ToString("HH:mm") : ""), row.WorkedMinutes, row.LateMinutes, row.EarlyMinutes, q(row.StatusCode)));
            }
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", "cham-cong.csv");
        }
    }
}
