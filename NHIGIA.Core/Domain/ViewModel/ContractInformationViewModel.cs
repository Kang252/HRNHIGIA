using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ContractInformationViewModel : ContractInformationEntity
    {
        public string ExpirationDateString { get; set; }
        public string ContractTypeName { get; set; }
        public string ContractTermName { get; set; }
        public string JobPositionName { get; set; }
        public string ExpirationDateValue { get; set; }
    }
}
