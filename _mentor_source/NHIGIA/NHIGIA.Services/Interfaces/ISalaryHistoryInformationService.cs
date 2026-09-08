using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface ISalaryHistoryInformationService
    {
        ResponseList<SalaryHistoryInformationViewModel> GetAllSalaryHistoryInformation(int id, int employeeId);
        Response<SalaryHistoryInformationEntity> SaveSalaryHistoryInformation(string content, int isAction);
    }
}
