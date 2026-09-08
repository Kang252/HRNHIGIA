using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IProblemInformationService
    {
        ResponseList<ProblemInformationViewModel> GetAllProblemInformation(int id, int employeeId);
        Response<ProblemInformationEntity> SaveProblemInformation(string content, int isAction);
        ResponseList<ProblemInformationViewModel> GetAllProblem(PagingData param);
        Response<ProblemInformationEntity> GetProblemById(int id);
    }
}
