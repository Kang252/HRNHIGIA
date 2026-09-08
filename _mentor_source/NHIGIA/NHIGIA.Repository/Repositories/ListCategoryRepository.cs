using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace NHIGIA.Repository.Repositories
{
    public class ListCategoryRepository : BaseRepository<ListCategoryEntity>, IListCategoryRepository
    {
        public ResponseList<ListCategoryViewModel> GetAllListCategory(PagingData param)
        {
            _logger.Trace("Start ListCategoryRepository - GetAllListCategory: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<ListCategoryViewModel>(Constants.StoredProc.spGetAllListCategory
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("ListCategoryRepository - GetAllListCategory - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetAllListCategory: " + DateTime.Now);
            return result;
        }

        public Response<ListCategoryEntity> GetListCategoryById(int id)
        {
            _logger.Trace("Start ListCategoryRepository - GetListCategoryById: " + DateTime.Now);
            var result = GetByStoredProcedure(Constants.StoredProc.spGetListCategoryById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("ListCategoryRepository - GetListCategoryById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetListCategoryById: " + DateTime.Now);
            return result;
        }

        public Response<ListCategoryEntity> SaveListCategory(TypeListCategory param, int isAction)
        {
            _logger.Trace("Start ListCategoryRepository - SaveListCategory: " + DateTime.Now);
            var result = GetByStoredProcedure<ListCategoryEntity>(Constants.StoredProc.spSaveListCategory,
                new StoredProcedureParameter("TypeListCategory", new List<TypeListCategory> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ListCategoryRepository - SaveListCategory - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - SaveListCategory: " + DateTime.Now);
            return result;
        }

        public Response<CheckDuplicateViewModel> CheckDuplicate(int id, string tableName, string name, int? listCategoryTypeId)
        {
            _logger.Trace("Start ListCategoryRepository - CheckDuplicate: " + DateTime.Now);
            var result = GetByStoredProcedure<CheckDuplicateViewModel>(Constants.StoredProc.spCheckDuplicate,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("TableName", tableName, DbType.String),
                new StoredProcedureParameter("Name", name, DbType.String),
                new StoredProcedureParameter("ListCategoryTypeId", listCategoryTypeId, DbType.Int32));
            _logger.Info("ListCategoryRepository - CheckDuplicate - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - CheckDuplicate: " + DateTime.Now);
            return result;
        }

        public ResponseList<ListCategoryEntity> GetDataForDropdown(int listCategoryTypeId)
        {
            _logger.Trace("Start ListCategoryRepository - GetDataForDropdown: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spGetDataForDropdown, new StoredProcedureParameter("ListCategoryTypeId", listCategoryTypeId, DbType.Int32));
            _logger.Info("ListCategoryRepository - GetDataForDropdown - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetDataForDropdown: " + DateTime.Now);
            return result;
        }

        public ResponseList<NationalityEntity> GetAllNationality()
        {
            _logger.Trace("Start ListCategoryRepository - GetAllNationality: " + DateTime.Now);
            var result = ListByStoredProcedure<NationalityEntity>(Constants.StoredProc.spGetAllNationality);
            _logger.Info("ListCategoryRepository - GetAllNationality - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetAllNationality: " + DateTime.Now);
            return result;
        }

        public ResponseList<ProvinceCityEntity> GetAllProvinceCity(int nationalityId)
        {
            _logger.Trace("Start ListCategoryRepository - GetAllProvinceCity: " + DateTime.Now);
            var result = ListByStoredProcedure<ProvinceCityEntity>(Constants.StoredProc.spGetAllProvinceCity, new StoredProcedureParameter("NationalityId", nationalityId, DbType.Int32));
            _logger.Info("ListCategoryRepository - GetAllProvinceCity - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetAllProvinceCity: " + DateTime.Now);
            return result;
        }

        public ResponseList<DistrictEntity> GetAllDistrict(int provinceCityId)
        {
            _logger.Trace("Start ListCategoryRepository - GetAllDistrict: " + DateTime.Now);
            var result = ListByStoredProcedure<DistrictEntity>(Constants.StoredProc.spGetAllDistrict, new StoredProcedureParameter("ProvinceCityId", provinceCityId, DbType.Int32));
            _logger.Info("ListCategoryRepository - GetAllDistrict - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetAllDistrict: " + DateTime.Now);
            return result;
        }

        public ResponseList<WardsEntity> GetAllWards(int districtId)
        {
            _logger.Trace("Start ListCategoryRepository - GetAllWards: " + DateTime.Now);
            var result = ListByStoredProcedure<WardsEntity>(Constants.StoredProc.spGetAllWards, new StoredProcedureParameter("DistrictId", districtId, DbType.Int32));
            _logger.Info("ListCategoryRepository - GetAllWards - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetAllWards: " + DateTime.Now);
            return result;
        }

        public ResponseList<ListStatusEntity> GetStatusForDropdown(string type)
        {
            _logger.Trace("Start ListCategoryRepository - GetStatusForDropdown: " + DateTime.Now);
            var result = ListByStoredProcedure<ListStatusEntity>(Constants.StoredProc.spGetStatusForDropdown, new StoredProcedureParameter("Type", type, DbType.String));
            _logger.Info("ListCategoryRepository - GetStatusForDropdown - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ListCategoryRepository - GetStatusForDropdown: " + DateTime.Now);
            return result;
        }
    }
}
