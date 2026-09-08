using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IListCategoryTypeService
    {
        ResponseList<ListCategoryTypeEntity> GetListCategoryType();
    }
}
