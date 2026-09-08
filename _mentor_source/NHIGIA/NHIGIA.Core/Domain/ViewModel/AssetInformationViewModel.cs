using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class AssetInformationViewModel : AssetInformationEntity
    {
        public string AssetTypeName { get; set; }
        public string ReceivedDateString { get; set; }
        public string AssetStatusName { get; set; }
        public string PayDayString { get; set; }
    }
}
