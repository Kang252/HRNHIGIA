using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class WorkProgressInformationViewModel : WorkProgressInformationEntity
    {
        public string StartDateEndDate { get; set; }
        public string WorkUnitName { get; set; }
        public string JobPositionName { get; set; }
        public string DirectManagementName { get; set; }
        public string IndirectManagementName { get; set; }
    }
}
