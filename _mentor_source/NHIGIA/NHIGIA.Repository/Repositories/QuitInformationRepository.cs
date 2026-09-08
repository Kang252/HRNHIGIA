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
    public class QuitInformationRepository : BaseRepository<QuitInformationEntity>, IQuitInformationRepository
    {
        public ResponseList<QuitInformationViewModel> GetAllQuitInformation(int id, int employeeId)
        {
            _logger.Trace("Start QuitInformationRepository - GetAllQuitInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<QuitInformationViewModel>(Constants.StoredProc.spGetQuitInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("QuitInformationRepository - GetAllQuitInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End QuitInformationRepository - GetAllQuitInformation: " + DateTime.Now);
            return result;
        }

        public Response<QuitInformationEntity> SaveQuitInformation(TypeQuitInformation param, int isAction)
        {
            _logger.Trace("Start QuitInformationRepository - SaveQuitInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<QuitInformationEntity>(Constants.StoredProc.spSaveQuitInformation,
                new StoredProcedureParameter("TypeQuitInformation", new List<TypeQuitInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("QuitInformationRepository - SaveQuitInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End QuitInformationRepository - SaveQuitInformation: " + DateTime.Now);
            return result;
        }
    }
}
