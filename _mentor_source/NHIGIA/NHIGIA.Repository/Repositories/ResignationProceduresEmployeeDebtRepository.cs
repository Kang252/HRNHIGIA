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
    public class ResignationProceduresEmployeeDebtRepository : BaseRepository<ResignationProceduresEmployeeDebtEntity>, IResignationProceduresEmployeeDebtRepository
    {
        public ResponseList<ResignationProceduresEmployeeDebtViewModel> GetAllResignationProceduresEmployeeDebt(int id, int resignationProceduresId)
        {
            _logger.Trace("Start ResignationProceduresEmployeeDebtRepository - GetAllResignationProceduresEmployeeDebt: " + DateTime.Now);
            var result = ListByStoredProcedure<ResignationProceduresEmployeeDebtViewModel>(Constants.StoredProc.spGetResignationProceduresEmployeeDebt,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("ResignationProceduresId", resignationProceduresId, DbType.Int32));
            _logger.Info("ResignationProceduresEmployeeDebtRepository - GetAllResignationProceduresEmployeeDebt - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresEmployeeDebtRepository - GetAllResignationProceduresEmployeeDebt: " + DateTime.Now);
            return result;
        }

        public Response<ResignationProceduresEmployeeDebtEntity> SaveResignationProceduresEmployeeDebt(TypeResignationProceduresEmployeeDebt param, int isAction)
        {
            _logger.Trace("Start ResignationProceduresEmployeeDebtRepository - SaveResignationProceduresEmployeeDebt: " + DateTime.Now);
            var result = GetByStoredProcedure<ResignationProceduresEmployeeDebtEntity>(Constants.StoredProc.spSaveResignationProceduresEmployeeDebt,
                new StoredProcedureParameter("TypeResignationProceduresEmployeeDebt", new List<TypeResignationProceduresEmployeeDebt> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ResignationProceduresEmployeeDebtRepository - SaveResignationProceduresEmployeeDebt - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ResignationProceduresEmployeeDebtRepository - SaveResignationProceduresEmployeeDebt: " + DateTime.Now);
            return result;
        }
    }
}
