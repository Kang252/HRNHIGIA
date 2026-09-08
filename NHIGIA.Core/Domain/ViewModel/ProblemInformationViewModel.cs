using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ProblemInformationViewModel : ProblemInformationEntity
    {
        public string HappenDayString { get; set; }
        public string TypeOfIncidentName { get; set; }
        public string CompensationStatusName { get; set; }
    }
}
