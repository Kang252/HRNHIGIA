using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IResignationProceduresEmployeeDebtRepository
    {
        ResponseList<ResignationProceduresEmployeeDebtViewModel> GetAllResignationProceduresEmployeeDebt(int id, int resignationProceduresId);
        Response<ResignationProceduresEmployeeDebtEntity> SaveResignationProceduresEmployeeDebt(TypeResignationProceduresEmployeeDebt param, int isAction);
    }
}
