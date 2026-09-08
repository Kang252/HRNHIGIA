using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IWorkExperienceInformationRepository
    {
        ResponseList<WorkExperienceInformationViewModel> GetAllWorkExperienceInformation(int id, int employeeId);
        Response<WorkExperienceInformationEntity> SaveWorkExperienceInformation(TypeWorkExperienceInformation param, int isAction);
    }
}
