using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IQuitInformationService
    {
        ResponseList<QuitInformationViewModel> GetAllQuitInformation(int id, int employeeId);
        Response<QuitInformationEntity> SaveQuitInformation(string content, int isAction);
    }
}
