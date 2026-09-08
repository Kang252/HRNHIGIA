using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IResignationProceduresEmployeeDebtService
    {
        ResponseList<ResignationProceduresEmployeeDebtViewModel> GetAllResignationProceduresEmployeeDebt(int id, int resignationProceduresId);
        Response<ResignationProceduresEmployeeDebtEntity> SaveResignationProceduresEmployeeDebt(string content, int isAction);
    }
}
