using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeesOnBusinessTripRevenueEstimatesService
    {
        ResponseList<EmployeesOnBusinessTripRevenueEstimatesViewModel> GetAllEmployeesOnBusinessTripRevenueEstimates(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripRevenueEstimatesEntity> SaveEmployeesOnBusinessTripRevenueEstimates(string content, int isAction);
    }
}
