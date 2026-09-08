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
    public class QuitInformationService : IQuitInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly QuitInformationRepository _repository = new QuitInformationRepository();

        public ResponseList<QuitInformationViewModel> GetAllQuitInformation(int id, int employeeId)
        {
            _logger.Trace("Start QuitInformationService - GetAllQuitInformation: " + DateTime.Now);
            var result = _repository.GetAllQuitInformation(id, employeeId);
            _logger.Info("QuitInformationService - GetAllQuitInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End QuitInformationService - GetAllQuitInformation: " + DateTime.Now);
            return new ResponseList<QuitInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<QuitInformationEntity> SaveQuitInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start QuitInformationService - SaveQuitInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<QuitInformationEntity>(content);
                var data = MapperHelper.Map<QuitInformationEntity, TypeQuitInformation>(saveData);
                var result = _repository.SaveQuitInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("QuitInformationService - SaveQuitInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End QuitInformationService - SaveQuitInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("QuitInformationService - SaveQuitInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End QuitInformationService - SaveQuitInformation: " + DateTime.Now);
                return new Response<QuitInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<QuitInformationEntity>(false, e.Message, null);
            }
        }
    }
}
