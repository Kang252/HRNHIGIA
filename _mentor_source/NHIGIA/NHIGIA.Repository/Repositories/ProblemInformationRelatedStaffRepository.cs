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
    public class ProblemInformationRelatedStaffRepository : BaseRepository<ProblemInformationRelatedStaffEntity>, IProblemInformationRelatedStaffRepository
    {
        public ResponseList<ProblemInformationRelatedStaffViewModel> GetAllProblemInformationRelatedStaff(int id, int problemInformationId)
        {
            _logger.Trace("Start ProblemInformationRelatedStaffRepository - GetAllProblemInformationRelatedStaff: " + DateTime.Now);
            var result = ListByStoredProcedure<ProblemInformationRelatedStaffViewModel>(Constants.StoredProc.spGetProblemInformationRelatedStaff,
                new StoredProcedureParameter("Id", id, DbType.Int32),
                new StoredProcedureParameter("ProblemInformationId", problemInformationId, DbType.Int32));
            _logger.Info("ProblemInformationRelatedStaffRepository - GetAllProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRelatedStaffRepository - GetAllProblemInformationRelatedStaff: " + DateTime.Now);
            return result;
        }

        public ResponseList<ProblemInformationRelatedStaffEntity> SaveProblemInformationRelatedStaff(DataTable param, int isAction)
        {
            _logger.Trace("Start ProblemInformationRelatedStaffRepository - SaveProblemInformationRelatedStaff: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spSaveProblemInformationRelatedStaff,
                new
                {
                    TypeProblemInformationRelatedStaff = param,
                    IsAction = isAction
                });
            _logger.Info("ProblemInformationRelatedStaffRepository - SaveProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRelatedStaffRepository - SaveProblemInformationRelatedStaff: " + DateTime.Now);
            return result;
        }

        public Response<ProblemInformationRelatedStaffEntity> SaveOnlyProblemInformationRelatedStaff(TypeProblemInformationRelatedStaff param, int isAction)
        {
            _logger.Trace("Start ProblemInformationRelatedStaffRepository - SaveOnlyProblemInformationRelatedStaff: " + DateTime.Now);
            var result = GetByStoredProcedure<ProblemInformationRelatedStaffEntity>(Constants.StoredProc.spSaveProblemInformationRelatedStaff,
                new StoredProcedureParameter("TypeProblemInformationRelatedStaff", new List<TypeProblemInformationRelatedStaff> { param }.ToUserDefinedDataTable(), DbType.Object),
                new StoredProcedureParameter("IsAction", isAction, DbType.Int32));
            _logger.Info("ProblemInformationRelatedStaffRepository - SaveOnlyProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRelatedStaffRepository - SaveOnlyProblemInformationRelatedStaff: " + DateTime.Now);
            return result;
        }
    }
}
