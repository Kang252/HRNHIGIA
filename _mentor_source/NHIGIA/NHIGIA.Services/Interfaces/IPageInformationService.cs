using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IPageInformationService
    {
        ResponseList<PageInformationViewModel> GetAllPageInformation(int id, int employeeId);
        Response<PageInformationEntity> SavePageInformation(string content, int isAction);
    }
}
