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
    public class EvaluateService : IEvaluateService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EvaluateRepository _repository = new EvaluateRepository();

        public ResponseList<EvaluateViewModel> GetAllEvaluate(PagingData param)
        {
            _logger.Trace("Start EvaluateService - GetAllEvaluate: " + DateTime.Now);
            var result = _repository.GetAllEvaluate(param);
            _logger.Info("EvaluateService - GetAllEvaluate - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateService - GetAllEvaluate: " + DateTime.Now);
            return new ResponseList<EvaluateViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<EvaluateDetailViewModel> GetAllEvaluateDetail(PagingData param, int evaluateId)
        {
            _logger.Trace("Start EvaluateService - GetAllEvaluateDetail: " + DateTime.Now);
            var result = _repository.GetAllEvaluateDetail(param, evaluateId);
            _logger.Info("EvaluateService - GetAllEvaluateDetail - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateService - GetAllEvaluateDetail: " + DateTime.Now);
            return new ResponseList<EvaluateDetailViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<EvaluateDetailViewModel> GetEvaluateByEmployee(int id, int employeeId)
        {
            _logger.Trace("Start EvaluateService - GetEvaluateByEmployee: " + DateTime.Now);
            var result = _repository.GetEvaluateByEmployee(id, employeeId);
            _logger.Info("EvaluateService - GetEvaluateByEmployee - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateService - GetEvaluateByEmployee: " + DateTime.Now);
            return new ResponseList<EvaluateDetailViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EvaluateViewModel> GetEvaluateById(int id)
        {
            _logger.Trace("Start EvaluateService - GetEvaluateById: " + DateTime.Now);
            var result = _repository.GetEvaluateById(id);
            _logger.Info("EvaluateService - GetEvaluateById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateService - GetEvaluateById: " + DateTime.Now);
            return result;
        }

        public Response<EvaluateEntity> SaveEvaluate(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EvaluateService - SaveEvaluate: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EvaluateEntity>(content);
                var data = MapperHelper.Map<EvaluateEntity, TypeEvaluate>(saveData);
                var result = _repository.SaveEvaluate(data, isAction);
                if (result != null)
                {
                    _logger.Info("EvaluateService - SaveEvaluate - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EvaluateService - SaveEvaluate: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EvaluateService - SaveEvaluate - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EvaluateService - SaveEvaluate: " + DateTime.Now);
                return new Response<EvaluateEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EvaluateEntity>(false, e.Message, null);
            }
        }

        public Response<EvaluateDetailEntity> SaveEvaluateDetail(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EvaluateService - SaveEvaluateDetail: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EvaluateDetailEntity>(content);
                var data = MapperHelper.Map<EvaluateDetailEntity, TypeEvaluateDetail>(saveData);
                var result = _repository.SaveEvaluateDetail(data, isAction);
                if (result != null)
                {
                    _logger.Info("EvaluateService - SaveEvaluateDetail - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EvaluateService - SaveEvaluateDetail: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EvaluateService - SaveEvaluateDetail - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EvaluateService - SaveEvaluateDetail: " + DateTime.Now);
                return new Response<EvaluateDetailEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EvaluateDetailEntity>(false, e.Message, null);
            }
        }
    }
}
