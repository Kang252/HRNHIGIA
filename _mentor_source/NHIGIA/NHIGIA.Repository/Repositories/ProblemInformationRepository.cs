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
    public class ProblemInformationRepository : BaseRepository<ProblemInformationEntity>, IProblemInformationRepository
    {
        public ResponseList<ProblemInformationViewModel> GetAllProblem(PagingData param)
        {
            _logger.Trace("Start ProblemInformationRepository - GetAllProblem: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<ProblemInformationViewModel>(Constants.StoredProc.spGetAllProblem
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("ProblemInformationRepository - GetAllProblem - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRepository - GetAllProblem: " + DateTime.Now);
            return result;
        }

        public ResponseList<ProblemInformationViewModel> GetAllProblemInformation(int id, int employeeId)
        {
            _logger.Trace("Start ProblemInformationRepository - GetAllProblemInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<ProblemInformationViewModel>(Constants.StoredProc.spGetProblemInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("ProblemInformationRepository - GetAllProblemInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRepository - GetAllProblemInformation: " + DateTime.Now);
            return result;
        }

        public Response<ProblemInformationEntity> GetProblemById(int id)
        {
            _logger.Trace("Start ProblemInformationRepository - GetProblemById: " + DateTime.Now);
            var result = GetByStoredProcedure<ProblemInformationEntity>(Constants.StoredProc.spGetProblemById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("ProblemInformationRepository - GetProblemById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRepository - GetProblemById: " + DateTime.Now);
            return result;
        }

        public Response<ProblemInformationEntity> SaveProblemInformation(TypeProblemInformation param, int isAction)
        {
            _logger.Trace("Start ProblemInformationRepository - SaveProblemInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<ProblemInformationEntity>(Constants.StoredProc.spSaveProblemInformation,
                new StoredProcedureParameter("TypeProblemInformation", new List<TypeProblemInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ProblemInformationRepository - SaveProblemInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRepository - SaveProblemInformation: " + DateTime.Now);
            return result;
        }
    }
}