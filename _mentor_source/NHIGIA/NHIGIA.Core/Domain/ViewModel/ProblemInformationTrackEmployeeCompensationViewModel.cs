using NHIGIA.Core.Domain.Entity;
using System.Collections.Generic;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ProblemInformationTrackEmployeeCompensationViewModel : ProblemInformationTrackEmployeeCompensationEntity
    {
        public string PayDayString { get; set; }
        public int IsEdit { get; set; }
        public List<ProblemInformationTrackEmployeeCompensationEntity> ListProblemInformationTrackEmployeeCompensation { get; set; }
    }
}
