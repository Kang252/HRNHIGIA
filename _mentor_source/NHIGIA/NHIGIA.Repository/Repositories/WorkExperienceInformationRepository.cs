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
    public class WorkExperienceInformationRepository : BaseRepository<WorkExperienceInformationEntity>, IWorkExperienceInformationRepository
    {
        public ResponseList<WorkExperienceInformationViewModel> GetAllWorkExperienceInformation(int id, int employeeId)
        {
            _logger.Trace("Start WorkExperienceInformationRepository - GetAllWorkExperienceInformation: " + DateTime.Now);
            var result = ListByStoredProcedure<WorkExperienceInformationViewModel>(Constants.StoredProc.spGetWorkExperienceInformation,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32));
            _logger.Info("WorkExperienceInformationRepository - GetAllWorkExperienceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkExperienceInformationRepository - GetAllWorkExperienceInformation: " + DateTime.Now);
            return result;
        }

        public Response<WorkExperienceInformationEntity> SaveWorkExperienceInformation(TypeWorkExperienceInformation param, int isAction)
        {
            _logger.Trace("Start WorkExperienceInformationRepository - SaveWorkExperienceInformation: " + DateTime.Now);
            var result = GetByStoredProcedure<WorkExperienceInformationEntity>(Constants.StoredProc.spSaveWorkExperienceInformation,
                new StoredProcedureParameter("TypeWorkExperienceInformation", new List<TypeWorkExperienceInformation> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("WorkExperienceInformationRepository - SaveWorkExperienceInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End WorkExperienceInformationRepository - SaveWorkExperienceInformation: " + DateTime.Now);
            return result;
        }
    }
}
