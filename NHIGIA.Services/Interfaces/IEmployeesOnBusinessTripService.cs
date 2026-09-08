using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeesOnBusinessTripService
    {
        ResponseList<EmployeesOnBusinessTripViewModel> GetAllEmployeesOnBusinessTrip(PagingData param);
        Response<EmployeesOnBusinessTripEntity> SaveEmployeesOnBusinessTrip(string content, int isAction);
        Response<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripById(int id);
        ResponseList<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripInformation(int employeeId);
    }
}
