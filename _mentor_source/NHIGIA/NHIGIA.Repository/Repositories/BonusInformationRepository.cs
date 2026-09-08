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
    public class BonusInformationRepository : BaseRepository<BonusInformationEntity>, IBonusInformationRepository
    {
        public ResponseList<BonusInformationViewModel> GetAllBonus(PagingData param)
        {
            _logger.Trace("Start BonusInformationRepository - GetAllBonus: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<BonusInformationViewModel>(Constants.StoredProc.spGetAllBonus
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("BonusInformationRepository - GetAllBonus - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationRepository - GetAllBonus: " + DateTime.Now);
            return result;
        }

        public ResponseList<BonusInformationViewModel> GetAllBonusInformation(int employeeId)
        {
            _logger.Trace("Start BonusInformationRepository - GetAllBonusInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<BonusInformationViewModel>(Constants.StoredProc.spGetBonusInformation,
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("BonusInformationRepository - GetAllBonusInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationRepository - GetAllBonusInformation: " + DateTime.Now);
            return result;
        }

        public Response<BonusInformationViewModel> GetBonusById(int id)
        {
            _logger.Trace("Start BonusInformationRepository - GetBonusById: " + DateTime.Now);
            var result = GetByStoredProcedure<BonusInformationViewModel>(Constants.StoredProc.spGetBonusById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("BonusInformationRepository - GetBonusById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationRepository - GetBonusById: " + DateTime.Now);
            return result;
        }

        public Response<BonusInformationEntity> SaveBonusInformation(TypeBonusInformation param, int isAction)
        {
            _logger.Trace("Start BonusInformationRepository - SaveBonusInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<BonusInformationEntity>(Constants.StoredProc.spSaveBonusInformation,
                new StoredProcedureParameter("TypeBonusInformation", new List<TypeBonusInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("BonusInformationRepository - SaveBonusInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationRepository - SaveBonusInformation: " + DateTime.Now);
            return result;
        }
    }
}
