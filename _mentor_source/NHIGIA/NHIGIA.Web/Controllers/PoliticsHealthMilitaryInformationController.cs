using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class PoliticsHealthMilitaryInformationController : BaseController
    {
        public JsonResult GetAllPoliticsHealthMilitaryInformation(int employeeId)
        {
            try
            {
                _logger.Trace("Start PoliticsHealthMilitaryInformationController - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
                var response = _politicsHealthMilitaryInformationService.GetAllPoliticsHealthMilitaryInformation(employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("PoliticsHealthMilitaryInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End PoliticsHealthMilitaryInformationController - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new PoliticsHealthMilitaryInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SavePoliticsHealthMilitaryInformation(PoliticsHealthMilitaryInformationEntity param)
        {
            try
            {
                _logger.Trace("Start PoliticsHealthMilitaryInformationController - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
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
                var result = _politicsHealthMilitaryInformationService.SavePoliticsHealthMilitaryInformation(data);
                _logger.Info("PoliticsHealthMilitaryInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End PoliticsHealthMilitaryInformationController - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End PoliticsHealthMilitaryInformationController - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
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