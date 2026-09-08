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
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class ListCategoryController : BaseController
    {
        private static PagingData pageData = new PagingData();

        public ResponseList<ListCategoryViewModel> GetAllListCategoryData(PagingData pageData, bool isExport)
        {
            _logger.Trace("Start ListCategoryController - GetAllListCategoryData: " + DateTime.Now);
            if (isExport == true)
            {
                var pageDataExcel = new PagingData(1, 999999999, pageData.FilterColumns, pageData.SortColumns);
                var resultDataExcel = _listCategoryService.GetAllListCategory(pageDataExcel);
                _logger.Trace("Start ListCategoryController - GetAllListCategoryData Data: " + JsonConvert.SerializeObject(resultDataExcel));
                _logger.Trace("Start ListCategoryController - GetAllListCategoryData: " + DateTime.Now);
                return resultDataExcel;
            }
            else
            {
                var result = _listCategoryService.GetAllListCategory(pageData);
                _logger.Trace("Start ListCategoryController - GetAllListCategoryData Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("Start ListCategoryController - GetAllListCategoryData: " + DateTime.Now);
                return result;
            }
        }

        public JsonResult GetAllListCategory(int page, int pageSize, string sortColumn, string sortType, string filterColumn)
        {
            try
            {
                _logger.Trace("Start ListCategoryController - GetAllListCategory: " + DateTime.Now);
                //Sorting and filtering parameters
                var sortingParameters = CreateSortingParameters(sortColumn, sortType);
                var filteringParameters = CreateFilteringParameters(filterColumn);

                pageData = new PagingData(page, pageSize, filteringParameters, sortingParameters);

                var response = GetAllListCategoryData(pageData, false);

                var result = new DataSourceResult
                {
                    Data = response.Data,
                    Total = response.Total
                };

                _logger.Info("ListCategoryController - Param: " + JsonConvert.SerializeObject(pageData));
                _logger.Trace("End ListCategoryController - GetAllListCategory: " + DateTime.Now);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ListCategoryViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetListCategoryById(int id)
        {
            try
            {
                _logger.Trace("Start ListCategoryController - GetListCategoryById: " + DateTime.Now);

                if (id == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Id },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                var result = _listCategoryService.GetListCategoryById(id);

                _logger.Info("ListCategoryController - Param: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ListCategoryController - GetListCategoryById: " + DateTime.Now);
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ListCategoryEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveListCategory(ListCategoryEntity param)
        {
            try
            {
                _logger.Trace("Start ListCategoryController - SaveListCategory: " + DateTime.Now);
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

                var duplicate = _listCategoryService.CheckDuplicate(param.Id, "ListCategory", param.Name, param.ListCategoryTypeId);
                if (duplicate.Data.Duplicate == 1)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.Conflict, message = Constants.MessageInformation.Duplicate },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                if (param.Id == 0)
                {
                    var name_value = string.Empty;
                    string authors = param.Name.ToUpper();
                    string[] authorsList = authors.Trim().Split(' ');
                    name_value = authorsList[0].Substring(0, 1);
                    for (int i = 0; i <= authorsList.Length - 1; i++)
                    {
                        name_value = name_value + authorsList[i].ToCharArray()[0];
                    }
                    param.Code = name_value.Substring(1, name_value.Length - 1);
                }
                var data = JsonConvert.SerializeObject(param);
                var result = _listCategoryService.SaveListCategory(data, (int)ActionCode.CreateUpdate);
                _logger.Info("ListCategoryController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End ListCategoryController - SaveListCategory: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End ListCategoryController - SaveListCategory: " + DateTime.Now);
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

        public ActionResult DeleteListCategory(ListCategoryEntity param)
        {
            try
            {
                _logger.Trace("Start ListCategoryController - DeleteListCategory: " + DateTime.Now);
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
                var result = _listCategoryService.SaveListCategory(data, (int)ActionCode.Delete);
                _logger.Info("ListCategoryController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End ListCategoryController - DeleteListCategory: " + DateTime.Now);
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
        public FileContentResult ExportForExcel(string jsonRequest)
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

                GenerateExcelWorksheetListCategory(excelFile, (int)ExportType.ExportForExcel);

                fileName = Constants.FileName.ListCategory;

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

        [HttpPost]
        public FileContentResult ExportForImport(string jsonRequest)
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

                GenerateExcelWorksheetListCategory(excelFile, (int)ExportType.ExportForImport);
                GenerateExcelWorksheetListCategoryType(excelFile, (int)ExportType.None);

                fileName = Constants.FileNameTemplate.TemplateListCategory;

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

        public ExcelWorksheet GenerateExcelWorksheetListCategory(ExcelFile excelFile, int exportType)
        {
            var data = new List<Dictionary<string, object>>();
            var header = new List<ExportToExcelColumns>();

            if (exportType == (int)ExportType.ExportForExcel)
            {
                var result = GetAllListCategoryData(pageData, true);
                if (result != null && result.Data != null)
                {
                    data = DictionaryHelper.DictionaryToList(result.Data);
                }
                header.AddRange(new List<ExportToExcelColumns> {
                new ExportToExcelColumns{DisplayOrder = 2,TitleColumn = Constants.ExportExcelTitleColumn.Code,NameColumn = Constants.ExportExcelColumn.Code},
                new ExportToExcelColumns{DisplayOrder = 3,TitleColumn = Constants.ExportExcelTitleColumn.Name,NameColumn = Constants.ExportExcelColumn.Name},
                new ExportToExcelColumns{DisplayOrder = 4,TitleColumn = Constants.ExportExcelTitleColumn.Address,NameColumn = Constants.ExportExcelColumn.Address},
                new ExportToExcelColumns{DisplayOrder = 5,TitleColumn = Constants.ExportExcelTitleColumn.ListCategoryTypeId,NameColumn = Constants.ExportExcelColumn.ListCategoryTypeName}});
            }
            else if (exportType == (int)ExportType.ExportForImport)
            {
                header.AddRange(new List<ExportToExcelColumns> {
                new ExportToExcelColumns{DisplayOrder = 1,TitleColumn = Constants.ExportExcelTitleColumn.Id,NameColumn = Constants.ExportExcelColumn.Id},
                new ExportToExcelColumns{DisplayOrder = 2,TitleColumn = Constants.ExportExcelTitleColumn.Code,NameColumn = Constants.ExportExcelColumn.Code},
                new ExportToExcelColumns{DisplayOrder = 3,TitleColumn = Constants.ExportExcelTitleColumn.Name,NameColumn = Constants.ExportExcelColumn.Name},
                new ExportToExcelColumns{DisplayOrder = 4,TitleColumn = Constants.ExportExcelTitleColumn.Address,NameColumn = Constants.ExportExcelColumn.Address},
                new ExportToExcelColumns{DisplayOrder = 5,TitleColumn = Constants.ExportExcelTitleColumn.ListCategoryTypeId,NameColumn = Constants.ExportExcelColumn.ListCategoryTypeId}});
            }

            var ws = new GenerationWorksheet().CreateWorksheet(excelFile, new CommonExcel() { ColumnsList = header, DataList = data, DataCountListCategoryType = _listCategoryTypeService.GetListCategoryType().Data.Count }, Constants.SheetName.ListCategory, exportType);
            return ws;
        }
    }
}