using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface ISkillInformationService
    {
        ResponseList<SkillInformationViewModel> GetAllSkillInformation(int id, int employeeId);
        Response<SkillInformationEntity> SaveSkillInformation(string content, int isAction);
    }
}
