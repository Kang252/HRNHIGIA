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
    public class SkinInformationRepository : BaseRepository<FamilyInformationEntity>, ISkinInformationRepository
    {
        public ResponseList<SkinInformationEntity> GetAllSkinInformation(int employeeId)
        {
            _logger.Trace("Start SkinInformationRepository - GetAllSkinInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<SkinInformationEntity>(Constants.StoredProc.spGetSkinInformation,
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("SkinInformationRepository - GetAllSkinInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkinInformationRepository - GetAllSkinInformation: " + DateTime.Now);
            return result;
        }

        public Response<SkinInformationEntity> SaveSkinInformation(TypeSkinInformation param)
        {
            _logger.Trace("Start SkinInformationRepository - SaveSkinInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<SkinInformationEntity>(Constants.StoredProc.spSaveSkinInformation,
                new StoredProcedureParameter("TypeSkinInformation", new List<TypeSkinInformation> { param }.ToUserDefinedDataTable(), DbType.Object));
            _logger.Info("SkinInformationRepository - SaveSkinInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkinInformationRepository - SaveSkinInformation: " + DateTime.Now);
            return result;
        }
    }
}
