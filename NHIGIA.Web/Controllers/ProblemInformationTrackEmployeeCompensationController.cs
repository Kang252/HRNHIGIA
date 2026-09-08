using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.ViewModel;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class ProblemInformationTrackEmployeeCompensationController : BaseController
    {
        public JsonResult GetAllProblemInformationTrackEmployeeCompensation(int problemInformationId, int employeeId, int type)
        {
            try
            {
                _logger.Trace("Start ProblemInformationTrackEmployeeCompensationController - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                var response = _problemInformationTrackEmployeeCompensationService.GetAllProblemInformationTrackEmployeeCompensation(problemInformationId, employeeId, type);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("ProblemInformationTrackEmployeeCompensationController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End ProblemInformationTrackEmployeeCompensationController - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ProblemInformationTrackEmployeeCompensationViewModel(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveProblemInformationTrackEmployeeCompensation(ProblemInformationTrackEmployeeCompensationViewModel param)
        {
            try
            {
                _logger.Trace("Start ProblemInformationTrackEmployeeCompensationController - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
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
                var result = _problemInformationTrackEmployeeCompensationService.SaveProblemInformationTrackEmployeeCompensation(data, (int)ActionCode.CreateUpdate);
                _logger.Info("ProblemInformationTrackEmployeeCompensationController - Param: " + JsonConvert.SerializeObject(param));

                if (result == JsonConvert.SerializeObject(Constants.MessageInformation.Success))
                {
                    _logger.Trace("End ProblemInformationTrackEmployeeCompensationController - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.OK, message = Constants.MessageInformation.Save_Success },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    _logger.Trace("End ProblemInformationTrackEmployeeCompensationController - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
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
    }
}