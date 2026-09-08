using NHIGIA.Core.Domain.Entity;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class EmployeesOnBusinessTripViewModel : EmployeesOnBusinessTripEntity
    {
        public string BrowsingStatusName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public string EmployeeApprovedName { get; set; }
        public string Proponent { get; set; }
        public string DayToString { get; set; }
        public string ReturnDateString { get; set; }
    }
}
