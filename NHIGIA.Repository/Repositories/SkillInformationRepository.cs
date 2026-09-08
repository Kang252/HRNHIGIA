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
    public class SkillInformationRepository : BaseRepository<SkillInformationEntity>, ISkillInformationRepository
    {
        public ResponseList<SkillInformationViewModel> GetAllSkillInformation(int id, int employeeId)
        {
            _logger.Trace("Start SkillInformationRepository - GetAllSkillInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<SkillInformationViewModel>(Constants.StoredProc.spGetSkillInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("SkillInformationRepository - GetAllSkillInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkillInformationRepository - GetAllSkillInformation: " + DateTime.Now);
            return result;
        }

        public Response<SkillInformationEntity> SaveSkillInformation(TypeSkillInformation param, int isAction)
        {
            _logger.Trace("Start SkillInformationRepository - SaveSkillInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<SkillInformationEntity>(Constants.StoredProc.spSaveSkillInformation,
                new StoredProcedureParameter("TypeSkillInformation", new List<TypeSkillInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("SkillInformationRepository - SaveSkillInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkillInformationRepository - SaveSkillInformation: " + DateTime.Now);
            return result;
        }
    }
}
