using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IListCategoryTypeRepository
    {
        ResponseList<ListCategoryTypeEntity> GetListCategoryType();
    }
}
