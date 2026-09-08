using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class SalaryHistoryInformationController : BaseController
    {
        public JsonResult GetAllSalaryHistoryInformation(int id, int employeeId)
        {
            try
            {
                _logger.Trace("Start SalaryHistoryInformationController - GetAllSalaryHistoryInformation: " + DateTime.Now);
                var response = _salaryHistoryInformationService.GetAllSalaryHistoryInformation(id, employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("SalaryHistoryInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End SalaryHistoryInformationController - GetAllSalaryHistoryInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new SalaryHistoryInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveSalaryHistoryInformation(SalaryHistoryInformationEntity param)
        {
            try
            {
                _logger.Trace("Start SalaryHistoryInformationController - SaveSalaryHistoryInformation: " + DateTime.Now);
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

                if (param.Id == 0)
                {
                    var checkSalaryDidNotChange = _salaryHistoryInformationService.GetAllSalaryHistoryInformation(0, param.EmployeeId ?? 0).Data.OrderByDescending(d => d.Id).FirstOrDefault();
                    if (checkSalaryDidNotChange != null)
                    {
                        if (param.BasicSalary == checkSalaryDidNotChange.BasicSalary && param.InsurancePremiums == checkSalaryDidNotChange.InsurancePremiums)
                        {
                            _logger.Trace("End SalaryHistoryInformationController - SaveSalaryHistoryInformation: " + DateTime.Now);
                            return new JsonResult()
                            {
                                Data = new { status = HttpStatusCode.BadRequest, message = Constants.MessageInformation.The_New_Salary_Is_Unchanged_From_The_Current_Salary },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                    }
                }

                var data = JsonConvert.SerializeObject(param);
                var result = _salaryHistoryInformationService.SaveSalaryHistoryInformation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("SalaryHistoryInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End SalaryHistoryInformationController - SaveSalaryHistoryInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End SalaryHistoryInformationController - SaveSalaryHistoryInformation: " + DateTime.Now);
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

        public ActionResult DeleteSalaryHistoryInformation(SalaryHistoryInformationEntity param)
        {
            try
            {
                _logger.Trace("Start SalaryHistoryInformationController - DeleteSalaryHistoryInformation: " + DateTime.Now);
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
                var result = _salaryHistoryInformationService.SaveSalaryHistoryInformation(data, (int)ActionCode.Delete);
                _logger.Info("SalaryHistoryInformationController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End SalaryHistoryInformationController - DeleteSalaryHistoryInformation: " + DateTime.Now);
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