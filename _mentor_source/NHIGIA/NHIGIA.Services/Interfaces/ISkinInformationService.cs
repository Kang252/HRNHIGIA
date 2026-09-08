using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface ISkinInformationService
    {
        ResponseList<SkinInformationEntity> GetAllSkinInformation(int employeeId);
        Response<SkinInformationEntity> SaveSkinInformation(string content);
    }
}
