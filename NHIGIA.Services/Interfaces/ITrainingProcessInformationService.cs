using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface ITrainingProcessInformationService
    {
        ResponseList<TrainingProcessInformationViewModel> GetAllTrainingProcessInformation(int id, int employeeId);
        Response<TrainingProcessInformationEntity> SaveTrainingProcessInformation(string content, int isAction);
    }
}
