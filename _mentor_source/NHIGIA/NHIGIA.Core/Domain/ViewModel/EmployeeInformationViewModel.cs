using NHIGIA.Core.Domain.Entity;
using System;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class EmployeeInformationViewModel : EmployeeInformationEntity
    {
        public string SexName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public string WorkStatusName { get; set; }
        public string DegreeTrainingName { get; set; }
        public string TrainingPlacesName { get; set; }
        public string SpecializedName { get; set; }
        public DateTime? ProbationDay { get; set; }
        public DateTime? OfficialDate { get; set; }
        public string ContractTypeName { get; set; }
        public bool IsCheck { get; set; }
    }
}
