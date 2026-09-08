using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IBonusInformationService
    {
        ResponseList<BonusInformationViewModel> GetAllBonus(PagingData param);
        ResponseList<BonusInformationViewModel> GetAllBonusInformation(int employeeId);
        Response<BonusInformationEntity> SaveBonusInformation(string content, int isAction);
        Response<BonusInformationViewModel> GetBonusById(int id);
    }
}
