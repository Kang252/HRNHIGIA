using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using System.Data;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IProblemInformationRelatedStaffRepository
    {
        ResponseList<ProblemInformationRelatedStaffViewModel> GetAllProblemInformationRelatedStaff(int id, int problemInformationId);
        ResponseList<ProblemInformationRelatedStaffEntity> SaveProblemInformationRelatedStaff(DataTable param, int isAction);
        Response<ProblemInformationRelatedStaffEntity> SaveOnlyProblemInformationRelatedStaff(TypeProblemInformationRelatedStaff param, int isAction);
    }
}
