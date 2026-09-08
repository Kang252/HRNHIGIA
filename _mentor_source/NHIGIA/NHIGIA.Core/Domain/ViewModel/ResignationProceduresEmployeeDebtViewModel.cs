using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ResignationProceduresEmployeeDebtViewModel : ResignationProceduresEmployeeDebtEntity
    {
        public string FinishDayString { get; set; }
        public string NameOfTheDebtName { get; set; }
        public string AccomplishedString { get; set; }
    }
}
