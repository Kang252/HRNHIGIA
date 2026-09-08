using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class BonusInformationViewModel : BonusInformationEntity
    {
        public string DecisionDateString { get; set; }
        public string CommendationFormName { get; set; }
        public string StatusBonusName { get; set; }
        public string RewardReasonName { get; set; }
        public string ThePersonSignedTheDecisionName { get; set; }
        public string JobPositionName { get; set; }
        public string StatusName { get; set; }
        public string BonusValue { get; set; }
    }
}
