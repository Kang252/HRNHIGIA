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
    public class AllowanceInformationService : IAllowanceInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly AllowanceInformationRepository _repository = new AllowanceInformationRepository();

        public ResponseList<AllowanceInformationViewModel> GetAllAllowanceInformation(int id, int employeeId)
        {
            _logger.Trace("Start AllowanceInformationService - GetAllAllowanceInformation: " + DateTime.Now);
            var result = _repository.GetAllAllowanceInformation(id, employeeId);
            _logger.Info("AllowanceInformationService - GetAllAllowanceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AllowanceInformationService - GetAllAllowanceInformation: " + DateTime.Now);
            return new ResponseList<AllowanceInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<AllowanceInformationEntity> SaveAllowanceInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start AllowanceInformationService - SaveAllowanceInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<AllowanceInformationEntity>(content);
                var data = MapperHelper.Map<AllowanceInformationEntity, TypeAllowanceInformation>(saveData);
                var result = _repository.SaveAllowanceInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("AllowanceInformationService - SaveAllowanceInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End AllowanceInformationService - SaveAllowanceInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("AllowanceInformationService - SaveAllowanceInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End AllowanceInformationService - SaveAllowanceInformation: " + DateTime.Now);
                return new Response<AllowanceInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<AllowanceInformationEntity>(false, e.Message, null);
            }
        }
    }
}
