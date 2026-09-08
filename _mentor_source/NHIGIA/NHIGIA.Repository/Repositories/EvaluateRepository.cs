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
    public class EvaluateRepository : BaseRepository<EvaluateEntity>, IEvaluateRepository
    {
        public ResponseList<EvaluateViewModel> GetAllEvaluate(PagingData param)
        {
            _logger.Trace("Start EvaluateRepository - GetAllEvaluate: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<EvaluateViewModel>(Constants.StoredProc.spGetAllEvaluate
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("EvaluateRepository - GetAllEvaluate - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - GetAllEvaluate: " + DateTime.Now);
            return result;
        }

        public ResponseList<EvaluateDetailViewModel> GetAllEvaluateDetail(PagingData param, int evaluateId)
        {
            _logger.Trace("Start EvaluateRepository - GetAllEvaluateDetail: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<EvaluateDetailViewModel>(Constants.StoredProc.spGetAllEvaluateDetail
                , new StoredProcedureParameter("EvaluateId", evaluateId, DbType.Int32)
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("EvaluateRepository - GetAllEvaluateDetail - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - GetAllEvaluateDetail: " + DateTime.Now);
            return result;
        }

        public ResponseList<EvaluateDetailViewModel> GetEvaluateByEmployee(int id, int employeeId)
        {
            _logger.Trace("Start EvaluateRepository - GetEvaluateByEmployee: " + DateTime.Now);
            var result = ListByStoredProcedure<EvaluateDetailViewModel>(Constants.StoredProc.spGetEvaluateByEmployee,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("EvaluateRepository - GetEvaluateByEmployee - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - GetEvaluateByEmployee: " + DateTime.Now);
            return result;
        }

        public Response<EvaluateViewModel> GetEvaluateById(int id)
        {
            _logger.Trace("Start EvaluateRepository - GetEvaluateById: " + DateTime.Now);
            var result = GetByStoredProcedure<EvaluateViewModel>(Constants.StoredProc.spGetEvaluateById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("EvaluateRepository - GetEvaluateById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - GetEvaluateById: " + DateTime.Now);
            return result;
        }

        public Response<EvaluateEntity> SaveEvaluate(TypeEvaluate param, int isAction)
        {
            _logger.Trace("Start EvaluateRepository - SaveEvaluate: " + DateTime.Now);
            var result = GetByStoredProcedure<EvaluateEntity>(Constants.StoredProc.spSaveEvaluate,
                new StoredProcedureParameter("TypeEvaluate", new List<TypeEvaluate> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EvaluateRepository - SaveEvaluate - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - SaveEvaluate: " + DateTime.Now);
            return result;
        }

        public Response<EvaluateDetailEntity> SaveEvaluateDetail(TypeEvaluateDetail param, int isAction)
        {
            _logger.Trace("Start EvaluateRepository - SaveEvaluateDetail: " + DateTime.Now);
            var result = GetByStoredProcedure<EvaluateDetailEntity>(Constants.StoredProc.spSaveEvaluateDetail,
                new StoredProcedureParameter("TypeEvaluateDetail", new List<TypeEvaluateDetail> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EvaluateRepository - SaveEvaluateDetail - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EvaluateRepository - SaveEvaluateDetail: " + DateTime.Now);
            return result;
        }
    }
}
