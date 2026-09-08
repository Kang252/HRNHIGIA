using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.DtoEntities;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class BulkUploadController : BaseController
    {
        public ActionResult ImportDataListCategory(HttpPostedFileBase examResultFileListCategory)
        {
            _logger.Trace("Start BulkUploadController - ImportDataListCategory: " + DateTime.Now);
            //var osPrincipal = (UserIdentity)System.Web.HttpContext.Current.User;
            //var userData = JsonConvert.DeserializeObject<UserViewModel>(osPrincipal.UserData);
            if (examResultFileListCategory != null)
            {
                var stream = examResultFileListCategory.InputStream;
                var fileBinary = new byte[stream.Length];

                stream.Read(fileBinary, 0, fileBinary.Length);

                var listData = new List<ListCategoryEntity>();
                var listMessage = new List<MessageDto>();
                using (var excel = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var ws = excel.Workbook.Worksheets.First();
                    var rowCount = ws.Dimension.End.Row;

                    if (rowCount < 2)
                    {
                        return new JsonResult()
                        {
                            Data = new { status = HttpStatusCode.NoContent, message = Constants.MessageInformation.MsgEmptyFileError },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }

                    //get row count
                    for (var row = 2; row <= rowCount; row++)
                    {
                        if (string.IsNullOrWhiteSpace(Convert.ToString(ws.Cells[row, 3].Value)) || Convert.ToInt32(ws.Cells[row, 5].Value).Equals(0))
                        {
                            return new JsonResult()
                            {
                                Data = new { status = HttpStatusCode.LengthRequired, message = Constants.MessageInformation.RequiredListCategory },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }

                        string authors = Convert.ToString(ws.Cells[row, 3].Value).ToUpper();
                        string[] authorsList = authors.Trim().Split(' ');
                        var name_value = authorsList[0].Substring(0, 1);
                        for (int i = 0; i <= authorsList.Length - 1; i++)
                        {
                            name_value = name_value + authorsList[i].ToCharArray()[0];
                        }

                        var duplicate = _listCategoryService.CheckDuplicate(0, "ListCategory", Convert.ToString(ws.Cells[row, 3].Value), Convert.ToInt32(ws.Cells[row, 5].Value));
                        if (duplicate.Data.Duplicate == 1)
                        {
                            var lstMessage = new MessageDto
                            {
                                Message = "Tên danh mục: " + Convert.ToString(ws.Cells[row, 3].Value) + " & Loại danh mục: " + Convert.ToInt32(ws.Cells[row, 5].Value) + Constants.MessageInformation.Duplicate
                            };
                            listMessage.Add(lstMessage);
                        }

                        var listModel = new ListCategoryEntity
                        {
                            Code = name_value.Substring(1, name_value.Length - 1),
                            Name = Convert.ToString(ws.Cells[row, 3].Value),
                            Address = Convert.ToString(ws.Cells[row, 4].Value),
                            ListCategoryTypeId = Convert.ToInt32(ws.Cells[row, 5].Value),
                            CreatedBy = string.Empty,
                            ModifiedBy = string.Empty
                        };
                        listData.Add(listModel);
                    }
                }

                try
                {
                    List<string> duplicateImport = listMessage.Select(a => a.Message).ToList();
                    if (duplicateImport.Count > 0)
                    {
                        var mess = string.Join(Constants.Br, duplicateImport);
                        return new JsonResult()
                        {
                            Data = new { status = HttpStatusCode.Conflict, message = mess },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }

                    var param = new RequestDto<ImportDataViewModel>()
                    {
                        Data = new ImportDataViewModel()
                        {
                            ListCategoryEntityUpload = listData
                        }
                    };
                    _logger.Trace("BulkUploadController - ListData: " + JsonConvert.SerializeObject(listData));
                    var result = _bulkUploadService.ImportDataListCategory(param, (int)ActionCode.CreateUpdate);
                    _logger.Trace("End BulkUploadController - ImportDataListCategory: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Import_Success },
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
            else
            {
                return new JsonResult()
                {
                    Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.File_Null },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}