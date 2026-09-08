using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class EmployeesOnBusinessTripAssignedStaffController : BaseController
    {
        public JsonResult GetAllEmployeesOnBusinessTripAssignedStaff(int employeesOnBusinessTripId)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffController - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                var response = _employeesOnBusinessTripAssignedStaffService.GetAllEmployeesOnBusinessTripAssignedStaff(employeesOnBusinessTripId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("EmployeesOnBusinessTripAssignedStaffController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End EmployeesOnBusinessTripAssignedStaffController - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeesOnBusinessTripAssignedStaffViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEmployeesOnBusinessTripAssignedStaff(EmployeesOnBusinessTripAssignedStaffViewModel param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffController - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                //var osPrincipal = (UserIdentity)System.Web.HttpContext.Current.User;
                //var userData = JsonConvert.DeserializeObject<TaiKhoanNguoiDungViewModel>(osPrincipal.UserData);
                //obj.NguoiTao = userData.TenDangNhap;
                if (param == null)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Param },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var data = JsonConvert.SerializeObject(param);
                var result = _employeesOnBusinessTripAssignedStaffService.SaveEmployeesOnBusinessTripAssignedStaff(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EmployeesOnBusinessTripAssignedStaffController - Param: " + JsonConvert.SerializeObject(param));

                if (result == JsonConvert.SerializeObject(Constants.MessageInformation.Success))
                {
                    _logger.Trace("End EmployeesOnBusinessTripAssignedStaffController - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EmployeesOnBusinessTripAssignedStaffController - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Save_Fail },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new JsonResult()
                {
                    Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Internal_Server_Error },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public ActionResult DeleteEmployeesOnBusinessTripAssignedStaff(EmployeesOnBusinessTripAssignedStaffViewModel param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffController - DeleteEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                //var osPrincipal = (UserIdentity)System.Web.HttpContext.Current.User;
                //var userData = JsonConvert.DeserializeObject<TaiKhoanNguoiDungViewModel>(osPrincipal.UserData);
                //obj.NguoiTao = userData.TenDangNhap;
                if (param.Id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var data = JsonConvert.SerializeObject(param);
                var result = _employeesOnBusinessTripAssignedStaffService.SaveEmployeesOnBusinessTripAssignedStaff(data, (int)ActionCode.Delete);
                _logger.Info("EmployeesOnBusinessTripAssignedStaffController - Param: " + JsonConvert.SerializeObject(param));

                if (result == JsonConvert.SerializeObject(Constants.MessageInformation.Success))
                {
                    _logger.Trace("End EmployeesOnBusinessTripAssignedStaffController - DeleteEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Delete_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EmployeesOnBusinessTripAssignedStaffController - DeleteEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Save_Fail },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new JsonResult()
                {
                    Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Internal_Server_Error },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}