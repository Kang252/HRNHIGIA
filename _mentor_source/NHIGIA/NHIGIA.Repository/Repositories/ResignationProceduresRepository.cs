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
    public class ResignationProceduresRepository : BaseRepository<ResignationProceduresEntity>, IResignationProceduresRepository
    {
        public Response<ResignationProceduresViewModel> GetGeneralInformationForResignationProcedures(int id)
        {
            _logger.Trace("Start ResignationProceduresRepository - GetGeneralInformationForResignationProcedures: " + DateTime.Now);
            var result = GetByStoredProcedure<ResignationProceduresViewModel>(Constants.StoredProc.spGetGeneralInformationForResignationProcedures, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("ResignationProceduresRepository - GetGeneralInformationForResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresRepository - GetGeneralInformationForResignationProcedures: " + DateTime.Now);
            return result;
        }

        public ResponseList<ResignationProceduresViewModel> GetResignationProcedures(PagingData param)
        {
            _logger.Trace("Start ResignationProceduresRepository - GetResignationProcedures: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<ResignationProceduresViewModel>(Constants.StoredProc.spGetResignationProcedures
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("ResignationProceduresRepository - GetResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresRepository - GetResignationProcedures: " + DateTime.Now);
            return result;
        }

        public Response<ResignationProceduresEntity> SaveResignationProcedures(TypeResignationProcedures param, int isAction)
        {
            _logger.Trace("Start ResignationProceduresRepository - SaveResignationProcedures: " + DateTime.Now);
            var result = GetByStoredProcedure<ResignationProceduresEntity>(Constants.StoredProc.spSaveResignationProcedures,
                new StoredProcedureParameter("TypeResignationProcedures", new List<TypeResignationProcedures> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ResignationProceduresRepository - SaveResignationProcedures - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresRepository - SaveResignationProcedures: " + DateTime.Now);
            return result;
        }
    }
}
