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
    public class ResignationProceduresEmployeeDebtService : IResignationProceduresEmployeeDebtService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ResignationProceduresEmployeeDebtRepository _repository = new ResignationProceduresEmployeeDebtRepository();

        public ResponseList<ResignationProceduresEmployeeDebtViewModel> GetAllResignationProceduresEmployeeDebt(int id, int resignationProceduresId)
        {
            _logger.Trace("Start ResignationProceduresEmployeeDebtService - GetAllResignationProceduresEmployeeDebt: " + DateTime.Now);
            var result = _repository.GetAllResignationProceduresEmployeeDebt(id, resignationProceduresId);
            _logger.Info("ResignationProceduresEmployeeDebtService - GetAllResignationProceduresEmployeeDebt - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresEmployeeDebtService - GetAllResignationProceduresEmployeeDebt: " + DateTime.Now);
            return new ResponseList<ResignationProceduresEmployeeDebtViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ResignationProceduresEmployeeDebtEntity> SaveResignationProceduresEmployeeDebt(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ResignationProceduresEmployeeDebtService - SaveResignationProceduresEmployeeDebt: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ResignationProceduresEmployeeDebtEntity>(content);
                var data = MapperHelper.Map<ResignationProceduresEmployeeDebtEntity, TypeResignationProceduresEmployeeDebt>(saveData);
                var result = _repository.SaveResignationProceduresEmployeeDebt(data, isAction);
                if (result != null)
                {
                    _logger.Info("ResignationProceduresEmployeeDebtService - SaveResignationProceduresEmployeeDebt - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ResignationProceduresEmployeeDebtService - SaveResignationProceduresEmployeeDebt: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ResignationProceduresEmployeeDebtService - SaveResignationProceduresEmployeeDebt - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ResignationProceduresEmployeeDebtService - SaveResignationProceduresEmployeeDebt: " + DateTime.Now);
                return new Response<ResignationProceduresEmployeeDebtEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ResignationProceduresEmployeeDebtEntity>(false, e.Message, null);
            }
        }
    }
}
