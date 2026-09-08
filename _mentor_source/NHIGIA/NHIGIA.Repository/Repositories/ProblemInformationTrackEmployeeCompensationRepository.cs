using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Infrastructure;
using NHIGIA.Repository.Pattern;
using Newtonsoft.Json;
using System;
using System.Data;

namespace NHIGIA.Repository.Repositories
{
    public class ProblemInformationTrackEmployeeCompensationRepository : BaseRepository<ProblemInformationTrackEmployeeCompensationEntity>, IProblemInformationTrackEmployeeCompensationRepository
    {
        public ResponseList<ProblemInformationTrackEmployeeCompensationViewModel> GetAllProblemInformationTrackEmployeeCompensation(int problemInformationId, int employeeId, int type)
        {
            _logger.Trace("Start ProblemInformationTrackEmployeeCompensationRepository - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            var result = ListByStoredProcedure<ProblemInformationTrackEmployeeCompensationViewModel>(Constants.StoredProc.spGetProblemInformationTrackEmployeeCompensation,
                new StoredProcedureParameter("ProblemInformationId", problemInformationId, DbType.Int32),
                new StoredProcedureParameter("EmployeeId", employeeId, DbType.Int32),
                new StoredProcedureParameter("Type", type, DbType.Int32));
            _logger.Info("ProblemInformationTrackEmployeeCompensationRepository - GetAllProblemInformationTrackEmployeeCompensation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationTrackEmployeeCompensationRepository - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            return result;
        }

        public ResponseList<ProblemInformationTrackEmployeeCompensationEntity> SaveProblemInformationTrackEmployeeCompensation(DataTable param, int isAction)
        {
            _logger.Trace("Start ProblemInformationTrackEmployeeCompensationRepository - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            var result = ListByStoredProcedure(Constants.StoredProc.spSaveProblemInformationTrackEmployeeCompensation,
                new
                {
                    TypeProblemInformationTrackEmployeeCompensation = param,
                    IsAction = isAction
                });
            _logger.Info("ProblemInformationTrackEmployeeCompensationRepository - SaveProblemInformationTrackEmployeeCompensation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationTrackEmployeeCompensationRepository - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            return result;
        }
    }
}
