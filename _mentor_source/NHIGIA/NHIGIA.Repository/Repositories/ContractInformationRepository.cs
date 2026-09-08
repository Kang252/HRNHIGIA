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
    public class ContractInformationRepository : BaseRepository<ContractInformationEntity>, IContractInformationRepository
    {
        public ResponseList<ContractInformationViewModel> GetAllContractInformation(int id, int employeeId)
        {
            _logger.Trace("Start ContractInformationRepository - GetAllContractInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<ContractInformationViewModel>(Constants.StoredProc.spGetContractInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("ContractInformationRepository - GetAllContractInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ContractInformationRepository - GetAllContractInformation: " + DateTime.Now);
            return result;
        }

        public Response<ContractInformationEntity> SaveContractInformation(TypeContractInformation param, int isAction)
        {
            _logger.Trace("Start ContractInformationRepository - SaveContractInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<ContractInformationEntity>(Constants.StoredProc.spSaveContractInformation,
                new StoredProcedureParameter("TypeContractInformation", new List<TypeContractInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ContractInformationRepository - SaveContractInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ContractInformationRepository - SaveContractInformation: " + DateTime.Now);
            return result;
        }
    }
}
