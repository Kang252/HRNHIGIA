using NHIGIA.Core.Domain.Entity;
using System.Collections.Generic;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class BonusInformationStaffWereCommendedViewModel : BonusInformationStaffWereCommendedEntity
    {
        public string EmployeeName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public List<BonusInformationStaffWereCommendedEntity> ListEmployeeId { get; set; }
    }
}
