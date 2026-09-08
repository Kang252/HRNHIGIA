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
    public class EmployeesOnBusinessTripPaymentsRepository : BaseRepository<EmployeesOnBusinessTripPaymentsEntity>, IEmployeesOnBusinessTripPaymentsRepository
    {
        public ResponseList<EmployeesOnBusinessTripPaymentsViewModel> GetAllEmployeesOnBusinessTripPayments(int id, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripPaymentsRepository - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripPaymentsViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripPayments,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeesOnBusinessTripId", employeesOnBusinessTripId, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripPaymentsRepository - GetAllEmployeesOnBusinessTripPayments - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripPaymentsRepository - GetAllEmployeesOnBusinessTripPayments: " + DateTime.Now);
            return result;
        }

        public Response<EmployeesOnBusinessTripPaymentsEntity> SaveEmployeesOnBusinessTripPayments(TypeEmployeesOnBusinessTripPayments param, int isAction)
        {
            _logger.Trace("Start EmployeesOnBusinessTripPaymentsRepository - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
            var result = GetByStoredProcedure<EmployeesOnBusinessTripPaymentsEntity>(Constants.StoredProc.spSaveEmployeesOnBusinessTripPayments,
                new StoredProcedureParameter("TypeEmployeesOnBusinessTripPayments", new List<TypeEmployeesOnBusinessTripPayments> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripPaymentsRepository - SaveEmployeesOnBusinessTripPayments - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripPaymentsRepository - SaveEmployeesOnBusinessTripPayments: " + DateTime.Now);
            return result;
        }
    }
}
