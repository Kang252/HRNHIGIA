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
    public class EmployeeInformationRepository : BaseRepository<EmployeeInformationEntity>, IEmployeeInformationRepository
    {
        public ResponseList<EmployeeInformationViewModel> GetAllEmployeeInformation(PagingData param)
        {
            _logger.Trace("Start EmployeeInformationRepository - GetAllEmployeeInformation: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<EmployeeInformationViewModel>(Constants.StoredProc.spGetAllEmployeeInformation
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("EmployeeInformationRepository - GetAllEmployeeInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationRepository - GetAllEmployeeInformation: " + DateTime.Now);
            return result;
        }

        public ResponseList<EmployeeInformationViewModel> GetEmployeeForAutoCompleBox(string keyword)
        {
            _logger.Trace("Start EmployeeInformationRepository - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeeInformationViewModel>(Constants.StoredProc.spGetEmployeeForAutoCompleBox, new StoredProcedureParameter("Keyword", keyword, DbType.String));
            _logger.Info("EmployeeInformationRepository - GetEmployeeForAutoCompleBox - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationRepository - GetEmployeeForAutoCompleBox: " + DateTime.Now);
            return result;
        }

        public Response<ProfileEntity> GetEmployeeInformationById(int id)
        {
            _logger.Trace("Start EmployeeInformationRepository - GetEmployeeInformationById: " + DateTime.Now);
            var result = GetByStoredProcedure<ProfileEntity>(Constants.StoredProc.spGetEmployeeInformationById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("EmployeeInformationRepository - GetEmployeeInformationById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationRepository - GetEmployeeInformationById: " + DateTime.Now);
            return result;
        }

        public Response<ProfileEntity> SaveProfile(TypeProfile param, int isAction)
        {
            _logger.Trace("Start EmployeeInformationRepository - SaveProfile: " + DateTime.Now);
            var result = GetByStoredProcedure<ProfileEntity>(Constants.StoredProc.spSaveProfile,
                new StoredProcedureParameter("TypeProfile", new List<TypeProfile> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EmployeeInformationRepository - SaveProfile - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationRepository - SaveProfile: " + DateTime.Now);
            return result;
        }

        public ResponseList<EmployeeInformationViewModel> GetEmployee(PagingData param, int id, string type)
        {
            _logger.Trace("Start EmployeeInformationRepository - GetEmployee: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<EmployeeInformationViewModel>(Constants.StoredProc.spGetEmployee
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("Id", id, DbType.Int32)
                , new StoredProcedureParameter("Type", type, DbType.String));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("EmployeeInformationRepository - GetEmployee - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeeInformationRepository - GetEmployee: " + DateTime.Now);
            return result;
        }
    }
}
