using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeesOnBusinessTripAdvancesRepository
    {
        ResponseList<EmployeesOnBusinessTripAdvancesViewModel> GetAllEmployeesOnBusinessTripAdvances(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripAdvancesEntity> SaveEmployeesOnBusinessTripAdvances(TypeEmployeesOnBusinessTripAdvances param, int isAction);
    }
}
