using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class EmployeeInformationController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<EmployeeInformationViewModel> GetAllListEmployeeInformationData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start EmployeeInformationController - GetAllListEmployeeInformationData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _employeeInformationService.GetAllEmployeeInformation(pageDataExcel);
                _logger.Trace("Start EmployeeInformationController - GetAllListEmployeeInformationData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start EmployeeInformationController - GetAllListEmployeeInformationData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _employeeInformationService.GetAllEmployeeInformation(pageData);
                _logger.Trace("Start EmployeeInformationController - GetAllListEmployeeInformationData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start EmployeeInformationController - GetAllListEmployeeInformationData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetAllEmployeeInformation(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start EmployeeInformationController - GetAllEmployeeInformation: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetAllListEmployeeInformationData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("EmployeeInformationController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End EmployeeInformationController - GetAllEmployeeInformation: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeeInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveProfile(ProfileEntity param)
        {
            try
            {
                _logger.Trace("Start EmployeeInformationController - SaveProfile: " + DateTime.Now);
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
                var result = _employeeInformationService.SaveProfile(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EmployeeInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End EmployeeInformationController - SaveProfile: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EmployeeInformationController - SaveProfile: " + DateTime.Now);
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

        public JsonResult GetEmployeeInformationById(int id)
        {
            try
            {
                _logger.Trace("Start EmployeeInformationController - GetEmployeeInformationById: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _employeeInformationService.GetEmployeeInformationById(id);

                _logger.Info("EmployeeInformationController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeeInformationController - GetEmployeeInformationById: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ProfileEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetEmployeeForAutoCompleBox(string keyword)
        {
            _logger.Trace("Start EmployeeInformationController - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            _logger.Info($"keyword: {keyword}");
            var resultDataSource = new DataSourceResult
            {
                Data = new List<EmployeeInformationViewModel>(),
                Total = 0
            };

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(resultDataSource, JsonRequestBehavior.AllowGet);

            }

            var response = _employeeInformationService.GetEmployeeForAutoCompleBox(keyword);

            if (response.Success && response.Data != null)
            {
                resultDataSource.Data = response.Data;
                resultDataSource.Total = response.Data.Count;
            }

            _logger.Info("EmployeeInformationController - Param: " + JsonConvert.SerializeObject(response.Data));
            _logger.Trace("End EmployeeInformationController - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployee(int page, int pageSize, string sortColumn, string sortType, string filterColumn, int id, string type)
        {
            try
            {
                _logger.Trace("Start EmployeeInformationController - GetEmployee: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                var pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = _employeeInformationService.GetEmployee(pageData, id, type);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("EmployeeInformationController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End EmployeeInformationController - GetEmployee: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EmployeeInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

    }
}