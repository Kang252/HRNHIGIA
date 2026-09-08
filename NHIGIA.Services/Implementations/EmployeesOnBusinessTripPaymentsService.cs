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
    public class EmployeesOnBusinessTripPaymentsService : IEmployeesOnBusinessTripPaymentsService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeesOnBusinessTripPaymentsRepository _repository = new EmployeesOnBusinessTripPaymentsRepository();

        public ResponseList<EmployeesOnBusinessTripPaymentsViewModel> GetAllEmployeesOnBusinessTripPayments(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripPaymentsService - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
            var result = _repository.GetAllEmployeesOnBusinessTripPayments(id, employeesOnBusinessTripId);
            _logger.Info("EmployeesOnBusinessTripPaymentsService - GetAllEmployeesOnBusinessTripPayments - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripPaymentsService - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripPaymentsViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<EmployeesOnBusinessTripPaymentsEntity> SaveEmployeesOnBusinessTripPayments(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripPaymentsService - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EmployeesOnBusinessTripPaymentsEntity>(content);
                var data = MapperHelper.Map<EmployeesOnBusinessTripPaymentsEntity, TypeEmployeesOnBusinessTripPayments>(saveData);
                var result = _repository.SaveEmployeesOnBusinessTripPayments(data, isAction);
                if (result != null)
                {
                    _logger.Info("EmployeesOnBusinessTripPaymentsService - SaveEmployeesOnBusinessTripPayments - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeesOnBusinessTripPaymentsService - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
                    return result;
                }
                _logger.Info("EmployeesOnBusinessTripPaymentsService - SaveEmployeesOnBusinessTripPayments - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End EmployeesOnBusinessTripPaymentsService - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
                return new Response<EmployeesOnBusinessTripPaymentsEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<EmployeesOnBusinessTripPaymentsEntity>(false, e.Message, null);
            }
        }
    }
}
