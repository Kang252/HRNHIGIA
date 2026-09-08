using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IProblemInformationTrackEmployeeCompensationService
    {
        ResponseList<ProblemInformationTrackEmployeeCompensationViewModel> GetAllProblemInformationTrackEmployeeCompensation(int problemInformationId, int employeeId, int type);
        string SaveProblemInformationTrackEmployeeCompensation(string content, int isAction);
    }
}
