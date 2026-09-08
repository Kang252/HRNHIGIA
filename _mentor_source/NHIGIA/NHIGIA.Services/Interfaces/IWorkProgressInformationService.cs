using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IWorkProgressInformationService
    {
        ResponseList<WorkProgressInformationViewModel> GetAllWorkProgressInformation(int id, int employeeId);
        Response<WorkProgressInformationEntity> SaveWorkProgressInformation(string content, int isAction);
    }
}
