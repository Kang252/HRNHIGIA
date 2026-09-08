using GemBox.Spreadsheet;
using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class EvaluateController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<EvaluateViewModel> GetAllListEvaluateData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start EvaluateController - GetAllListEvaluateData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _evaluateService.GetAllEvaluate(pageDataExcel);
                _logger.Trace("Start EvaluateController - GetAllListEvaluateData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start EvaluateController - GetAllListEvaluateData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _evaluateService.GetAllEvaluate(pageData);
                _logger.Trace("Start EvaluateController - GetAllListEvaluateData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start EvaluateController - GetAllListEvaluateData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetAllEvaluate(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start EvaluateController - GetAllEvaluate: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetAllListEvaluateData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End EvaluateController - GetAllEvaluate: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EvaluateViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEvaluate(EvaluateEntity param)
        {
            try
            {
                _logger.Trace("Start EvaluateController - SaveEvaluate: " + DateTime.Now);
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

                var duplicate = _listCategoryService.CheckDuplicate(param.Id, "Evaluate", param.NameOfAudit, param.EvaluationPeriodId);
                if (duplicate.Data.Duplicate == 1)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.Conflict, message = Constants.MessageInformation.Duplicate },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var data = JsonConvert.SerializeObject(param);
                var result = _evaluateService.SaveEvaluate(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End EvaluateController - SaveEvaluate: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EvaluateController - SaveEvaluate: " + DateTime.Now);
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

        public JsonResult GetEvaluateById(int id)
        {
            try
            {
                _logger.Trace("Start EvaluateController - GetEvaluateById: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _evaluateService.GetEvaluateById(id);

                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EvaluateController - GetEvaluateById: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EvaluateViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteEvaluate(EvaluateEntity param)
        {
            try
            {
                _logger.Trace("Start EvaluateController - DeleteEvaluate: " + DateTime.Now);
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
                var result = _evaluateService.SaveEvaluate(data, (int)ActionCode.Delete);
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End EvaluateController - DeleteEvaluate: " + DateTime.Now);
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

        [HttpPost]
        public FileContentResult EvaluateExportForExcel(string jsonRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonRequest))
                {
                    return null;
                }

                //GemBoxLicense
                SpreadsheetInfo.SetLicense(ConfigurationManager.AppSettings[Constants.GemBox.GemBoxSpreadsheetLicense]);

                var excelFile = new ExcelFile();
                byte[] fileContents;
                var options = SaveOptions.XlsxDefault;
                var fileName = string.Empty;

                GenerateExcelWorksheetEvaluate(excelFile, (int)ExportType.ExportForExcel);

                fileName = Constants.FileName.Evaluate;

                using (var stream = new MemoryStream())
                {
                    excelFile.Save(stream, options);
                    fileContents = stream.ToArray();
                    return File(fileContents, options.ContentType, $"{fileName}_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"ExportExcel FAILED: {e.Message} at {e.StackTrace}");
                return null;
            }
        }

        public ExcelWorksheet GenerateExcelWorksheetEvaluate(ExcelFile excelFile, int exportType)
        {
            var data = new List<Dictionary<string, object>>();
            var header = new List<ExportToExcelColumns>();

            if (exportType == (int)ExportType.ExportForExcel)
            {
                var result = GetAllListEvaluateData(pageData, true);
                if (result != null && result.Data != null)
                {
                    data = DictionaryHelper.DictionaryToList(result.Data);
                }
                header.AddRange(new List<ExportToExcelColumns> {
                new ExportToExcelColumns{DisplayOrder = 1,TitleColumn = Constants.ExportExcelTitleColumn.NameOfAudit,NameColumn = Constants.ExportExcelColumn.NameOfAudit},
                new ExportToExcelColumns{DisplayOrder = 2,TitleColumn = Constants.ExportExcelTitleColumn.EvaluationPeriodId,NameColumn = Constants.ExportExcelColumn.EvaluationPeriodName},
                new ExportToExcelColumns{DisplayOrder = 3,TitleColumn = Constants.ExportExcelTitleColumn.Since,NameColumn = Constants.ExportExcelColumn.Since},
                new ExportToExcelColumns{DisplayOrder = 4,TitleColumn = Constants.ExportExcelTitleColumn.ToDate,NameColumn = Constants.ExportExcelColumn.ToDate},
                new ExportToExcelColumns{DisplayOrder = 5,TitleColumn = Constants.ExportExcelTitleColumn.EvaluationTerm,NameColumn = Constants.ExportExcelColumn.EvaluationTerm},
                new ExportToExcelColumns{DisplayOrder = 6,TitleColumn = Constants.ExportExcelTitleColumn.EvaluationStatusId,NameColumn = Constants.ExportExcelColumn.EvaluationStatusName}});
            }
            //else if (exportType == (int)ExportType.ExportForImport)
            //{
            //    header.AddRange(new List<ExportToExcelColumns> {
            //    new ExportToExcelColumns{DisplayOrder = 1,TitleColumn = Constants.ExportExcelTitleColumn.Id,NameColumn = Constants.ExportExcelColumn.Id},
            //    new ExportToExcelColumns{DisplayOrder = 2,TitleColumn = Constants.ExportExcelTitleColumn.Code,NameColumn = Constants.ExportExcelColumn.Code},
            //    new ExportToExcelColumns{DisplayOrder = 3,TitleColumn = Constants.ExportExcelTitleColumn.Name,NameColumn = Constants.ExportExcelColumn.Name},
            //    new ExportToExcelColumns{DisplayOrder = 4,TitleColumn = Constants.ExportExcelTitleColumn.Address,NameColumn = Constants.ExportExcelColumn.Address},
            //    new ExportToExcelColumns{DisplayOrder = 5,TitleColumn = Constants.ExportExcelTitleColumn.ListCategoryTypeId,NameColumn = Constants.ExportExcelColumn.ListCategoryTypeId}});
            //}

            var ws = new GenerationWorksheet().CreateWorksheet(excelFile, new CommonExcel() { ColumnsList = header, DataList = data, DataCountListCategory = _listCategoryTypeService.GetListCategoryType().Data.Count }, Constants.SheetName.Evaluate, exportType);
            return ws;
        }

        public JsonResult GetAllEvaluateDetail(int evaluateId, int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start EvaluateController - GetAllEvaluateDetail: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                var pageDatas = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = _evaluateService.GetAllEvaluateDetail(pageDatas, evaluateId);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End EvaluateController - GetAllEvaluateDetail: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EvaluateDetailViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEvaluateDetail(EvaluateDetailEntity param)
        {
            try
            {
                _logger.Trace("Start EvaluateController - SaveEvaluateDetail: " + DateTime.Now);
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

                var duplicate = _listCategoryService.CheckDuplicate(param.Id, "EvaluateDetail", Convert.ToString(param.EmployeeId), param.EvaluateId);
                if (duplicate.Data.Duplicate == 1)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.Conflict, message = Constants.MessageInformation.Duplicate },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var data = JsonConvert.SerializeObject(param);
                var result = _evaluateService.SaveEvaluateDetail(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End EvaluateController - SaveEvaluateDetail: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End EvaluateController - SaveEvaluateDetail: " + DateTime.Now);
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

        public ActionResult DeleteEvaluateDetail(EvaluateDetailEntity param)
        {
            try
            {
                _logger.Trace("Start EvaluateController - DeleteEvaluateDetail: " + DateTime.Now);
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
                var result = _evaluateService.SaveEvaluateDetail(data, (int)ActionCode.Delete);
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End EvaluateController - DeleteEvaluateDetail: " + DateTime.Now);
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

        public ActionResult SaveEvaluateForm(EvaluateFormEntity param)
        {
            try
            {
                _logger.Trace("Start EvaluateController - SaveEvaluateForm: " + DateTime.Now);
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

                List<int> foo = new List<int>(){
                    param.EvaluateForm1,
                    param.EvaluateForm2,
                    param.EvaluateForm3,
                    param.EvaluateForm4,
                    param.EvaluateForm5,
                    param.EvaluateForm6,
                    param.EvaluateForm7,
                    param.EvaluateForm8,
                    param.EvaluateForm9,
                    param.EvaluateForm10,
                    param.EvaluateForm11,
                    param.EvaluateForm12,
                    param.EvaluateForm13,
                    param.EvaluateForm14,
                    param.EvaluateForm15,
                    param.EvaluateForm16,
                    param.EvaluateForm17,
                    param.EvaluateForm18,
                    param.EvaluateForm19,
                    param.EvaluateForm20
                };

                int total = foo.Sum(x => Convert.ToInt32(x));

                var dataJson = JsonConvert.SerializeObject(param);

                var paramModel = new EvaluateDetailEntity
                {
                    Id = param.Id,
                    Result = Convert.ToString(total),
                    ResultJson = dataJson
                };

                var data = JsonConvert.SerializeObject(paramModel);
                var result = _evaluateService.SaveEvaluateDetail(data, (int)ActionCode.CreateUpdate);
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End EvaluateController - SaveEvaluateForm: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Evaluate_Success },
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

        public JsonResult GetEvaluateByEmployee(int id, int employeeId)
        {
            try
            {
                _logger.Trace("Start EvaluateController - GetEvaluateByEmployee: " + DateTime.Now);
                var response = _evaluateService.GetEvaluateByEmployee(id, employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("EvaluateController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End EvaluateController - GetEvaluateByEmployee: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new EvaluateDetailViewModel(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}