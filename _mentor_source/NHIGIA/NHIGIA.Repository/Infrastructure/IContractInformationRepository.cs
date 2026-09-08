using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;


namespace NHIGIA.Repository.Infrastructure
{
    public interface IContractInformationRepository
    {
        ResponseList<ContractInformationViewModel> GetAllContractInformation(int id, int employeeId);
        Response<ContractInformationEntity> SaveContractInformation(TypeContractInformation param, int isAction);
    }
}
