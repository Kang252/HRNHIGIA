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
    public class ContractInformationService : IContractInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ContractInformationRepository _repository = new ContractInformationRepository();

        public ResponseList<ContractInformationViewModel> GetAllContractInformation(int id, int employeeId)
        {
            _logger.Trace("Start ContractInformationService - GetAllContractInformation: " + DateTime.Now);
            var result = _repository.GetAllContractInformation(id, employeeId);
            _logger.Info("ContractInformationService - GetAllContractInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ContractInformationService - GetAllContractInformation: " + DateTime.Now);
            return new ResponseList<ContractInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ContractInformationEntity> SaveContractInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ContractInformationService - SaveContractInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ContractInformationEntity>(content);
                var data = MapperHelper.Map<ContractInformationEntity, TypeContractInformation>(saveData);
                var result = _repository.SaveContractInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("ContractInformationService - SaveContractInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ContractInformationService - SaveContractInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ContractInformationService - SaveContractInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ContractInformationService - SaveContractInformation: " + DateTime.Now);
                return new Response<ContractInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ContractInformationEntity>(false, e.Message, null);
            }
        }
    }
}
