using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEvaluateService
    {
        ResponseList<EvaluateViewModel> GetAllEvaluate(PagingData param);
        Response<EvaluateEntity> SaveEvaluate(string content, int isAction);
        Response<EvaluateViewModel> GetEvaluateById(int id);
        ResponseList<EvaluateDetailViewModel> GetAllEvaluateDetail(PagingData param, int evaluateId);
        Response<EvaluateDetailEntity> SaveEvaluateDetail(string content, int isAction);
        ResponseList<EvaluateDetailViewModel> GetEvaluateByEmployee(int id, int employeeId);
    }
}
