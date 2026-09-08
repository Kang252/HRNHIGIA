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
    public class SalaryHistoryInformationService : ISalaryHistoryInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SalaryHistoryInformationRepository _repository = new SalaryHistoryInformationRepository();

        public ResponseList<SalaryHistoryInformationViewModel> GetAllSalaryHistoryInformation(int id, int employeeId)
        {
            _logger.Trace("Start SalaryHistoryInformationService - GetAllSalaryHistoryInformation: " + DateTime.Now);
            var result = _repository.GetAllSalaryHistoryInformation(id, employeeId);
            _logger.Info("SalaryHistoryInformationService - GetAllSalaryHistoryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SalaryHistoryInformationService - GetAllSalaryHistoryInformation: " + DateTime.Now);
            return new ResponseList<SalaryHistoryInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<SalaryHistoryInformationEntity> SaveSalaryHistoryInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start SalaryHistoryInformationService - SaveSalaryHistoryInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<SalaryHistoryInformationEntity>(content);
                var data = MapperHelper.Map<SalaryHistoryInformationEntity, TypeSalaryHistoryInformation>(saveData);
                var result = _repository.SaveSalaryHistoryInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("SalaryHistoryInformationService - SaveSalaryHistoryInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End SalaryHistoryInformationService - SaveSalaryHistoryInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("SalaryHistoryInformationService - SaveSalaryHistoryInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End SalaryHistoryInformationService - SaveSalaryHistoryInformation: " + DateTime.Now);
                return new Response<SalaryHistoryInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<SalaryHistoryInformationEntity>(false, e.Message, null);
            }
        }
    }
}
