using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IFamilyInformationRepository
    {
        ResponseList<FamilyInformationViewModel> GetAllFamilyInformation(int id, int employeeId);
        Response<FamilyInformationEntity> SaveFamilyInformation(TypeFamilyInformation param, int isAction);
    }
}
