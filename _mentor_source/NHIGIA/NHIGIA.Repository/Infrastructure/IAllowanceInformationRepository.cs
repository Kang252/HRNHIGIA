using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IAllowanceInformationRepository
    {
        ResponseList<AllowanceInformationViewModel> GetAllAllowanceInformation(int id, int employeeId);
        Response<AllowanceInformationEntity> SaveAllowanceInformation(TypeAllowanceInformation param, int isAction);
    }
}
