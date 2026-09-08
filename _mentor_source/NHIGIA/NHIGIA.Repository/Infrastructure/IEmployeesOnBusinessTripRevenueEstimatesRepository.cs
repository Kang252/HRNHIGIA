using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeesOnBusinessTripRevenueEstimatesRepository
    {
        ResponseList<EmployeesOnBusinessTripRevenueEstimatesViewModel> GetAllEmployeesOnBusinessTripRevenueEstimates(int id, int employeesOnBusinessTripId);
        Response<EmployeesOnBusinessTripRevenueEstimatesEntity> SaveEmployeesOnBusinessTripRevenueEstimates(TypeEmployeesOnBusinessTripRevenueEstimates param, int isAction);
    }
}
