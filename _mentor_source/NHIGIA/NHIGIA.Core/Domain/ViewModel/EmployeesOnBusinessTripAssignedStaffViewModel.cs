using NHIGIA.Core.Domain.Entity;
using System.Collections.Generic;

namespace NHIGIA.Core.Domain.ViewModel
{
    public class EmployeesOnBusinessTripAssignedStaffViewModel : EmployeesOnBusinessTripAssignedStaffEntity
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobPositionName { get; set; }
        public string WorkUnitName { get; set; }
        public string MobilePhone { get; set; }
        public string CompanyEmail { get; set; }
        public List<EmployeesOnBusinessTripAssignedStaffEntity> ListEmployeeId { get; set; }
    }
}
