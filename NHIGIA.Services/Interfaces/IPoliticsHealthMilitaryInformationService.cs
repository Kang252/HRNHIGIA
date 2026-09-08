using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IPoliticsHealthMilitaryInformationService
    {
        ResponseList<PoliticsHealthMilitaryInformationViewModel> GetAllPoliticsHealthMilitaryInformation(int employeeId);
        Response<PoliticsHealthMilitaryInformationEntity> SavePoliticsHealthMilitaryInformation(string content);
    }
}
