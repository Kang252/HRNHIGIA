using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeesOnBusinessTripPaymentsRepository
    {
        ResponseList<EmployeesOnBusinessTripPaymentsViewModel> GetAllEmployeesOnBusinessTripPayments(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripPaymentsEntity> SaveEmployeesOnBusinessTripPayments(TypeEmployeesOnBusinessTripPayments param, int isAction);
    }
}
