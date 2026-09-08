using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IResignationProceduresService
    {
        ResponseList<ResignationProceduresViewModel> GetResignationProcedures(PagingData param);
        Response<ResignationProceduresEntity> SaveResignationProcedures(string content, int isAction);
        Response<ResignationProceduresViewModel> GetGeneralInformationForResignationProcedures(int id);
    }
}
