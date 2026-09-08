using NHIGIA.Core.Domain.DtoEntities;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace NHIGIA.Services.Implementations
{
    public class BulkUploadService : IBulkUploadService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BulkUploadRepository _repository = new BulkUploadRepository();

        public Response ImportDataListCategory(RequestDto<ImportDataViewModel> param, int isAction)
        {
            _logger.Trace("Start ListCategoryService - ImportData: " + DateTime.Now);
            List<TypeListCategory> listData = MapperHelper.MapList<ListCategoryEntity, TypeListCategory>(param.Data.ListCategoryEntityUpload);
            var result = _repository.ImportDataListCategory(listData, isAction);
            _logger.Info("ListCategoryService - ImportData - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - ImportData: " + DateTime.Now);
            return result;
        }
    }
}
