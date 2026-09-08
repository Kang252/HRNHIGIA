using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Helper;
using System.Collections.Generic;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IBulkUploadRepository
    {
        //ResponseList<ListCategoryEntity> ImportDataListCategory(DataTable param, int isAction);
        Response ImportDataListCategory(List<TypeListCategory> param, int isAction);
    }
}
