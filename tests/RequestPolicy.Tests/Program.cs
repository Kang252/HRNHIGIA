using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

// Pure authorization/accounting checks: no production SQL, HANET or external calls.
var count = 0;
void Check(string label, bool value) { if (!value) throw new Exception(label); Console.WriteLine("PASS " + label); count++; }
HrmUserAccountModel Actor(string role, int? dept=2, int id=10) => new() { Id=id, RoleCode=role, DepartmentId=dept, IsActive=true };
LeaveRequestModel Leave(string type="Nghỉ phép", string session="Cả ngày", string status="APPROVED") => new() {
    UserId=20, RoleCode="EMPLOYEE", DepartmentId=2, LeaveType=type, SessionCode=session, StatusCode=status,
    StartDate=new(2026,9,25), EndDate=new(2026,9,25) };
var pending = Leave(status:"PENDING_MANAGER");
Check("Manager approves department employee", LeaveRequestPolicy.CanApprove(Actor("MANAGER"), pending));
Check("Manager cannot approve another department", !LeaveRequestPolicy.CanApprove(Actor("MANAGER",3), pending));
Check("Unassigned manager cannot approve", !LeaveRequestPolicy.CanApprove(Actor("MANAGER",null), pending));
Check("Employee cannot approve", !LeaveRequestPolicy.CanApprove(Actor("EMPLOYEE"), pending));
Check("Even admin cannot self-approve", !LeaveRequestPolicy.CanApprove(Actor("ADMIN",2,20), pending));
pending.RoleCode="MANAGER";
Check("Manager cannot approve peer", !LeaveRequestPolicy.CanApprove(Actor("MANAGER"), pending));
Check("Director approves manager", LeaveRequestPolicy.CanApprove(Actor("DIRECTOR"), pending));
pending.RoleCode="EMPLOYEE"; pending.StatusCode="PENDING_HR";
Check("Manager cannot skip HR stage", !LeaveRequestPolicy.CanApprove(Actor("MANAGER"), pending));
foreach (var status in new[]{"APPROVED","REJECTED","CANCELLED"}) {
 pending.StatusCode=status; Check("Final state cannot be approved again: "+status, !LeaveRequestPolicy.CanApprove(Actor("ADMIN"),pending));
}
Check("Employee cannot submit for colleague", !LeaveRequestPolicy.CanCreateFor(Actor("EMPLOYEE"),Actor("EMPLOYEE",2,20)));
Check("Manager cannot submit for other department", !LeaveRequestPolicy.CanCreateFor(Actor("MANAGER"),Actor("EMPLOYEE",3,20)));
var inactive=Actor("EMPLOYEE",2,20);inactive.IsActive=false;
Check("Inactive employee cannot receive request", !LeaveRequestPolicy.CanCreateFor(Actor("ADMIN"),inactive));
var half=Leave(session:"Buổi sáng");
Check("Half day counts 0.5",LeaveRequestPolicy.UsedAnnualDays([half],[],2026)==.5m);
Check("Duplicate overlapping requests count once",LeaveRequestPolicy.UsedAnnualDays([half,half],[],2026)==.5m);
Check("Two halves count one day",LeaveRequestPolicy.UsedAnnualDays([half,Leave(session:"Buổi chiều")],[],2026)==1m);
foreach (var type in new[]{"Công tác/Ra ngoài","Yêu cầu hỗ trợ","Tạm ứng lương","Nghỉ ốm","Nghỉ thai sản"})
 Check(type+" does not spend annual leave",LeaveRequestPolicy.UsedAnnualDays([Leave(type)],[],2026)==0);
