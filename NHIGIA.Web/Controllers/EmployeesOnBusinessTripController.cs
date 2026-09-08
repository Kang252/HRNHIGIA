using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class EmployeesOnBusinessTripController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<EmployeesOnBusinessTripViewModel> GetAllListEmployeesOnBusinessTripData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start EmployeesOnBusinessTripController - GetAllListEmployeesOnBusinessTripData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _employeesOnBusinessTripService.GetAllEmployeesOnBusinessTrip(pageDataExcel);
                _logger.Trace("Start EmployeesOnBusinessTripController - GetAllListEmployeesOnBusinessTripData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start EmployeesOnBusinessTripController - GetAllListEmployeesOnBusinessTripData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _employeesOnBusinessTripService.GetAllEmployeesOnBusinessTrip(pageData);
                _logger.Trace("Start EmployeesOnBusinessTripController - GetAllListEmployeesOnBusinessTripData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start EmployeesOnBusinessTripController - GetAllListEmployeesOnBusinessTripData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetAllEmployeesOnBusinessTrip(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripController - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetAllListEmployeesOnBusinessTripData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("EmployeesOnBusinessTripController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End EmployeesOnBusinessTripController - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeesOnBusinessTripViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEmployeesOnBusinessTrip(EmployeesOnBusinessTripEntity param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripController - SaveProfile: " + DateTime.Now);
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
                var result = _employeesOnBusinessTripService.SaveEmployeesOnBusinessTrip(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EmployeesOnBusinessTripController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End EmployeesOnBusinessTripController - SaveEmployeesOnBusinessTrip: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EmployeesOnBusinessTripController - SaveEmployeesOnBusinessTrip: " + DateTime.Now);
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

        public JsonResult GetEmployeesOnBusinessTripById(int id)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripController - GetEmployeesOnBusinessTripById: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _employeesOnBusinessTripService.GetEmployeesOnBusinessTripById(id);

                _logger.Info("EmployeesOnBusinessTripController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeesOnBusinessTripController - GetEmployeesOnBusinessTripById: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeesOnBusinessTripViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetEmployeesOnBusinessTripInformation(int employeeId)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripController - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
                var response = _employeesOnBusinessTripService.GetEmployeesOnBusinessTripInformation(employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("EmployeesOnBusinessTripController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End EmployeesOnBusinessTripController - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeesOnBusinessTripViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteEmployeesOnBusinessTrip(EmployeesOnBusinessTripEntity param)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripController - DeleteEmployeesOnBusinessTrip: " + DateTime.Now);
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
                var result = _employeesOnBusinessTripService.SaveEmployeesOnBusinessTrip(data, (int)ActionCode.Delete);
                _logger.Info("EmployeesOnBusinessTripController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End EmployeesOnBusinessTripController - DeleteEmployeesOnBusinessTrip: " + DateTime.Now);
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