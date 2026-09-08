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
    public class EmployeesOnBusinessTripAdvancesService : IEmployeesOnBusinessTripAdvancesService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeesOnBusinessTripAdvancesRepository _repository = new EmployeesOnBusinessTripAdvancesRepository();

        public ResponseList<EmployeesOnBusinessTripAdvancesViewModel> GetAllEmployeesOnBusinessTripAdvances(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAdvancesService - GetAllEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            var result = _repository.GetAllEmployeesOnBusinessTripAdvances(id, employeesOnBusinessTripId);
            _logger.Info("EmployeesOnBusinessTripAdvancesService - GetAllEmployeesOnBusinessTripAdvances - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAdvancesService - GetAllEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripAdvancesViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EmployeesOnBusinessTripAdvancesEntity> SaveEmployeesOnBusinessTripAdvances(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripAdvancesService - SaveEmployeesOnBusinessTripAdvances: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EmployeesOnBusinessTripAdvancesEntity>(content);
                var data = MapperHelper.Map<EmployeesOnBusinessTripAdvancesEntity, TypeEmployeesOnBusinessTripAdvances>(saveData);
                var result = _repository.SaveEmployeesOnBusinessTripAdvances(data, isAction);
                if (result != null)
                {
                    _logger.Info("EmployeesOnBusinessTripAdvancesService - SaveEmployeesOnBusinessTripAdvances - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeesOnBusinessTripAdvancesService - SaveEmployeesOnBusinessTripAdvances: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EmployeesOnBusinessTripAdvancesService - SaveEmployeesOnBusinessTripAdvances - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeesOnBusinessTripAdvancesService - SaveEmployeesOnBusinessTripAdvances: " + DateTime.Now);
                return new Response<EmployeesOnBusinessTripAdvancesEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EmployeesOnBusinessTripAdvancesEntity>(false, e.Message, null);
            }
        }
    }
}
