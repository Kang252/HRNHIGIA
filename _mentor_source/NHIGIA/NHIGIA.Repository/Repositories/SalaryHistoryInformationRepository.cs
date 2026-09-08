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
    public class SalaryHistoryInformationRepository : BaseRepository<SalaryHistoryInformationEntity>, ISalaryHistoryInformationRepository
    {
        public ResponseList<SalaryHistoryInformationViewModel> GetAllSalaryHistoryInformation(int id, int employeeId)
        {
            _logger.Trace("Start SalaryHistoryInformationRepository - GetAllSalaryHistoryInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<SalaryHistoryInformationViewModel>(Constants.StoredProc.spGetSalaryHistoryInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("SalaryHistoryInformationRepository - GetAllSalaryHistoryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SalaryHistoryInformationRepository - GetAllSalaryHistoryInformation: " + DateTime.Now);
            return result;
        }

        public Response<SalaryHistoryInformationEntity> SaveSalaryHistoryInformation(TypeSalaryHistoryInformation param, int isAction)
        {
            _logger.Trace("Start SalaryHistoryInformationRepository - SaveSalaryHistoryInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<SalaryHistoryInformationEntity>(Constants.StoredProc.spSaveSalaryHistoryInformation,
                new StoredProcedureParameter("TypeSalaryHistoryInformation", new List<TypeSalaryHistoryInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("SalaryHistoryInformationRepository - SaveSalaryHistoryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SalaryHistoryInformationRepository - SaveSalaryHistoryInformation: " + DateTime.Now);
            return result;
        }
    }
}
