using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class ResignationProceduresViewModel : ResignationProceduresEntity
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public string ProbationDayString { get; set; }
        public string MobilePhone { get; set; }
        public string CompanyEmail { get; set; }
        public string SomeContracts { get; set; }
        public string ContractTypeName { get; set; }
        public string EffectiveDateString { get; set; }
        public string ExpirationDateString { get; set; }
        public string DayOfftring { get; set; }
        public string ContractTermName { get; set; }
    }
}
