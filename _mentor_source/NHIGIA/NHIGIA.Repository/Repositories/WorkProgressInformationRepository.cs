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
    public class WorkProgressInformationRepository : BaseRepository<WorkProgressInformationEntity>, IWorkProgressInformationRepository
    {
        public ResponseList<WorkProgressInformationViewModel> GetAllWorkProgressInformation(int id, int employeeId)
        {
            _logger.Trace("Start WorkProgressInformationRepository - GetAllWorkProgressInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<WorkProgressInformationViewModel>(Constants.StoredProc.spGetWorkProgressInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("WorkProgressInformationRepository - GetAllWorkProgressInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkProgressInformationRepository - GetAllWorkProgressInformation: " + DateTime.Now);
            return result;
        }

        public Response<WorkProgressInformationEntity> SaveWorkProgressInformation(TypeWorkProgressInformation param, int isAction)
        {
            _logger.Trace("Start WorkProgressInformationRepository - SaveWorkProgressInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<WorkProgressInformationEntity>(Constants.StoredProc.spSaveWorkProgressInformation,
                new StoredProcedureParameter("TypeWorkProgressInformation", new List<TypeWorkProgressInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("WorkProgressInformationRepository - SaveWorkProgressInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkProgressInformationRepository - SaveWorkProgressInformation: " + DateTime.Now);
            return result;
        }
    }
}