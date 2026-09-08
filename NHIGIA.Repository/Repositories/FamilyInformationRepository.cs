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
    public class FamilyInformationRepository : BaseRepository<FamilyInformationEntity>, IFamilyInformationRepository
    {
        public ResponseList<FamilyInformationViewModel> GetAllFamilyInformation(int id, int employeeId)
        {
            _logger.Trace("Start FamilyInformationRepository - GetAllFamilyInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<FamilyInformationViewModel>(Constants.StoredProc.spGetFamilyInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("FamilyInformationRepository - GetAllFamilyInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End FamilyInformationRepository - GetAllFamilyInformation: " + DateTime.Now);
            return result;
        }

        public Response<FamilyInformationEntity> SaveFamilyInformation(TypeFamilyInformation param, int isAction)
        {
            _logger.Trace("Start FamilyInformationRepository - SaveFamilyInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<FamilyInformationEntity>(Constants.StoredProc.spSaveFamilyInformation,
                new StoredProcedureParameter("TypeFamilyInformation", new List<TypeFamilyInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("FamilyInformationRepository - SaveFamilyInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End FamilyInformationRepository - SaveFamilyInformation: " + DateTime.Now);
            return result;
        }
    }
}
