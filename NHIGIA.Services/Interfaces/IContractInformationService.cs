using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IContractInformationService
    {
        ResponseList<ContractInformationViewModel> GetAllContractInformation(int id, int employeeId);
        Response<ContractInformationEntity> SaveContractInformation(string content, int isAction);
    }
}
