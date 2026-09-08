using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IAssetInformationRepository
    {
        ResponseList<AssetInformationViewModel> GetAllAssetInformation(int id, int employeeId);
        Response<AssetInformationEntity> SaveAssetInformation(TypeAssetInformation param, int isAction);
    }
}
