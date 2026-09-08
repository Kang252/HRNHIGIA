using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;


namespace NHIGIA.Repository.Infrastructure
{
    public interface IProblemInformationRepository
    {
        ResponseList<ProblemInformationViewModel> GetAllProblemInformation(int id, int employeeId);
        Response<ProblemInformationEntity> SaveProblemInformation(TypeProblemInformation param, int isAction);
        ResponseList<ProblemInformationViewModel> GetAllProblem(PagingData param);
        Response<ProblemInformationEntity> GetProblemById(int id);
    }
}
