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
    public class BonusInformationStaffWereCommendedRepository : BaseRepository<BonusInformationStaffWereCommendedEntity>, IBonusInformationStaffWereCommendedRepository
    {
        public ResponseList<BonusInformationStaffWereCommendedViewModel> GetAllBonusInformationStaffWereCommended(int bonusInformationId)
        {
            _logger.Trace("Start BonusInformationStaffWereCommendedRepository - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
            var result = ListByStoredProcedure<BonusInformationStaffWereCommendedViewModel>(Constants.StoredProc.spGetBonusInformationStaffWereCommended,
                new StoredProcedureParameter("BonusInformationId", bonusInformationId, DbType.Int32));
            _logger.Info("BonusInformationStaffWereCommendedRepository - GetAllBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationStaffWereCommendedRepository - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
            return result;
        }

        public ResponseList<BonusInformationStaffWereCommendedEntity> SaveBonusInformationStaffWereCommended(DataTable param, int isAction)
        {
            _logger.Trace("Start BonusInformationStaffWereCommendedRepository - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spSaveBonusInformationStaffWereCommended,
                new
                {
                    TypeBonusInformationStaffWereCommended = param,
                    IsAction = isAction
                });
            _logger.Info("BonusInformationStaffWereCommendedRepository - SaveBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationStaffWereCommendedRepository - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
            return result;
        }

        public Response<BonusInformationStaffWereCommendedEntity> SaveOnlyBonusInformationStaffWereCommended(TypeBonusInformationStaffWereCommended param, int isAction)
        {
            _logger.Trace("Start BonusInformationStaffWereCommendedRepository - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
            var result = GetByStoredProcedure<BonusInformationStaffWereCommendedEntity>(Constants.StoredProc.spSaveBonusInformationStaffWereCommended,
                new StoredProcedureParameter("TypeBonusInformationStaffWereCommended", new List<TypeBonusInformationStaffWereCommended> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("BonusInformationStaffWereCommendedRepository - SaveOnlyBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationStaffWereCommendedRepository - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
            return result;
        }
    }
}
