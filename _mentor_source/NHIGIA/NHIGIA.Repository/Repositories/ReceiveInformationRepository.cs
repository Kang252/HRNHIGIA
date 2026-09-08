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
    public class ReceiveInformationRepository : BaseRepository<ReceiveInformationEntity>, IReceiveInformationRepository
    {
        public ResponseList<ReceiveInformationViewModel> GetAllReceiveInformation(int id, int employeeId)
        {
            _logger.Trace("Start ReceiveInformationRepository - GetAllReceiveInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<ReceiveInformationViewModel>(Constants.StoredProc.spGetReceiveInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("ReceiveInformationRepository - GetAllReceiveInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ReceiveInformationRepository - GetAllReceiveInformation: " + DateTime.Now);
            return result;
        }

        public Response<ReceiveInformationEntity> SaveReceiveInformation(TypeReceiveInformation param, int isAction)
        {
            _logger.Trace("Start ReceiveInformationRepository - SaveReceiveInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<ReceiveInformationEntity>(Constants.StoredProc.spSaveReceiveInformation,
                new StoredProcedureParameter("TypeReceiveInformation", new List<TypeReceiveInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ReceiveInformationRepository - SaveReceiveInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ReceiveInformationRepository - SaveReceiveInformation: " + DateTime.Now);
            return result;
        }
    }
}
