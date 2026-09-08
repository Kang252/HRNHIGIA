using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IBonusInformationStaffWereCommendedService
    {
        ResponseList<BonusInformationStaffWereCommendedViewModel> GetAllBonusInformationStaffWereCommended(int bonusInformationId);
        string SaveBonusInformationStaffWereCommended(string content, int isAction);
        Response<BonusInformationStaffWereCommendedEntity> SaveOnlyBonusInformationStaffWereCommended(string content, int isAction);
    }
}
