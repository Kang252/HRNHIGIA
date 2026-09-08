using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class QuitInformationViewModel : QuitInformationEntity
    {
        public string ProcedureGroupQuitName { get; set; }
        public string FinishDayString { get; set; }
        public string AccomplishedString { get; set; }
    }
}
