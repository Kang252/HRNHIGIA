using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEvaluateRepository
    {
        ResponseList<EvaluateViewModel> GetAllEvaluate(PagingData param);
        Response<EvaluateEntity> SaveEvaluate(TypeEvaluate param, int isAction);
        Response<EvaluateViewModel> GetEvaluateById(int id);
        ResponseList<EvaluateDetailViewModel> GetAllEvaluateDetail(PagingData param, int evaluateId);
        Response<EvaluateDetailEntity> SaveEvaluateDetail(TypeEvaluateDetail param, int isAction);
        ResponseList<EvaluateDetailViewModel> GetEvaluateByEmployee(int id, int employeeId);
    }
}
