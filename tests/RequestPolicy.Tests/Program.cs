using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
Check("Director cannot bypass short-leave manager stage", !LeaveRequestPolicy.CanApprove(Actor("DIRECTOR"), pending));
pending.RoleCode="EMPLOYEE"; pending.StatusCode="PENDING_HR";
Check("Manager cannot skip HR stage", !LeaveRequestPolicy.CanApprove(Actor("MANAGER"), pending));
Check("HR approves long leave", LeaveRequestPolicy.CanApprove(Actor("HR"), pending));
Check("Director approves long leave", LeaveRequestPolicy.CanApprove(Actor("DIRECTOR"), pending));
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
var threeDayDraft = new CreateLeaveRequest { LeaveType="Nghỉ phép",StartDate=new(2026,9,25),EndDate=new(2026,9,29),SessionCode="Cả ngày",Reason="Nghỉ 3 ngày làm việc" };
Check("Three workdays route to department manager", LeaveRequestPolicy.RequestedDays(threeDayDraft,[],20)==3m && LeaveRequestPolicy.InitialStatus(threeDayDraft,[],20,"EMPLOYEE")=="PENDING_MANAGER");
var fourDayDraft = new CreateLeaveRequest { LeaveType="Nghỉ phép",StartDate=new(2026,9,25),EndDate=new(2026,9,30),SessionCode="Cả ngày",Reason="Nghỉ 4 ngày làm việc" };
Check("More than three workdays route to HR or director", LeaveRequestPolicy.RequestedDays(fourDayDraft,[],20)==4m && LeaveRequestPolicy.InitialStatus(fourDayDraft,[],20,"EMPLOYEE")=="PENDING_HR");
Check("Manager leave routes to HR or director", LeaveRequestPolicy.InitialStatus(threeDayDraft,[],20,"MANAGER")=="PENDING_HR");
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
var manualPayroll = new PayrollManualEditRequest { BaseSalary=1000,ProductSalary=200,SalesSalary=300,KpiSalary=400,OtSalary=100,TotalAllowance=50,Bonus=25,SocialInsurance=80,HealthInsurance=15,UnemploymentInsurance=10,PersonalIncomeTax=20,Advance=100,Deduction=30 };
var manualJson = PayrollStoredValues.UpdateManualAmounts("{\"custom\":{\"retain\":true}}", manualPayroll);
using (var manualDocument = System.Text.Json.JsonDocument.Parse(manualJson)) {
 Check("Manual payroll edit preserves unrelated metadata", manualDocument.RootElement.GetProperty("custom").GetProperty("retain").GetBoolean());
 Check("Manual payroll edit stores product and sales separately", manualDocument.RootElement.GetProperty("productSalary").GetDecimal()==200 && manualDocument.RootElement.GetProperty("salesSalary").GetDecimal()==300);
 Check("Manual payroll edit calculates net amount", manualDocument.RootElement.GetProperty("netSalary").GetDecimal()==manualPayroll.NetSalary);
}
Check("Only accountant or admin edits payroll", PayrollPolicy.CanEdit("ACCOUNTANT") && PayrollPolicy.CanEdit("ADMIN") && !PayrollPolicy.CanEdit("HR") && !PayrollPolicy.CanEdit("DIRECTOR"));
Check("Only director or admin approves payroll", PayrollPolicy.CanApprove("DIRECTOR") && PayrollPolicy.CanApprove("ADMIN") && !PayrollPolicy.CanApprove("HR") && !PayrollPolicy.CanApprove("ACCOUNTANT"));
Check("Payroll due date skips weekend", PayrollPolicy.AddBusinessDays(new DateTime(2026,10,30),2)==new DateTime(2026,11,3));
Check("KPI month resolves exact range", KpiPeriodPolicy.TryResolve("MONTH","Tháng 02-2026",out var monthPeriod) && monthPeriod.Start==new DateTime(2026,2,1) && monthPeriod.End==new DateTime(2026,2,28));
Check("KPI quarter resolves exact range", KpiPeriodPolicy.TryResolve("QUARTER","Quý 4-2026",out var quarterPeriod) && quarterPeriod.Start==new DateTime(2026,10,1) && quarterPeriod.End==new DateTime(2026,12,31));
Check("KPI year resolves exact range", KpiPeriodPolicy.TryResolve("YEAR","2026",out var yearPeriod) && yearPeriod.Start==new DateTime(2026,1,1) && yearPeriod.End==new DateTime(2026,12,31));
Check("Incomplete insurance total stays unknown", new PayrollInsuranceItem { BhxhComp=100, BhytComp=20 }.TotalComp == null);
var stagingMissingGuard = false;
try { DatabaseConfiguration.Resolve(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> { ["ASPNETCORE_ENVIRONMENT"]="Staging", ["ConnectionStrings:MainConnectionString"]="Server=localhost;Database=NHIGIA_STAGING;User Id=test;Password=test;TrustServerCertificate=True" }).Build()); }
catch (InvalidOperationException) { stagingMissingGuard=true; }
Check("Staging requires expected database guard",stagingMissingGuard);
var stagingMismatch = false;
try { DatabaseConfiguration.Resolve(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> { ["ASPNETCORE_ENVIRONMENT"]="Staging", ["HRM_EXPECTED_DATABASE"]="NHIGIA_STAGING", ["ConnectionStrings:MainConnectionString"]="Server=localhost;Database=NHIGIA_PRODUCTION;User Id=test;Password=test;TrustServerCertificate=True" }).Build()); }
catch (InvalidOperationException) { stagingMismatch=true; }
Check("Staging rejects mismatched database",stagingMismatch);
var stagingConnection=DatabaseConfiguration.Resolve(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> { ["ASPNETCORE_ENVIRONMENT"]="Staging", ["HRM_EXPECTED_DATABASE"]="NHIGIA_STAGING", ["ConnectionStrings:MainConnectionString"]="Server=localhost;Database=NHIGIA_STAGING;User Id=test;Password=test;TrustServerCertificate=True" }).Build());
Check("Staging accepts exact expected database",stagingConnection.Contains("Initial Catalog=NHIGIA_STAGING",StringComparison.OrdinalIgnoreCase));
var apiCutoverConnection=DatabaseConfiguration.Resolve(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> { ["DB_CONNECTION_STRING"]="Server=localhost;Database=NHIGIA_API_CUTOVER;User Id=test;Password=test;TrustServerCertificate=True" }).Build());
Check("API connection variable is accepted by the portal",apiCutoverConnection.Contains("Initial Catalog=NHIGIA_API_CUTOVER",StringComparison.OrdinalIgnoreCase));
var localAuthentication = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>
{
    ["Authentication:Mode"] = "Local",
    ["Authentication:Keycloak:Enabled"] = "true"
}).Build();
Check("Local authentication mode overrides the legacy Keycloak flag", !HrmAuthenticationMode.UsesKeycloak(localAuthentication));
var keycloakAuthentication = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>
{
    ["Authentication:Mode"] = "Keycloak",
    ["Authentication:Keycloak:Enabled"] = "false"
}).Build();
Check("Keycloak authentication mode can re-enable SSO", HrmAuthenticationMode.UsesKeycloak(keycloakAuthentication));
var legacyAuthentication = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>
{
    ["Authentication:Keycloak:Enabled"] = "true"
}).Build();
Check("Missing authentication mode keeps the legacy flag compatible", HrmAuthenticationMode.UsesKeycloak(legacyAuthentication));
var invalidAuthenticationModeRejected = false;
try
{
    HrmAuthenticationMode.UsesKeycloak(new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string,string> { ["Authentication:Mode"] = "invalid" }).Build());
}
catch (InvalidOperationException)
{
    invalidAuthenticationModeRejected = true;
}
Check("Unknown authentication mode is rejected", invalidAuthenticationModeRejected);
Check("Local and Keycloak sessions use different cookies",
    HrmAuthenticationMode.CookieName(false) != HrmAuthenticationMode.CookieName(true));
var tlsOverrideConnection=DatabaseConfiguration.Resolve(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string> { ["DB_CONNECTION_STRING"]="Server=tenant.sql.example;Database=NHIGIA;User Id=test;Password=test;Encrypt=True;TrustServerCertificate=False", ["DB_TLS_SERVER_NAME"]="certificate.sql.example" }).Build());
Check("Portal supports a verified SQL certificate hostname override",new SqlConnectionStringBuilder(tlsOverrideConnection).HostNameInCertificate=="certificate.sql.example");
Console.WriteLine($"Passed {count} policy and payroll tests.");
