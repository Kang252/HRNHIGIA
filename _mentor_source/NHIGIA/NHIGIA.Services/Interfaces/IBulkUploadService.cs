using NHIGIA.Core.Domain.DtoEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IBulkUploadService
    {
        //string ImportDataListCategory(string content, int isAction);
        Response ImportDataListCategory(RequestDto<ImportDataViewModel> param, int isAction);
    }
}
