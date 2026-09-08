using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using System.Data;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IBonusInformationStaffWereCommendedRepository
    {
        ResponseList<BonusInformationStaffWereCommendedViewModel> GetAllBonusInformationStaffWereCommended(int bonusInformationId);
        ResponseList<BonusInformationStaffWereCommendedEntity> SaveBonusInformationStaffWereCommended(DataTable param, int isAction);
        Response<BonusInformationStaffWereCommendedEntity> SaveOnlyBonusInformationStaffWereCommended(TypeBonusInformationStaffWereCommended param, int isAction);
    }
}
