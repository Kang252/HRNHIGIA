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
    public class ResignationProceduresService : IResignationProceduresService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ResignationProceduresRepository _repository = new ResignationProceduresRepository();

        public Response<ResignationProceduresViewModel> GetGeneralInformationForResignationProcedures(int id)
        {
            _logger.Trace("Start ResignationProceduresService - GetGeneralInformationForResignationProcedures: " + DateTime.Now);
            var result = _repository.GetGeneralInformationForResignationProcedures(id);
            _logger.Info("ResignationProceduresService - GetGeneralInformationForResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresService - GetGeneralInformationForResignationProcedures: " + DateTime.Now);
            return result;
        }

        public ResponseList<ResignationProceduresViewModel> GetResignationProcedures(PagingData param)
        {
            _logger.Trace("Start ResignationProceduresService - GetResignationProcedures: " + DateTime.Now);
            var result = _repository.GetResignationProcedures(param);
            _logger.Info("ResignationProceduresService - GetResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresService - GetResignationProcedures: " + DateTime.Now);
            return new ResponseList<ResignationProceduresViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ResignationProceduresEntity> SaveResignationProcedures(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresService - SaveResignationProcedures: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ResignationProceduresEntity>(content);
                var data = MapperHelper.Map<ResignationProceduresEntity, TypeResignationProcedures>(saveData);
                var result = _repository.SaveResignationProcedures(data, isAction);
                if (result != null)
                {
                    _logger.Info("ResignationProceduresService - SaveResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ResignationProceduresService - SaveResignationProcedures: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ResignationProceduresService - SaveResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ResignationProceduresService - SaveResignationProcedures: " + DateTime.Now);
                return new Response<ResignationProceduresEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ResignationProceduresEntity>(false, e.Message, null);
            }
        }
    }
}
