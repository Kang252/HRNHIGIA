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
    public class BonusInformationStaffWereCommendedController : BaseController
    {
        public JsonResult GetAllBonusInformationStaffWereCommended(int bonusInformationId)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedController - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
                var response = _bonusInformationStaffWereCommendedService.GetAllBonusInformationStaffWereCommended(bonusInformationId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BonusInformationStaffWereCommendedController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BonusInformationStaffWereCommendedController - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new BonusInformationStaffWereCommendedViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveBonusInformationStaffWereCommended(BonusInformationStaffWereCommendedViewModel param)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedController - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
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
                var result = _bonusInformationStaffWereCommendedService.SaveBonusInformationStaffWereCommended(data, (int)ActionCode.CreateUpdate);
                _logger.Info("BonusInformationStaffWereCommendedController - Param: " + JsonConvert.SerializeObject(param));

                if (result == JsonConvert.SerializeObject(Constants.MessageInformation.Success))
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Save_Fail },
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

        public ActionResult DeleteBonusInformationStaffWereCommended(BonusInformationStaffWereCommendedViewModel param)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedController - DeleteBonusInformationStaffWereCommended: " + DateTime.Now);
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
                var result = _bonusInformationStaffWereCommendedService.SaveBonusInformationStaffWereCommended(data, (int)ActionCode.Delete);
                _logger.Info("BonusInformationStaffWereCommendedController - Param: " + JsonConvert.SerializeObject(param));

                if (result == JsonConvert.SerializeObject(Constants.MessageInformation.Success))
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - DeleteBonusInformationStaffWereCommended: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Delete_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - DeleteBonusInformationStaffWereCommended: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.InternalServerError, message = Constants.MessageInformation.Save_Fail },
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

        public ActionResult SaveOnlyBonusInformationStaffWereCommended(BonusInformationStaffWereCommendedEntity param)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedController - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
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

                param.BonusValue = param.BonusValue.Replace(",", string.Empty);
                var data = JsonConvert.SerializeObject(param);
                var result = _bonusInformationStaffWereCommendedService.SaveOnlyBonusInformationStaffWereCommended(data, (int)ActionCode.CreateUpdate);
                _logger.Info("BonusInformationStaffWereCommendedController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End BonusInformationStaffWereCommendedController - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
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
    }
}