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
    public class PoliticsHealthMilitaryInformationRepository : BaseRepository<FamilyInformationEntity>, IPoliticsHealthMilitaryInformationRepository
    {
        public ResponseList<PoliticsHealthMilitaryInformationViewModel> GetAllPoliticsHealthMilitaryInformation(int employeeId)
        {
            _logger.Trace("Start PoliticsHealthMilitaryInformationRepository - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<PoliticsHealthMilitaryInformationViewModel>(Constants.StoredProc.spGetPoliticsHealthMilitaryInformation,
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("PoliticsHealthMilitaryInformationRepository - GetAllPoliticsHealthMilitaryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PoliticsHealthMilitaryInformationRepository - GetAllPoliticsHealthMilitaryInformation: " + DateTime.Now);
            return result;
        }

        public Response<PoliticsHealthMilitaryInformationEntity> SavePoliticsHealthMilitaryInformation(TypePoliticsHealthMilitaryInformation param)
        {
            _logger.Trace("Start PoliticsHealthMilitaryInformationRepository - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<PoliticsHealthMilitaryInformationEntity>(Constants.StoredProc.spSavePoliticsHealthMilitaryInformation,
                new StoredProcedureParameter("TypePoliticsHealthMilitaryInformation", new List<TypePoliticsHealthMilitaryInformation> { param }.ToUserDefinedDataTable(), DbType.Object));
            _logger.Info("PoliticsHealthMilitaryInformationRepository - SavePoliticsHealthMilitaryInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PoliticsHealthMilitaryInformationRepository - SavePoliticsHealthMilitaryInformation: " + DateTime.Now);
            return result;
        }
    }
}