var oldSick=Leave();oldSick.Reason="Tiêu đề: nghỉ\nLoại: Nghỉ ốm đau / điều trị | Thời gian: 1 ngày";
Check("Legacy subtype remains nonannual",LeaveRequestPolicy.UsedAnnualDays([oldSick],[],2026)==0);
var oldAnnual=Leave();oldAnnual.Reason="Loại: Nghỉ phép năm | Thời gian: 1 ngày";
Check("Legacy annual subtype recognized",LeaveRequestPolicy.UsedAnnualDays([oldAnnual],[],2026)==1);
var span=Leave();span.EndDate=new(2026,9,27);
var weekday=new ScheduleModel {UserId=20,EffectiveFrom=new(2026,1,1),StatusCode="ACTIVE",WorkDaysMask=62,StartTime=new(8,0,0),EndTime=new(17,30,0)};
var saturday=new ScheduleModel {UserId=20,EffectiveFrom=new(2026,1,1),StatusCode="ACTIVE",WorkDaysMask=64,StartTime=new(8,0,0),EndTime=new(12,0,0)};
Check("Friday and half Saturday count 1.5, Sunday excluded",LeaveRequestPolicy.UsedAnnualDays([span],[weekday,saturday],2026)==1.5m);
var crossover=Leave();crossover.StartDate=new(2025,12,31);crossover.EndDate=new(2026,1,2);
Check("Cross-year leave clipped to selected year",LeaveRequestPolicy.UsedAnnualDays([crossover],[],2026)==2);
var draft=new CreateLeaveRequest {LeaveType="Nghỉ phép",StartDate=new(2026,9,25),EndDate=new(2026,9,25),SessionCode="Cả ngày",Reason=new string('a',2000)};
Check("Detailed request longer than old SQL limit accepted",LeaveRequestPolicy.Validate(draft)==null);
draft.EndDate=draft.StartDate.AddDays(-1);Check("Reverse dates rejected",LeaveRequestPolicy.Validate(draft)!=null);
draft.EndDate=draft.StartDate;draft.SessionCode="bad";Check("Invalid leave session rejected",LeaveRequestPolicy.Validate(draft)!=null);
var missingPayroll = PayrollStoredValues.Parse(null);
Check("Missing payroll details are unknown", missingPayroll.PersonalDeduction == null && missingPayroll.EmployerSocialInsurance == null);
var storedPayroll = PayrollStoredValues.Parse("{\"personalIncomeTax\":0,\"dependentCount\":0,\"insuranceSalary\":15000000,\"employerSocialInsurance\":2500000}");
Check("Explicit payroll zero preserved", storedPayroll.PersonalIncomeTax == 0 && storedPayroll.DependentCount == 0);
Check("Stored employer insurance preserved", storedPayroll.InsuranceSalary == 15000000 && storedPayroll.EmployerSocialInsurance == 2500000);
Check("Malformed payroll read is unknown", PayrollStoredValues.Parse("broken").PersonalIncomeTax == null);
var metadata = "{\"personalIncomeTax\":0,\"personalDeduction\":123,\"custom\":{\"note\":\"retain\"},\"advance\":999}";
var updated = PayrollStoredValues.UpdateComputedAmounts(metadata, 0, 20, 0, 1000);
using (var document = System.Text.Json.JsonDocument.Parse(updated)) {
 Check("Recalculation retains custom metadata", document.RootElement.GetProperty("custom").GetProperty("note").GetString() == "retain");
 Check("Recalculation retains recorded tax", document.RootElement.GetProperty("personalDeduction").GetDecimal() == 123 && document.RootElement.GetProperty("personalIncomeTax").GetDecimal() == 0);
 Check("Recalculation accepts explicit zero advance", document.RootElement.GetProperty("advance").GetDecimal() == 0);
 Check("Recalculation does not invent insurance", !document.RootElement.TryGetProperty("socialInsurance", out _));
}
var rejectedMetadata = false;
try { PayrollStoredValues.UpdateComputedAmounts("broken",0,0,0,0); } catch (System.Text.Json.JsonException) { rejectedMetadata = true; }
Check("Malformed payroll is not overwritten on save", rejectedMetadata);
Check("Incomplete insurance total stays unknown", new PayrollInsuranceItem { BhxhComp=100, BhytComp=20 }.TotalComp == null);
Console.WriteLine($"Passed {count} policy and payroll tests.");
