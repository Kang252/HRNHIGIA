using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using System.Data;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeesOnBusinessTripAssignedStaffRepository
    {
        ResponseList<EmployeesOnBusinessTripAssignedStaffViewModel> GetAllEmployeesOnBusinessTripAssignedStaff(int employeesOnBusinessTripId);
        ResponseList<EmployeesOnBusinessTripAssignedStaffEntity> SaveEmployeesOnBusinessTripAssignedStaff(DataTable param, int isAction);
    }
}
