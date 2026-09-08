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
    public class ReceiveInformationService : IReceiveInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ReceiveInformationRepository _repository = new ReceiveInformationRepository();

        public ResponseList<ReceiveInformationViewModel> GetAllReceiveInformation(int id, int employeeId)
        {
            _logger.Trace("Start ReceiveInformationService - GetAllReceiveInformation: " + DateTime.Now);
            var result = _repository.GetAllReceiveInformation(id, employeeId);
            _logger.Info("ReceiveInformationService - GetAllReceiveInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ReceiveInformationService - GetAllReceiveInformation: " + DateTime.Now);
            return new ResponseList<ReceiveInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ReceiveInformationEntity> SaveReceiveInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ReceiveInformationService - SaveReceiveInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ReceiveInformationEntity>(content);
                var data = MapperHelper.Map<ReceiveInformationEntity, TypeReceiveInformation>(saveData);
                var result = _repository.SaveReceiveInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("ReceiveInformationService - SaveReceiveInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ReceiveInformationService - SaveReceiveInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ReceiveInformationService - SaveReceiveInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ReceiveInformationService - SaveReceiveInformation: " + DateTime.Now);
                return new Response<ReceiveInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ReceiveInformationEntity>(false, e.Message, null);
            }
        }
    }
}
