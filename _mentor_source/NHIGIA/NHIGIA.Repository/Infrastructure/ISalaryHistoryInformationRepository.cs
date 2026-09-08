using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface ISalaryHistoryInformationRepository
    {
        ResponseList<SalaryHistoryInformationViewModel> GetAllSalaryHistoryInformation(int id, int employeeId);
        Response<SalaryHistoryInformationEntity> SaveSalaryHistoryInformation(TypeSalaryHistoryInformation param, int isAction);
    }
}
