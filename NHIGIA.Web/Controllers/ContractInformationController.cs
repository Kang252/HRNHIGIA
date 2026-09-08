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
    public class ContractInformationController : BaseController
    {
        public JsonResult GetAllContractInformation(int id, int employeeId)
        {
            try
            {
                _logger.Trace("Start ContractInformationController - GetAllContractInformation: " + DateTime.Now);
                var response = _contractInformationService.GetAllContractInformation(id, employeeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("ContractInformationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End ContractInformationController - GetAllContractInformation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ContractInformationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveContractInformation(ContractInformationEntity param)
        {
            try
            {
                _logger.Trace("Start ContractInformationController - SaveContractInformation: " + DateTime.Now);
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
                var result = _contractInformationService.SaveContractInformation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("ContractInformationController - Param: " + JsonConvert.SerializeObject(param));

                if (result.Success)
                {
                    _logger.Trace("End ContractInformationController - SaveContractInformation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { result.Data, status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End ContractInformationController - SaveContractInformation: " + DateTime.Now);
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