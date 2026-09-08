using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IFamilyInformationService
    {
        ResponseList<FamilyInformationViewModel> GetAllFamilyInformation(int id, int employeeId);
        Response<FamilyInformationEntity> SaveFamilyInformation(string content, int isAction);
    }
}
