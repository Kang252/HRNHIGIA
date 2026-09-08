using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;


namespace NHIGIA.Repository.Infrastructure
{
    public interface IReceiveInformationRepository
    {
        ResponseList<ReceiveInformationViewModel> GetAllReceiveInformation(int id, int employeeId);
        Response<ReceiveInformationEntity> SaveReceiveInformation(TypeReceiveInformation param, int isAction);
    }
}
