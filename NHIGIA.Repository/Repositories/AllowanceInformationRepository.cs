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
    public class AllowanceInformationRepository : BaseRepository<AllowanceInformationEntity>, IAllowanceInformationRepository
    {
        public ResponseList<AllowanceInformationViewModel> GetAllAllowanceInformation(int id, int employeeId)
        {
            _logger.Trace("Start AllowanceInformationRepository - GetAllAllowanceInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<AllowanceInformationViewModel>(Constants.StoredProc.spGetAllowanceInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("AllowanceInformationRepository - GetAllAllowanceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AllowanceInformationRepository - GetAllAllowanceInformation: " + DateTime.Now);
            return result;
        }

        public Response<AllowanceInformationEntity> SaveAllowanceInformation(TypeAllowanceInformation param, int isAction)
        {
            _logger.Trace("Start AllowanceInformationRepository - SaveAllowanceInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<AllowanceInformationEntity>(Constants.StoredProc.spSaveAllowanceInformation,
                new StoredProcedureParameter("TypeAllowanceInformation", new List<TypeAllowanceInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("AllowanceInformationRepository - SaveAllowanceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AllowanceInformationRepository - SaveAllowanceInformation: " + DateTime.Now);
            return result;
        }
    }
}
