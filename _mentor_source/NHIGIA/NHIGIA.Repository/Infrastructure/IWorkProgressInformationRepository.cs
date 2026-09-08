using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IWorkProgressInformationRepository
    {
        ResponseList<WorkProgressInformationViewModel> GetAllWorkProgressInformation(int id, int employeeId);
        Response<WorkProgressInformationEntity> SaveWorkProgressInformation(TypeWorkProgressInformation param, int isAction);
    }
}
