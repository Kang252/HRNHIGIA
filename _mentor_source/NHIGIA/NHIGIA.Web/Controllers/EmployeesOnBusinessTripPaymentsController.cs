using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class EmployeesOnBusinessTripPaymentsController : BaseController
    {
        public JsonResult GetAllEmployeesOnBusinessTripPayments(int id, int employeesOnBusinessTripId)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripPaymentsController - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
                var response = _employeesOnBusinessTripPaymentsService.GetAllEmployeesOnBusinessTripPayments(id, employeesOnBusinessTripId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("EmployeesOnBusinessTripPaymentsController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End EmployeesOnBusinessTripPaymentsController - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeesOnBusinessTripPaymentsViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEmployeesOnBusinessTripPayments(EmployeesOnBusinessTripPaymentsEntity param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripPaymentsController - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
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
                var result = _employeesOnBusinessTripPaymentsService.SaveEmployeesOnBusinessTripPayments(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EmployeesOnBusinessTripPaymentsController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End EmployeesOnBusinessTripPaymentsController - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EmployeesOnBusinessTripPaymentsController - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Save_Fail },
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

        public ActionResult DeleteEmployeesOnBusinessTripPayments(EmployeesOnBusinessTripPaymentsEntity param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripPaymentsController - DeleteEmployeesOnBusinessTripPayments: " + DateTime.Now);
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
                var result = _employeesOnBusinessTripPaymentsService.SaveEmployeesOnBusinessTripPayments(data, (int)ActionCode.Delete);
                _logger.Info("EmployeesOnBusinessTripPaymentsController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End EmployeesOnBusinessTripPaymentsController - DeleteEmployeesOnBusinessTripPayments: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Delete_Success },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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