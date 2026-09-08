using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class EvaluateViewModel : EvaluateEntity
    {
        public string EvaluationPeriodName { get; set; }
        public string EvaluationStatusName { get; set; }
        public string PersonInChargeName { get; set; }
    }
}
