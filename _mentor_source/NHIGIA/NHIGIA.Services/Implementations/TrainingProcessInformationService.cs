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
    public class TrainingProcessInformationService : ITrainingProcessInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly TrainingProcessInformationRepository _repository = new TrainingProcessInformationRepository();

        public ResponseList<TrainingProcessInformationViewModel> GetAllTrainingProcessInformation(int id, int employeeId)
        {
            _logger.Trace("Start TrainingProcessInformationService - GetAllTrainingProcessInformation: " + DateTime.Now);
            var result = _repository.GetAllTrainingProcessInformation(id, employeeId);
            _logger.Info("TrainingProcessInformationService - GetAllTrainingProcessInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End TrainingProcessInformationService - GetAllTrainingProcessInformation: " + DateTime.Now);
            return new ResponseList<TrainingProcessInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<TrainingProcessInformationEntity> SaveTrainingProcessInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start TrainingProcessInformationService - SaveTrainingProcessInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<TrainingProcessInformationEntity>(content);
                var data = MapperHelper.Map<TrainingProcessInformationEntity, TypeTrainingProcessInformation>(saveData);
                var result = _repository.SaveTrainingProcessInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("TrainingProcessInformationService - SaveTrainingProcessInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End TrainingProcessInformationService - SaveTrainingProcessInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("TrainingProcessInformationService - SaveTrainingProcessInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End TrainingProcessInformationService - SaveTrainingProcessInformation: " + DateTime.Now);
                return new Response<TrainingProcessInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<TrainingProcessInformationEntity>(false, e.Message, null);
            }
        }
    }
}
