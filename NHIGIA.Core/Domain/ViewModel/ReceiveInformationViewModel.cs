using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ReceiveInformationViewModel : ReceiveInformationEntity
    {
        public string ProcedureGroupReceiveName { get; set; }
        public string FinishDayString { get; set; }
        public string AccomplishedString { get; set; }
    }
}
