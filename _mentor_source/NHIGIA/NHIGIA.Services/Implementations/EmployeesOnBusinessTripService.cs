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
    public class EmployeesOnBusinessTripService : IEmployeesOnBusinessTripService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeesOnBusinessTripRepository _repository = new EmployeesOnBusinessTripRepository();

        public ResponseList<EmployeesOnBusinessTripViewModel> GetAllEmployeesOnBusinessTrip(PagingData param)
        {
            _logger.Trace("Start EmployeesOnBusinessTripService - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
            var result = _repository.GetAllEmployeesOnBusinessTrip(param);
            _logger.Info("EmployeesOnBusinessTripService - GetAllEmployeesOnBusinessTrip - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripService - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripById(int id)
        {
            _logger.Trace("Start EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripById: " + DateTime.Now);
            var result = _repository.GetEmployeesOnBusinessTripById(id);
            _logger.Info("EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripById: " + DateTime.Now);
            return result;
        }

        public ResponseList<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripInformation(int employeeId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
            var result = _repository.GetEmployeesOnBusinessTripInformation(employeeId);
            _logger.Info("EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripService - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EmployeesOnBusinessTripEntity> SaveEmployeesOnBusinessTrip(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripService - EmployeesOnBusinessTripEntity: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EmployeesOnBusinessTripEntity>(content);
                var data = MapperHelper.Map<EmployeesOnBusinessTripEntity, TypeEmployeesOnBusinessTrip>(saveData);
                var result = _repository.SaveEmployeesOnBusinessTrip(data, isAction);
                if (result != null)
                {
                    _logger.Info("EmployeesOnBusinessTripService - EmployeesOnBusinessTripEntity - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeesOnBusinessTripService - EmployeesOnBusinessTripEntity: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EmployeesOnBusinessTripService - EmployeesOnBusinessTripEntity - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeesOnBusinessTripService - EmployeesOnBusinessTripEntity: " + DateTime.Now);
                return new Response<EmployeesOnBusinessTripEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EmployeesOnBusinessTripEntity>(false, e.Message, null);
            }
        }
    }
}
