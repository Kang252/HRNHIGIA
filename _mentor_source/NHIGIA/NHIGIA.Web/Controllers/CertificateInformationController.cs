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
    public class CertificateInformationController : BaseController
    {
        public JsonResult GetAllCertificateInformation(int id, int employeeId)
        {
            try
            {
                _logger.Trace("Start CertificateInformationController - GetAllCertificateInformation: " + DateTime.Now);
                var response = _certificateInformationService.GetAllCertificateInformation(id, employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("CertificateInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End CertificateInformationController - GetAllCertificateInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new CertificateInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveCertificateInformation(CertificateInformationEntity param)
        {
            try
            {
                _logger.Trace("Start CertificateInformationController - SaveCertificateInformation: " + DateTime.Now);
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
                var result = _certificateInformationService.SaveCertificateInformation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("CertificateInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End CertificateInformationController - SaveCertificateInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End CertificateInformationController - SaveCertificateInformation: " + DateTime.Now);
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

        public ActionResult DeleteCertificateInformation(CertificateInformationEntity param)
        {
            try
            {
                _logger.Trace("Start CertificateInformationController - DeleteCertificateInformation: " + DateTime.Now);
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
                var result = _certificateInformationService.SaveCertificateInformation(data, (int)ActionCode.Delete);
                _logger.Info("CertificateInformationController - Param: " + JsonConvert.SerializeObject(param));
                _logger.Trace("End CertificateInformationController - DeleteCertificateInformation: " + DateTime.Now);
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