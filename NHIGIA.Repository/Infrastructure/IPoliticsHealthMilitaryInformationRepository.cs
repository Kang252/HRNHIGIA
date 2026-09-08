using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IPoliticsHealthMilitaryInformationRepository
    {
        ResponseList<PoliticsHealthMilitaryInformationViewModel> GetAllPoliticsHealthMilitaryInformation(int employeeId);
        Response<PoliticsHealthMilitaryInformationEntity> SavePoliticsHealthMilitaryInformation(TypePoliticsHealthMilitaryInformation param);
    }
}
