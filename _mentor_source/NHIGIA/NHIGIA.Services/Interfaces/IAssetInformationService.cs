using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IAssetInformationService
    {
        ResponseList<AssetInformationViewModel> GetAllAssetInformation(int id, int employeeId);
        Response<AssetInformationEntity> SaveAssetInformation(string content, int isAction);
    }
}
