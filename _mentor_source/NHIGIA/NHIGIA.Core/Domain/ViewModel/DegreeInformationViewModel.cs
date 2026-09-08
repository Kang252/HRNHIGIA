using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class DegreeInformationViewModel : DegreeInformationEntity
    {
        public string TrainingTime { get; set; }
        public string TrainingPlacesName { get; set; }
        public string SpecializedName { get; set; }
        public string DegreeTrainingName { get; set; }
        public string ClassificationName { get; set; }
    }
}
