using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class SalaryHistoryInformationViewModel : SalaryHistoryInformationEntity
    {
        public string JobPositionName { get; set; }
        public string DateOfChangeString { get; set; }
    }
}
