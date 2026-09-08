using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;

namespace NHIGIA.Services.Implementations
{
    public class EmployeeInformationService : IEmployeeInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeeInformationRepository _repository = new EmployeeInformationRepository();

        public ResponseList<EmployeeInformationViewModel> GetAllEmployeeInformation(PagingData param)
        {
            _logger.Trace("Start EmployeeInformationService - GetAllEmployeeInformation: " + DateTime.Now);
            var result = _repository.GetAllEmployeeInformation(param);
            _logger.Info("EmployeeInformationService - GetAllEmployeeInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationService - GetAllEmployeeInformation: " + DateTime.Now);
            return new ResponseList<EmployeeInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<EmployeeInformationViewModel> GetEmployee(PagingData param, int id, string type)
        {
            _logger.Trace("Start EmployeeInformationService - GetEmployee: " + DateTime.Now);
            var result = _repository.GetEmployee(param, id, type);
            _logger.Info("EmployeeInformationService - GetEmployee - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationService - GetEmployee: " + DateTime.Now);
            return new ResponseList<EmployeeInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<EmployeeInformationViewModel> GetEmployeeForAutoCompleBox(string keyword)
        {
            _logger.Trace("Start EmployeeInformationService - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            var result = _repository.GetEmployeeForAutoCompleBox(keyword);
            _logger.Info("EmployeeInformationService - GetEmployeeForAutoCompleBox - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationService - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            return result;
        }

        public Response<ProfileEntity> GetEmployeeInformationById(int id)
        {
            _logger.Trace("Start EmployeeInformationService - GetEmployeeInformationById: " + DateTime.Now);
            var result = _repository.GetEmployeeInformationById(id);
            _logger.Info("EmployeeInformationService - GetEmployeeInformationById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationService - GetEmployeeInformationById: " + DateTime.Now);
            return result;
        }

        public Response<ProfileEntity> SaveProfile(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeeInformationService - SaveProfile: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ProfileEntity>(content);
                var data = MapperHelper.Map<ProfileEntity, TypeProfile>(saveData);
                var result = _repository.SaveProfile(data, isAction);
                if (result != null)
                {
                    _logger.Info("EmployeeInformationService - SaveProfile - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeeInformationService - SaveProfile: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EmployeeInformationService - SaveProfile - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeeInformationService - SaveProfile: " + DateTime.Now);
                return new Response<ProfileEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ProfileEntity>(false, e.Message, null);
            }
        }
    }
}
