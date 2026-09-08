using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using System.Data;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IProblemInformationTrackEmployeeCompensationRepository
    {
        ResponseList<ProblemInformationTrackEmployeeCompensationViewModel> GetAllProblemInformationTrackEmployeeCompensation(int problemInformationId, int employeeId, int type);
        ResponseList<ProblemInformationTrackEmployeeCompensationEntity> SaveProblemInformationTrackEmployeeCompensation(DataTable param, int isAction);
    }
}
