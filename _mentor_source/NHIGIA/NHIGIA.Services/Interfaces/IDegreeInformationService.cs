using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IDegreeInformationService
    {
        ResponseList<DegreeInformationViewModel> GetAllDegreeInformation(int id, int employeeId);
        Response<DegreeInformationEntity> SaveDegreeInformation(string content, int isAction);
    }
}
