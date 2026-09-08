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
    public class PoliticsHealthMilitaryInformationService : IPoliticsHealthMilitaryInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly PoliticsHealthMilitaryInformationRepository _repository = new PoliticsHealthMilitaryInformationRepository();

        public ResponseList<PoliticsHealthMilitaryInformationViewModel> GetAllPoliticsHealthMilitaryInformation(int employeeId)
        {
            _logger.Trace("Start PoliticsHealthMilitaryInformationService - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
            var result = _repository.GetAllPoliticsHealthMilitaryInformation(employeeId);
            _logger.Info("PoliticsHealthMilitaryInformationService - GetAllPoliticsHealthMilitaryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PoliticsHealthMilitaryInformationService - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
            return new ResponseList<PoliticsHealthMilitaryInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<PoliticsHealthMilitaryInformationEntity> SavePoliticsHealthMilitaryInformation(string content)
        {
            try
            {
                _logger.Trace("Start PoliticsHealthMilitaryInformationService - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<PoliticsHealthMilitaryInformationEntity>(content);
                var data = MapperHelper.Map<PoliticsHealthMilitaryInformationEntity, TypePoliticsHealthMilitaryInformation>(saveData);
                var result = _repository.SavePoliticsHealthMilitaryInformation(data);
                if (result != null)
                {
                    _logger.Info("PoliticsHealthMilitaryInformationService - SavePoliticsHealthMilitaryInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End PoliticsHealthMilitaryInformationService - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("PoliticsHealthMilitaryInformationService - SavePoliticsHealthMilitaryInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End PoliticsHealthMilitaryInformationService - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
                return new Response<PoliticsHealthMilitaryInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<PoliticsHealthMilitaryInformationEntity>(false, e.Message, null);
            }
        }
    }
}
