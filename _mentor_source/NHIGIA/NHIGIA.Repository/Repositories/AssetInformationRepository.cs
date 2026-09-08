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
    public class AssetInformationRepository : BaseRepository<AssetInformationEntity>, IAssetInformationRepository
    {
        public ResponseList<AssetInformationViewModel> GetAllAssetInformation(int id, int employeeId)
        {
            _logger.Trace("Start AssetInformationRepository - GetAllAssetInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<AssetInformationViewModel>(Constants.StoredProc.spGetAssetInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("AssetInformationRepository - GetAllAssetInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AssetInformationRepository - GetAllAssetInformation: " + DateTime.Now);
            return result;
        }

        public Response<AssetInformationEntity> SaveAssetInformation(TypeAssetInformation param, int isAction)
        {
            _logger.Trace("Start AssetInformationRepository - SaveAssetInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<AssetInformationEntity>(Constants.StoredProc.spSaveAssetInformation,
                new StoredProcedureParameter("TypeAssetInformation", new List<TypeAssetInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("AssetInformationRepository - SaveAssetInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AssetInformationRepository - SaveAssetInformation: " + DateTime.Now);
            return result;
        }
    }
}
