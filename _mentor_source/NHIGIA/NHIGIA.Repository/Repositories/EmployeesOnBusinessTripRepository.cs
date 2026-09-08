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
    public class EmployeesOnBusinessTripRepository : BaseRepository<EmployeesOnBusinessTripEntity>, IEmployeesOnBusinessTripRepository
    {
        public ResponseList<EmployeesOnBusinessTripViewModel> GetAllEmployeesOnBusinessTrip(PagingData param)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRepository - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
            var offset = (param.PageIndex - 1) * param.Length;
            var totalPara = new StoredProcedureParameter("Total", 0, DbType.Int32, ParameterDirection.Output, 0);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripViewModel>(Constants.StoredProc.spGetAllEmployeesOnBusinessTrip
                , new StoredProcedureParameter("Offset", offset, DbType.Int32)
                , new StoredProcedureParameter("PageSize", param.Length, DbType.Int32)
                , totalPara
                , new StoredProcedureParameter("SortColumns", param.SortColumns.ToUserDefinedDataTable(), DbType.Object)
                , new StoredProcedureParameter("FilterColumns", param.FilterColumns.ToUserDefinedDataTable(), DbType.Object));

            if (result.Success)
            {
                result.Total = Convert.ToInt32(totalPara.Value);
            }
            _logger.Info("EmployeesOnBusinessTripRepository - GetAllEmployeesOnBusinessTrip - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRepository - GetAllEmployeesOnBusinessTrip: " + DateTime.Now);
            return result;
        }

        public Response<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripById(int id)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripById: " + DateTime.Now);
            var result = GetByStoredProcedure<EmployeesOnBusinessTripViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripById, new StoredProcedureParameter("Id", id, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripById - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripById: " + DateTime.Now);
            return result;
        }

        public ResponseList<EmployeesOnBusinessTripViewModel> GetEmployeesOnBusinessTripInformation(int employeeId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<EmployeesOnBusinessTripViewModel>(Constants.StoredProc.spGetEmployeesOnBusinessTripInformation,
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRepository - GetEmployeesOnBusinessTripInformation: " + DateTime.Now);
            return result;
        }

        public Response<EmployeesOnBusinessTripEntity> SaveEmployeesOnBusinessTrip(TypeEmployeesOnBusinessTrip param, int isAction)
        {
            _logger.Trace("Start EmployeesOnBusinessTripRepository - SaveEmployeesOnBusinessTrip: " + DateTime.Now);
            var result = GetByStoredProcedure<EmployeesOnBusinessTripEntity>(Constants.StoredProc.spSaveEmployeesOnBusinessTrip,
                new StoredProcedureParameter("TypeEmployeesOnBusinessTrip", new List<TypeEmployeesOnBusinessTrip> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("EmployeesOnBusinessTripRepository - SaveEmployeesOnBusinessTrip - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripRepository - SaveEmployeesOnBusinessTrip: " + DateTime.Now);
            return result;
        }
    }
}
