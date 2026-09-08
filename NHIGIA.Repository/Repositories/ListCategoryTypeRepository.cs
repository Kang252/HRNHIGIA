using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;

namespace NHIGIA.Repository.Repositories
{
    public class ListCategoryTypeRepository : BaseRepository<ListCategoryTypeEntity>, IListCategoryTypeRepository
    {
        public ResponseList<ListCategoryTypeEntity> GetListCategoryType()
        {
            _logger.Trace("Start ListCategoryTypeRepository - GetListCategoryType: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spGetListCategoryType);
            _logger.Info("ListCategoryTypeRepository - GetListCategoryType - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryTypeRepository - GetListCategoryType: " + DateTime.Now);
            return result;
        }
    }
}
