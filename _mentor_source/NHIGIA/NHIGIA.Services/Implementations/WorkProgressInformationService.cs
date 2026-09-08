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
    public class WorkProgressInformationService : IWorkProgressInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly WorkProgressInformationRepository _repository = new WorkProgressInformationRepository();

        public ResponseList<WorkProgressInformationViewModel> GetAllWorkProgressInformation(int id, int employeeId)
        {
            _logger.Trace("Start WorkProgressInformationService - GetAllWorkProgressInformation: " + DateTime.Now);
            var result = _repository.GetAllWorkProgressInformation(id, employeeId);
            _logger.Info("WorkProgressInformationService - GetAllWorkProgressInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkProgressInformationService - GetAllWorkProgressInformation: " + DateTime.Now);
            return new ResponseList<WorkProgressInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<WorkProgressInformationEntity> SaveWorkProgressInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start WorkProgressInformationService - SaveWorkProgressInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<WorkProgressInformationEntity>(content);
                var data = MapperHelper.Map<WorkProgressInformationEntity, TypeWorkProgressInformation>(saveData);
                var result = _repository.SaveWorkProgressInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("WorkProgressInformationService - SaveWorkProgressInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End WorkProgressInformationService - SaveWorkProgressInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("WorkProgressInformationService - SaveWorkProgressInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End WorkProgressInformationService - SaveWorkProgressInformation: " + DateTime.Now);
                return new Response<WorkProgressInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<WorkProgressInformationEntity>(false, e.Message, null);
            }
        }
    }
}
