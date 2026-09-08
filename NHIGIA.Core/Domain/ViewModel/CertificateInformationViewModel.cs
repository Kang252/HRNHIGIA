using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class CertificateInformationViewModel : CertificateInformationEntity
    {
        public string CertificateGroupName { get; set; }
        public string DateRangeString { get; set; }
        public string ExpirationDateString { get; set; }
        public string ClassificationName { get; set; }
    }
}
