using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface ISkillInformationRepository
    {
        ResponseList<SkillInformationViewModel> GetAllSkillInformation(int id, int employeeId);
        Response<SkillInformationEntity> SaveSkillInformation(TypeSkillInformation param, int isAction);
    }
}
