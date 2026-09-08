using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class FamilyInformationViewModel : FamilyInformationEntity
    {
        public string RelationshipName { get; set; }
        public string DateOfBirthString { get; set; }
    }
}
