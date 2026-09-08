using NHIGIA.Core.Domain.Entity;
using System.Collections.Generic;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ProblemInformationRelatedStaffViewModel : ProblemInformationRelatedStaffEntity
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public string TheDecisionAutoComplete { get; set; }
        public string ProcessingStatusName { get; set; }
        public List<ProblemInformationRelatedStaffEntity> ListEmployeeId { get; set; }
    }
}
