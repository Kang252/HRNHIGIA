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
    public class PageInformationRepository : BaseRepository<PageInformationEntity>, IPageInformationRepository
    {
        public ResponseList<PageInformationViewModel> GetAllPageInformation(int id, int employeeId)
        {
            _logger.Trace("Start PageInformationRepository - GetAllPageInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<PageInformationViewModel>(Constants.StoredProc.spGetPageInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("PageInformationRepository - GetAllPageInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PageInformationRepository - GetAllPageInformation: " + DateTime.Now);
            return result;
        }

        public Response<PageInformationEntity> SavePageInformation(TypePageInformation param, int isAction)
        {
            _logger.Trace("Start PageInformationRepository - SavePageInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<PageInformationEntity>(Constants.StoredProc.spSavePageInformation,
                new StoredProcedureParameter("TypePageInformation", new List<TypePageInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("PageInformationRepository - SavePageInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PageInformationRepository - SavePageInformation: " + DateTime.Now);
            return result;
        }
    }
}
