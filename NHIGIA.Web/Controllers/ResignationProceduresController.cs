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
    public class ResignationProceduresController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<ResignationProceduresViewModel> GetResignationProceduresData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start ResignationProceduresController - GetResignationProceduresData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _resignationProceduresService.GetResignationProcedures(pageDataExcel);
                _logger.Trace("Start ResignationProceduresController - GetResignationProceduresData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start ResignationProceduresController - GetResignationProceduresData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _resignationProceduresService.GetResignationProcedures(pageData);
                _logger.Trace("Start ResignationProceduresController - GetResignationProceduresData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start ResignationProceduresController - GetResignationProceduresData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetResignationProcedures(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresController - GetResignationProcedures: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetResignationProceduresData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("ResignationProceduresController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End ResignationProceduresController - GetResignationProcedures: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ResignationProceduresViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveResignationProcedures(ResignationProceduresEntity param)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresController - SaveResignationProcedures: " + DateTime.Now);
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
                var result = _resignationProceduresService.SaveResignationProcedures(data, (int)ActionCode.CreateUpdate);
                _logger.Info("ResignationProceduresController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End ResignationProceduresController - SaveResignationProcedures: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End ResignationProceduresController - SaveResignationProcedures: " + DateTime.Now);
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

        public ActionResult DeleteResignationProcedures(ResignationProceduresEntity param)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresController - DeleteResignationProcedures: " + DateTime.Now);
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
                var result = _resignationProceduresService.SaveResignationProcedures(data, (int)ActionCode.Delete);
                _logger.Info("ResignationProceduresController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End ResignationProceduresController - DeleteResignationProcedures: " + DateTime.Now);
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

        public JsonResult GetGeneralInformationForResignationProcedures(int id)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresController - GetGeneralInformationForResignationProcedures: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _resignationProceduresService.GetGeneralInformationForResignationProcedures(id);

                _logger.Info("ResignationProceduresController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ResignationProceduresController - GetGeneralInformationForResignationProcedures: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ReceiveInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}