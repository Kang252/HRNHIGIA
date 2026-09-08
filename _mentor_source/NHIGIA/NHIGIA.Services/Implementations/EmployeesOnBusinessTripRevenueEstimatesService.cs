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
    public class EmployeesOnBusinessTripRevenueEstimatesService : IEmployeesOnBusinessTripRevenueEstimatesService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeesOnBusinessTripRevenueEstimatesRepository _repository = new EmployeesOnBusinessTripRevenueEstimatesRepository();

        public ResponseList<EmployeesOnBusinessTripRevenueEstimatesViewModel> GetAllEmployeesOnBusinessTripRevenueEstimates(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRevenueEstimatesService - GetAllEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            var result = _repository.GetAllEmployeesOnBusinessTripRevenueEstimates(id, employeesOnBusinessTripId);
            _logger.Info("EmployeesOnBusinessTripRevenueEstimatesService - GetAllEmployeesOnBusinessTripRevenueEstimates - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRevenueEstimatesService - GetAllEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripRevenueEstimatesViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EmployeesOnBusinessTripRevenueEstimatesEntity> SaveEmployeesOnBusinessTripRevenueEstimates(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripRevenueEstimatesService - SaveEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EmployeesOnBusinessTripRevenueEstimatesEntity>(content);
                var data = MapperHelper.Map<EmployeesOnBusinessTripRevenueEstimatesEntity, TypeEmployeesOnBusinessTripRevenueEstimates>(saveData);
                var result = _repository.SaveEmployeesOnBusinessTripRevenueEstimates(data, isAction);
                if (result != null)
                {
                    _logger.Info("EmployeesOnBusinessTripRevenueEstimatesService - SaveEmployeesOnBusinessTripRevenueEstimates - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeesOnBusinessTripRevenueEstimatesService - SaveEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EmployeesOnBusinessTripRevenueEstimatesService - SaveEmployeesOnBusinessTripRevenueEstimates - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeesOnBusinessTripRevenueEstimatesService - SaveEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
                return new Response<EmployeesOnBusinessTripRevenueEstimatesEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EmployeesOnBusinessTripRevenueEstimatesEntity>(false, e.Message, null);
            }
        }
    }
}
