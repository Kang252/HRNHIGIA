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
    public class FamilyInformationService : IFamilyInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly FamilyInformationRepository _repository = new FamilyInformationRepository();

        public ResponseList<FamilyInformationViewModel> GetAllFamilyInformation(int id, int employeeId)
        {
            _logger.Trace("Start FamilyInformationService - GetAllFamilyInformation: " + DateTime.Now);
            var result = _repository.GetAllFamilyInformation(id, employeeId);
            _logger.Info("FamilyInformationService - GetAllFamilyInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End FamilyInformationService - GetAllFamilyInformation: " + DateTime.Now);
            return new ResponseList<FamilyInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<FamilyInformationEntity> SaveFamilyInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start FamilyInformationService - SaveFamilyInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<FamilyInformationEntity>(content);
                var data = MapperHelper.Map<FamilyInformationEntity, TypeFamilyInformation>(saveData);
                var result = _repository.SaveFamilyInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("FamilyInformationService - SaveFamilyInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End FamilyInformationService - SaveFamilyInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("FamilyInformationService - SaveFamilyInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End FamilyInformationService - SaveFamilyInformation: " + DateTime.Now);
                return new Response<FamilyInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<FamilyInformationEntity>(false, e.Message, null);
            }
        }
    }
}
