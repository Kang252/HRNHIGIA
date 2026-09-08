using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class PageInformationViewModel : PageInformationEntity
    {
        public string DateRangeString { get; set; }
        public string ExpirationDateString { get; set; }
    }
}
