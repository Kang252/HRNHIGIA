using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class AttachmentInformationController : BaseController
    {
        private static int? EmployeeId = 0;
        public JsonResult GetAllAttachmentInformation(int id, int employeeId, int employeesOnBusinessTripId)
        {
            try
            {
                _logger.Trace("Start AttachmentInformationController - GetAllAttachmentInformation: " + DateTime.Now);
                var response = _attachmentInformationService.GetAllAttachmentInformation(id, employeeId, employeesOnBusinessTripId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("AttachmentInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End AttachmentInformationController - GetAllAttachmentInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new AttachmentInformationEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveAttachmentInformation(AttachmentInformationEntity param)
        {
            try
            {
                _logger.Trace("Start AttachmentInformationController - SaveAttachmentInformation: " + DateTime.Now);
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

                EmployeeId = param.EmployeeId;

                var data = JsonConvert.SerializeObject(param);
                var result = _attachmentInformationService.SaveAttachmentInformation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("AttachmentInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End AttachmentInformationController - SaveAttachmentInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End AttachmentInformationController - SaveAttachmentInformation: " + DateTime.Now);
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

        public ActionResult DeleteAttachmentInformation(AttachmentInformationEntity param)
        {
            try
            {
                _logger.Trace("Start AttachmentInformationController - DeleteAttachmentInformation: " + DateTime.Now);
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
                var result = _attachmentInformationService.SaveAttachmentInformation(data, (int)ActionCode.Delete);
                _logger.Info("AttachmentInformationController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End AttachmentInformationController - DeleteAttachmentInformation: " + DateTime.Now);
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

        public ActionResult DownloadAttachmentInformation(int id)
        {
            try
            {
                var result = _attachmentInformationService.DownloadAttachmentInformation(id);
                var cd = new ContentDisposition
                {
                    FileName = result.Data.FileName,
                    Inline = false
                };
                Response.AppendHeader("Content-Disposition", cd.ToString());
                return File(result.Data.FileContent, "application/");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return Json(new { Invalid = true, Msg = string.Empty }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UploadDocument(IEnumerable<HttpPostedFileBase> files)
        {
            var postedFileBases = files as HttpPostedFileBase[] ?? files.ToArray();
            var httpPostedFileBases = files as IList<HttpPostedFileBase> ?? postedFileBases.ToList();
            if (httpPostedFileBases != null)
            {
                foreach (var httpPostedFileBase in httpPostedFileBases)
                {
                    var binaryData = new byte[httpPostedFileBase.InputStream.Length];
                    httpPostedFileBase.InputStream.Read(binaryData, 0, (int)httpPostedFileBase.InputStream.Length);
                    var model = new AttachmentViewModel();
                    model.FileContent = binaryData;
                    model.FileName = httpPostedFileBase.FileName;
                    model.FileSize = httpPostedFileBase.ContentLength;
                    model.FileType = httpPostedFileBase.ContentType;
                    model.EmployeeId = EmployeeId;
                    var cc = JsonConvert.SerializeObject(model);
                    _attachmentInformationService.UpdateAttachmentInformation(cc);
                }
                return new JsonResult()
                {
                    Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.AttachmentUploadedSuccessfully },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            return new JsonResult()
            {
                Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Internal_Server_Error },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}