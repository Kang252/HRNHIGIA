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
    public class AttachmentInformationRepository : BaseRepository<AttachmentInformationEntity>, IAttachmentInformationRepository
    {
        public Response<AttachmentInformationEntity> DownloadAttachmentInformation(int id)
        {
            return GetByStoredProcedure(Constants.StoredProc.spDownloadAttachmentInformation, new StoredProcedureParameter("Id", id, DbType.Int32));
        }

        public ResponseList<AttachmentInformationEntity> GetAllAttachmentInformation(int id, int employeeId, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start AttachmentInformationRepository - GetAllAttachmentInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<AttachmentInformationEntity>(Constants.StoredProc.spGetAttachmentInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32),
            new StoredProcedureParameter("EmployeesOnBusinessTripId", employeesOnBusinessTripId, DbType.Int32));
            _logger.Info("AttachmentInformationRepository - GetAllAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AttachmentInformationRepository - GetAllAttachmentInformation: " + DateTime.Now);
            return result;
        }

        public Response<AttachmentInformationEntity> SaveAttachmentInformation(TypeAttachmentInformation param, int isAction)
        {
            _logger.Trace("Start AttachmentInformationRepository - SaveAttachmentInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<AttachmentInformationEntity>(Constants.StoredProc.spSaveAttachmentInformation,
                new StoredProcedureParameter("TypeAttachmentInformation", new List<TypeAttachmentInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("AttachmentInformationRepository - SaveAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AttachmentInformationRepository - SaveAttachmentInformation: " + DateTime.Now);
            return result;
        }

        public Response<AttachmentViewModel> UpdateAttachmentInformation(TypeAttachment param)
        {
            _logger.Trace("Start AttachmentInformationRepository - UpdateAttachmentInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<AttachmentViewModel>(Constants.StoredProc.spUpdateAttachmentInformation,
                new StoredProcedureParameter("TypeAttachment", new List<TypeAttachment> { param }.ToUserDefinedDataTable(), DbType.Object));
            _logger.Info("AttachmentInformationRepository - UpdateAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AttachmentInformationRepository - UpdateAttachmentInformation: " + DateTime.Now);
            return result;
        }
    }
}
