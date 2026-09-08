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
    public class CertificateInformationRepository : BaseRepository<CertificateInformationEntity>, ICertificateInformationRepository
    {
        public ResponseList<CertificateInformationViewModel> GetAllCertificateInformation(int id, int employeeId)
        {
            _logger.Trace("Start CertificateInformationRepository - GetAllCertificateInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<CertificateInformationViewModel>(Constants.StoredProc.spGetCertificateInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("CertificateInformationRepository - GetAllCertificateInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End CertificateInformationRepository - GetAllCertificateInformation: " + DateTime.Now);
            return result;
        }

        public Response<CertificateInformationEntity> SaveCertificateInformation(TypeCertificateInformation param, int isAction)
        {
            _logger.Trace("Start CertificateInformationRepository - SaveCertificateInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<CertificateInformationEntity>(Constants.StoredProc.spSaveCertificateInformation,
                new StoredProcedureParameter("TypeCertificateInformation", new List<TypeCertificateInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("CertificateInformationRepository - SaveCertificateInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End CertificateInformationRepository - SaveCertificateInformation: " + DateTime.Now);
            return result;
        }
    }
}
