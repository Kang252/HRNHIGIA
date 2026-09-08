using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;

namespace NHIGIA.Services.Implementations
{
    public class ListCategoryService : IListCategoryService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ListCategoryRepository _repository = new ListCategoryRepository();

        public ResponseList<ListCategoryViewModel> GetAllListCategory(PagingData param)
        {
            _logger.Trace("Start ListCategoryService - GetAllListCategory: " + DateTime.Now);
            var result = _repository.GetAllListCategory(param);
            _logger.Info("ListCategoryService - GetAllListCategory - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetAllListCategory: " + DateTime.Now);
            return new ResponseList<ListCategoryViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<ListCategoryEntity> GetListCategoryById(int id)
        {
            _logger.Trace("Start ListCategoryService - GetListCategoryById: " + DateTime.Now);
            var result = _repository.GetListCategoryById(id);
            _logger.Info("ListCategoryService - GetListCategoryById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetListCategoryById: " + DateTime.Now);
            return result;
        }

        public Response<ListCategoryEntity> SaveListCategory(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ListCategoryService - SaveListCategory: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ListCategoryEntity>(content);
                var data = MapperHelper.Map<ListCategoryEntity, TypeListCategory>(saveData);
                var result = _repository.SaveListCategory(data, isAction);
                if (result != null)
                {
                    _logger.Info("ListCategoryService - SaveListCategory - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ListCategoryService - SaveListCategory: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ListCategoryService - SaveListCategory - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ListCategoryService - SaveListCategory: " + DateTime.Now);
                return new Response<ListCategoryEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ListCategoryEntity>(false, e.Message, null);
            }
        }

        public Response<CheckDuplicateViewModel> CheckDuplicate(int id, string tableName, string name, int? listCategoryTypeId)
        {
            _logger.Trace("Start ListCategoryService - CheckDuplicate: " + DateTime.Now);
            var result = _repository.CheckDuplicate(id, tableName, name, listCategoryTypeId);
            _logger.Info("ListCategoryService - CheckDuplicate - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - CheckDuplicate: " + DateTime.Now);
            return result;
        }

        public ResponseList<ListCategoryEntity> GetDataForDropdown(int listCategoryTypeId)
        {
            _logger.Trace("Start ListCategoryService - GetDataForDropdown: " + DateTime.Now);
            var result = _repository.GetDataForDropdown(listCategoryTypeId);
            _logger.Info("ListCategoryService - GetDataForDropdown - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetDataForDropdown: " + DateTime.Now);
            return new ResponseList<ListCategoryEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<NationalityEntity> GetAllNationality()
        {
            _logger.Trace("Start ListCategoryService - GetAllNationality: " + DateTime.Now);
            var result = _repository.GetAllNationality();
            _logger.Info("ListCategoryService - GetAllNationality - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetAllNationality: " + DateTime.Now);
            return new ResponseList<NationalityEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<ProvinceCityEntity> GetAllProvinceCity(int nationalityId)
        {
            _logger.Trace("Start ListCategoryService - GetAllProvinceCity: " + DateTime.Now);
            var result = _repository.GetAllProvinceCity(nationalityId);
            _logger.Info("ListCategoryService - GetAllProvinceCity - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetAllProvinceCity: " + DateTime.Now);
            return new ResponseList<ProvinceCityEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<DistrictEntity> GetAllDistrict(int provinceCityId)
        {
            _logger.Trace("Start ListCategoryService - GetAllDistrict: " + DateTime.Now);
            var result = _repository.GetAllDistrict(provinceCityId);
            _logger.Info("ListCategoryService - GetAllDistrict - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetAllDistrict: " + DateTime.Now);
            return new ResponseList<DistrictEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<WardsEntity> GetAllWards(int districtId)
        {
            _logger.Trace("Start ListCategoryService - GetAllWards: " + DateTime.Now);
            var result = _repository.GetAllWards(districtId);
            _logger.Info("ListCategoryService - GetAllWards - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetAllWards: " + DateTime.Now);
            return new ResponseList<WardsEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public ResponseList<ListStatusEntity> GetStatusForDropdown(string type)
        {
            _logger.Trace("Start ListCategoryService - GetStatusForDropdown: " + DateTime.Now);
            var result = _repository.GetStatusForDropdown(type);
            _logger.Info("ListCategoryService - GetStatusForDropdown - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryService - GetStatusForDropdown: " + DateTime.Now);
            return new ResponseList<ListStatusEntity>(result.Success, result.Message, result.Data, result.Total);
        }
    }
}
