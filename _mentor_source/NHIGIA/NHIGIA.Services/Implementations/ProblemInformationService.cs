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
    public class ProblemInformationService : IProblemInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ProblemInformationRepository _repository = new ProblemInformationRepository();

        public ResponseList<ProblemInformationViewModel> GetAllProblem(PagingData param)
        {
            var result = _repository.GetAllProblem(param);
            _logger.Info("ProblemInformationService - GetAllProblem - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationService - GetAllProblem: " + DateTime.Now);
            return new ResponseList<ProblemInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<ProblemInformationViewModel> GetAllProblemInformation(int id, int employeeId)
        {
            _logger.Trace("Start ProblemInformationService - GetAllProblemInformation: " + DateTime.Now);
            var result = _repository.GetAllProblemInformation(id, employeeId);
            _logger.Info("ProblemInformationService - GetAllProblemInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationService - GetAllProblemInformation: " + DateTime.Now);
            return new ResponseList<ProblemInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ProblemInformationEntity> GetProblemById(int id)
        {
            _logger.Trace("Start ProblemInformationService - GetProblemById: " + DateTime.Now);
            var result = _repository.GetProblemById(id);
            _logger.Info("ProblemInformationService - GetProblemById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationService - GetProblemById: " + DateTime.Now);
            return result;
        }

        public Response<ProblemInformationEntity> SaveProblemInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ProblemInformationService - SaveProblemInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ProblemInformationEntity>(content);
                var data = MapperHelper.Map<ProblemInformationEntity, TypeProblemInformation>(saveData);
                var result = _repository.SaveProblemInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("ProblemInformationService - SaveProblemInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ProblemInformationService - SaveProblemInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ProblemInformationService - SaveProblemInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ProblemInformationService - SaveProblemInformation: " + DateTime.Now);
                return new Response<ProblemInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ProblemInformationEntity>(false, e.Message, null);
            }
        }
    }
}
