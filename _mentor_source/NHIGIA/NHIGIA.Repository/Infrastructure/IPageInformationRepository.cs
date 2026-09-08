using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IPageInformationRepository
    {
        ResponseList<PageInformationViewModel> GetAllPageInformation(int id, int employeeId);
        Response<PageInformationEntity> SavePageInformation(TypePageInformation param, int isAction);
    }
}
