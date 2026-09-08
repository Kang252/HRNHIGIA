using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface ITrainingProcessInformationRepository
    {
        ResponseList<TrainingProcessInformationViewModel> GetAllTrainingProcessInformation(int id, int employeeId);
        Response<TrainingProcessInformationEntity> SaveTrainingProcessInformation(TypeTrainingProcessInformation param, int isAction);
    }
}
