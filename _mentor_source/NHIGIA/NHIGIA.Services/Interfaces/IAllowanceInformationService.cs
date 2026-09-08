using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IAllowanceInformationService
    {
        ResponseList<AllowanceInformationViewModel> GetAllAllowanceInformation(int id, int employeeId);
        Response<AllowanceInformationEntity> SaveAllowanceInformation(string content, int isAction);
    }
}
