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
    public class SkillInformationService : ISkillInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SkillInformationRepository _repository = new SkillInformationRepository();

        public ResponseList<SkillInformationViewModel> GetAllSkillInformation(int id, int employeeId)
        {
            _logger.Trace("Start SkillInformationService - GetAllSkillInformation: " + DateTime.Now);
            var result = _repository.GetAllSkillInformation(id, employeeId);
            _logger.Info("SkillInformationService - GetAllSkillInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkillInformationService - GetAllSkillInformation: " + DateTime.Now);
            return new ResponseList<SkillInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<SkillInformationEntity> SaveSkillInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start SkillInformationService - SaveSkillInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<SkillInformationEntity>(content);
                var data = MapperHelper.Map<SkillInformationEntity, TypeSkillInformation>(saveData);
                var result = _repository.SaveSkillInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("SkillInformationService - SaveSkillInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End SkillInformationService - SaveSkillInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("SkillInformationService - SaveSkillInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End SkillInformationService - SaveSkillInformation: " + DateTime.Now);
                return new Response<SkillInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<SkillInformationEntity>(false, e.Message, null);
            }
        }
    }
}
