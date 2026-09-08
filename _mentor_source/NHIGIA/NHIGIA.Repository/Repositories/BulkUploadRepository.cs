using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace NHIGIA.Repository.Repositories
{
    public class BulkUploadRepository : BaseRepository<ListCategoryEntity>, IBulkUploadRepository
    {
        public Response ImportDataListCategory(List<TypeListCategory> param, int isAction)
        {
            _logger.Trace("Start ListCategoryRepository - ImportData: " + DateTime.Now);
            var result = CallStoredProcedure(Constants.StoredProc.spSaveListCategory,
                new StoredProcedureParameter("TypeListCategory", param.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ListCategoryRepository - ImportData - Data: " + JsonConvert.SerializeObject(param));
            _logger.Trace("End ListCategoryRepository - ImportData: " + DateTime.Now);
            return result;
        }
    }
}
