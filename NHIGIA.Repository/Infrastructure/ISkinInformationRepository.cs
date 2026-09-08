using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface ISkinInformationRepository
    {
        ResponseList<SkinInformationEntity> GetAllSkinInformation(int employeeId);
        Response<SkinInformationEntity> SaveSkinInformation(TypeSkinInformation param);
    }
}
