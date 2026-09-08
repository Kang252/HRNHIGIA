using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IDegreeInformationRepository
    {
        ResponseList<DegreeInformationViewModel> GetAllDegreeInformation(int id, int employeeId);
        Response<DegreeInformationEntity> SaveDegreeInformation(TypeDegreeInformation param, int isAction);
    }
}
