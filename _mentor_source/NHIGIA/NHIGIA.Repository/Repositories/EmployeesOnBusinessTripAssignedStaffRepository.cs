using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;
using System.Data;

namespace NHIGIA.Repository.Repositories
{
    public class EmployeesOnBusinessTripAssignedStaffRepository : BaseRepository<EmployeesOnBusinessTripAssignedStaffEntity>, IEmployeesOnBusinessTripAssignedStaffRepository
    {
        public ResponseList<EmployeesOnBusinessTripAssignedStaffViewModel> GetAllEmployeesOnBusinessTripAssignedStaff(int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffRepository - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripAssignedStaffViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripAssignedStaff,
                new StoredProcedureParameter("EmployeesOnBusinessTripId", employeesOnBusinessTripId, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripAssignedStaffRepository - GetAllEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAssignedStaffRepository - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            return result;
        }

        public ResponseList<EmployeesOnBusinessTripAssignedStaffEntity> SaveEmployeesOnBusinessTripAssignedStaff(DataTable param, int isAction)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffRepository - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spSaveEmployeesOnBusinessTripAssignedStaff,
                new
                {
                    TypeEmployeesOnBusinessTripAssignedStaff = param,
                    IsAction = isAction
                });
            _logger.Info("EmployeesOnBusinessTripAssignedStaffRepository - SaveEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAssignedStaffRepository - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            return result;
        }
    }
}
