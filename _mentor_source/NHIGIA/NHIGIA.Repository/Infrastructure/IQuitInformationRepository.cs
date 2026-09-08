using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IQuitInformationRepository
    {
        ResponseList<QuitInformationViewModel> GetAllQuitInformation(int id, int employeeId);
        Response<QuitInformationEntity> SaveQuitInformation(TypeQuitInformation param, int isAction);
    }
}
