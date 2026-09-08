using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IWorkExperienceInformationService
    {
        ResponseList<WorkExperienceInformationViewModel> GetAllWorkExperienceInformation(int id, int employeeId);
        Response<WorkExperienceInformationEntity> SaveWorkExperienceInformation(string content, int isAction);
    }
}
