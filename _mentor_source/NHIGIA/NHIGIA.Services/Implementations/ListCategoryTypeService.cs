using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;

namespace NHIGIA.Services.Implementations
{
    public class ListCategoryTypeService : IListCategoryTypeService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ListCategoryTypeRepository _repository = new ListCategoryTypeRepository();

        public ResponseList<ListCategoryTypeEntity> GetListCategoryType()
        {
            _logger.Trace("Start ListCategoryTypeService - GetListCategoryType: " + DateTime.Now);
            var result = _repository.GetListCategoryType();
            _logger.Info("ListCategoryTypeService - GetListCategoryType - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryTypeService - GetListCategoryType: " + DateTime.Now);
            return new ResponseList<ListCategoryTypeEntity>(result.Success, result.Message, result.Data, result.Total);
        }
    }
}
