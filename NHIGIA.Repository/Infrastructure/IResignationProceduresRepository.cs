using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IResignationProceduresRepository
    {
        ResponseList<ResignationProceduresViewModel> GetResignationProcedures(PagingData param);
        Response<ResignationProceduresEntity> SaveResignationProcedures(TypeResignationProcedures param, int isAction);
        Response<ResignationProceduresViewModel> GetGeneralInformationForResignationProcedures(int id);
    }
}
