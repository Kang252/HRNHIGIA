using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IBonusInformationRepository
    {
        ResponseList<BonusInformationViewModel> GetAllBonus(PagingData param);
        ResponseList<BonusInformationViewModel> GetAllBonusInformation(int employeeId);
        Response<BonusInformationEntity> SaveBonusInformation(TypeBonusInformation param, int isAction);
        Response<BonusInformationViewModel> GetBonusById(int id);
    }
}
