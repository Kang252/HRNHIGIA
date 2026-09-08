using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeesOnBusinessTripAdvancesService
    {
        ResponseList<EmployeesOnBusinessTripAdvancesViewModel> GetAllEmployeesOnBusinessTripAdvances(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripAdvancesEntity> SaveEmployeesOnBusinessTripAdvances(string content, int isAction);
    }
}
