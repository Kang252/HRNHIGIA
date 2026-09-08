using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class EvaluateDetailViewModel : EvaluateDetailEntity
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string WorkUnitName { get; set; }
        public string JobPositionName { get; set; }
        public string Rank { get; set; }
        public string DateString { get; set; }
        public string EvaluationPeriodName { get; set; }
        public string NameOfAudit { get; set; }
    }
}
