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
    public class BonusInformationController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<BonusInformationViewModel> GetAllBonusData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start BonusInformationController - GetAllBonusData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _bonusInformationService.GetAllBonus(pageDataExcel);
                _logger.Trace("Start BonusInformationController - GetAllBonusData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start BonusInformationController - GetAllBonusData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _bonusInformationService.GetAllBonus(pageData);
                _logger.Trace("Start BonusInformationController - GetAllBonusData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start BonusInformationController - GetAllBonusData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetAllBonus(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start BonusInformationController - GetAllBonus: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetAllBonusData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("BonusInformationController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End BonusInformationController - GetAllBonus: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new BonusInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllBonusInformation(int employeeId)
        {
            try
            {
                _logger.Trace("Start BonusInformationController - GetAllBonusInformation: " + DateTime.Now);
                var response = _bonusInformationService.GetAllBonusInformation(employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BonusInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BonusInformationController - GetAllBonusInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new BonusInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveBonusInformation(BonusInformationEntity param)
        {
            try
            {
                _logger.Trace("Start BonusInformationController - SaveBonusInformation: " + DateTime.Now);
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
                var result = _bonusInformationService.SaveBonusInformation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("BonusInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End BonusInformationController - SaveBonusInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End BonusInformationController - SaveBonusInformation: " + DateTime.Now);
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

        public ActionResult DeleteBonusInformation(BonusInformationEntity param)
        {
            try
            {
                _logger.Trace("Start BonusInformationController - DeleteBonusInformation: " + DateTime.Now);
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
                var result = _bonusInformationService.SaveBonusInformation(data, (int)ActionCode.Delete);
                _logger.Info("BonusInformationController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End BonusInformationController - DeleteBonusInformation: " + DateTime.Now);
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

        public JsonResult GetBonusById(int id)
        {
            try
            {
                _logger.Trace("Start BonusInformationController - GetBonusById: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _bonusInformationService.GetBonusById(id);

                _logger.Info("BonusInformationController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End BonusInformationController - GetBonusById: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new BonusInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}