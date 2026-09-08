using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IReceiveInformationService
    {
        ResponseList<ReceiveInformationViewModel> GetAllReceiveInformation(int id, int employeeId);
        Response<ReceiveInformationEntity> SaveReceiveInformation(string content, int isAction);
    }
}
