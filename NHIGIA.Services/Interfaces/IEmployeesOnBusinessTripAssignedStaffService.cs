using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeesOnBusinessTripAssignedStaffService
    {
        ResponseList<EmployeesOnBusinessTripAssignedStaffViewModel> GetAllEmployeesOnBusinessTripAssignedStaff(int employeesOnBusinessTripId);
        string SaveEmployeesOnBusinessTripAssignedStaff(string content, int isAction);
    }
}
