using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace NHIGIA.Repository.Repositories
{
    public class EmployeesOnBusinessTripRevenueEstimatesRepository : BaseRepository<EmployeesOnBusinessTripRevenueEstimatesEntity>, IEmployeesOnBusinessTripRevenueEstimatesRepository
    {
        public ResponseList<EmployeesOnBusinessTripRevenueEstimatesViewModel> GetAllEmployeesOnBusinessTripRevenueEstimates(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRevenueEstimatesRepository - GetAllEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripRevenueEstimatesViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripRevenueEstimates,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeesOnBusinessTripId", employeesOnBusinessTripId, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripRevenueEstimatesRepository - GetAllEmployeesOnBusinessTripRevenueEstimates - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRevenueEstimatesRepository - GetAllEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            return result;
        }

        public Response<EmployeesOnBusinessTripRevenueEstimatesEntity> SaveEmployeesOnBusinessTripRevenueEstimates(TypeEmployeesOnBusinessTripRevenueEstimates param, int isAction)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRevenueEstimatesRepository - SaveEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            var result = GetByStoredProcedure<EmployeesOnBusinessTripRevenueEstimatesEntity>(Constants.StoredProc.spSaveEmployeesOnBusinessTripRevenueEstimates,
                new StoredProcedureParameter("TypeEmployeesOnBusinessTripRevenueEstimates", new List<TypeEmployeesOnBusinessTripRevenueEstimates> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripRevenueEstimatesRepository - SaveEmployeesOnBusinessTripRevenueEstimates - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRevenueEstimatesRepository - SaveEmployeesOnBusinessTripRevenueEstimates: " + DateTime.Now);
            return result;
        }
    }
}
