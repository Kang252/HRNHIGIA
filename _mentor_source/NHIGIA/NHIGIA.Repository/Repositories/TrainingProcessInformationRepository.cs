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
    public class TrainingProcessInformationRepository : BaseRepository<TrainingProcessInformationEntity>, ITrainingProcessInformationRepository
    {
        public ResponseList<TrainingProcessInformationViewModel> GetAllTrainingProcessInformation(int id, int employeeId)
        {
            _logger.Trace("Start TrainingProcessInformationRepository - GetAllTrainingProcessInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<TrainingProcessInformationViewModel>(Constants.StoredProc.spGetTrainingProcessInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("TrainingProcessInformationRepository - GetAllTrainingProcessInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End TrainingProcessInformationRepository - GetAllTrainingProcessInformation: " + DateTime.Now);
            return result;
        }

        public Response<TrainingProcessInformationEntity> SaveTrainingProcessInformation(TypeTrainingProcessInformation param, int isAction)
        {
            _logger.Trace("Start TrainingProcessInformationRepository - SaveTrainingProcessInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<TrainingProcessInformationEntity>(Constants.StoredProc.spSaveTrainingProcessInformation,
                new StoredProcedureParameter("TypeTrainingProcessInformation", new List<TypeTrainingProcessInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("TrainingProcessInformationRepository - SaveTrainingProcessInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End TrainingProcessInformationRepository - SaveTrainingProcessInformation: " + DateTime.Now);
            return result;
        }
    }
}
