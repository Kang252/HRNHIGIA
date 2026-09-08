using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IProblemInformationRelatedStaffService
    {
        ResponseList<ProblemInformationRelatedStaffViewModel> GetAllProblemInformationRelatedStaff(int id, int problemInformationId);
        string SaveProblemInformationRelatedStaff(string content, int isAction);
        Response<ProblemInformationRelatedStaffEntity> SaveOnlyProblemInformationRelatedStaff(string content, int isAction);
    }
}
