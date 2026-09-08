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
    public class EmployeesOnBusinessTripAdvancesRepository : BaseRepository<EmployeesOnBusinessTripAdvancesEntity>, IEmployeesOnBusinessTripAdvancesRepository
    {
        public ResponseList<EmployeesOnBusinessTripAdvancesViewModel> GetAllEmployeesOnBusinessTripAdvances(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAdvancesRepository - GetAllEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripAdvancesViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripAdvances,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeesOnBusinessTripId", employeesOnBusinessTripId, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripAdvancesRepository - GetAllEmployeesOnBusinessTripAdvances - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAdvancesRepository - GetAllEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            return result;
        }

        public Response<EmployeesOnBusinessTripAdvancesEntity> SaveEmployeesOnBusinessTripAdvances(TypeEmployeesOnBusinessTripAdvances param, int isAction)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAdvancesRepository - SaveEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            var result = GetByStoredProcedure<EmployeesOnBusinessTripAdvancesEntity>(Constants.StoredProc.spSaveEmployeesOnBusinessTripAdvances,
                new StoredProcedureParameter("TypeEmployeesOnBusinessTripAdvances", new List<TypeEmployeesOnBusinessTripAdvances> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripAdvancesRepository - SaveEmployeesOnBusinessTripAdvances - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAdvancesRepository - SaveEmployeesOnBusinessTripAdvances: " + DateTime.Now);
            return result;
        }
    }
}
