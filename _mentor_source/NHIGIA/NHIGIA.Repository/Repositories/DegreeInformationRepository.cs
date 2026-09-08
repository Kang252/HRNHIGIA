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
    public class DegreeInformationRepository : BaseRepository<DegreeInformationEntity>, IDegreeInformationRepository
    {
        public ResponseList<DegreeInformationViewModel> GetAllDegreeInformation(int id, int employeeId)
        {
            _logger.Trace("Start DegreeInformationRepository - GetAllDegreeInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<DegreeInformationViewModel>(Constants.StoredProc.spGetDegreeInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("DegreeInformationRepository - GetAllDegreeInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End DegreeInformationRepository - GetAllDegreeInformation: " + DateTime.Now);
            return result;
        }

        public Response<DegreeInformationEntity> SaveDegreeInformation(TypeDegreeInformation param, int isAction)
        {
            _logger.Trace("Start DegreeInformationRepository - SaveDegreeInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<DegreeInformationEntity>(Constants.StoredProc.spSaveDegreeInformation,
                new StoredProcedureParameter("TypeDegreeInformation", new List<TypeDegreeInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("DegreeInformationRepository - SaveDegreeInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End DegreeInformationRepository - SaveDegreeInformation: " + DateTime.Now);
            return result;
        }
    }
}
