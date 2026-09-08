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
    public class WorkExperienceInformationService : IWorkExperienceInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly WorkExperienceInformationRepository _repository = new WorkExperienceInformationRepository();

        public ResponseList<WorkExperienceInformationViewModel> GetAllWorkExperienceInformation(int id, int employeeId)
        {
            _logger.Trace("Start WorkExperienceInformationService - GetAllWorkExperienceInformation: " + DateTime.Now);
            var result = _repository.GetAllWorkExperienceInformation(id, employeeId);
            _logger.Info("WorkExperienceInformationService - GetAllWorkExperienceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkExperienceInformationService - GetAllWorkExperienceInformation: " + DateTime.Now);
            return new ResponseList<WorkExperienceInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<WorkExperienceInformationEntity> SaveWorkExperienceInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start WorkExperienceInformationService - SaveWorkExperienceInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<WorkExperienceInformationEntity>(content);
                var data = MapperHelper.Map<WorkExperienceInformationEntity, TypeWorkExperienceInformation>(saveData);
                var result = _repository.SaveWorkExperienceInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("WorkExperienceInformationService - SaveWorkExperienceInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End WorkExperienceInformationService - SaveWorkExperienceInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("WorkExperienceInformationService - SaveWorkExperienceInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End WorkExperienceInformationService - SaveWorkExperienceInformation: " + DateTime.Now);
                return new Response<WorkExperienceInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<WorkExperienceInformationEntity>(false, e.Message, null);
            }
        }
    }
}