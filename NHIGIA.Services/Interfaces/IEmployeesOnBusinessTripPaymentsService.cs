using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeesOnBusinessTripPaymentsService
    {
        ResponseList<EmployeesOnBusinessTripPaymentsViewModel> GetAllEmployeesOnBusinessTripPayments(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripPaymentsEntity> SaveEmployeesOnBusinessTripPayments(string content, int isAction);
    }
}
