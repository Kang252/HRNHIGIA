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
    public class BonusInformationService : IBonusInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BonusInformationRepository _repository = new BonusInformationRepository();

        public ResponseList<BonusInformationViewModel> GetAllBonus(PagingData param)
        {
            _logger.Trace("Start BonusInformationService - GetAllBonus: " + DateTime.Now);
            var result = _repository.GetAllBonus(param);
            _logger.Info("BonusInformationService - GetAllBonus - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationService - GetAllBonus: " + DateTime.Now);
            return new ResponseList<BonusInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<BonusInformationViewModel> GetAllBonusInformation(int employeeId)
        {
            _logger.Trace("Start BonusInformationService - GetAllBonusInformation: " + DateTime.Now);
            var result = _repository.GetAllBonusInformation(employeeId);
            _logger.Info("BonusInformationService - GetAllBonusInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationService - GetAllBonusInformation: " + DateTime.Now);
            return new ResponseList<BonusInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<BonusInformationViewModel> GetBonusById(int id)
        {
            _logger.Trace("Start BonusInformationService - GetBonusById: " + DateTime.Now);
            var result = _repository.GetBonusById(id);
            _logger.Info("BonusInformationService - GetBonusById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationService - GetBonusById: " + DateTime.Now);
            return result;
        }

        public Response<BonusInformationEntity> SaveBonusInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start BonusInformationService - SaveBonusInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<BonusInformationEntity>(content);
                var data = MapperHelper.Map<BonusInformationEntity, TypeBonusInformation>(saveData);
                var result = _repository.SaveBonusInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("BonusInformationService - SaveBonusInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End BonusInformationService - SaveBonusInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("BonusInformationService - SaveBonusInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End BonusInformationService - SaveBonusInformation: " + DateTime.Now);
                return new Response<BonusInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<BonusInformationEntity>(false, e.Message, null);
            }
        }
    }
}
