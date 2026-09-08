using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeesOnBusinessTripRepository
    {
        ResponseList<EmployeesOnBusinessTripViewModel> GetAllEmployeesOnBusinessTrip(PagingData param);
        Response<EmployeesOnBusinessTripEntity> SaveEmployeesOnBusinessTrip(TypeEmployeesOnBusinessTrip param, int isAction);
        Response<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripById(int id);
        ResponseList<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripInformation(int employeeId);
    }
}
