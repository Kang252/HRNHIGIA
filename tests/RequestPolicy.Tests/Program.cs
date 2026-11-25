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
var monthKpi = new WorkItem { Kind="kpi", KpiType="ASSIGNED", KpiPeriodType="MONTH", Quarter="Tháng 11-2026", PeriodStart=new(2026,11,1), PeriodEnd=new(2026,11,30) };
var defaultFilter = new KpiFilterState { PeriodType="QUARTER", FromQuarter="Quý 4-2026", ToQuarter="Quý 4-2026", KpiType="ASSIGNED" }.Showing(monthKpi);
Check("Saved month KPI switches the filter to its month", defaultFilter.PeriodType=="MONTH" && defaultFilter.FromQuarter=="Tháng 11-2026" && defaultFilter.ToQuarter=="Tháng 11-2026");
var rangeFilter = new KpiFilterState { PeriodType="MONTH", FromQuarter="Tháng 09-2026", ToQuarter="Tháng 12-2026", KpiType="PLAN", Query="cũ" }.Showing(monthKpi);
Check("Filter already showing the KPI keeps its range", rangeFilter.FromQuarter=="Tháng 09-2026" && rangeFilter.ToQuarter=="Tháng 12-2026" && rangeFilter.KpiType=="ASSIGNED" && rangeFilter.Query==null);
var outsideFilter = new KpiFilterState { PeriodType="MONTH", FromQuarter="Tháng 01-2026", ToQuarter="Tháng 02-2026" }.Showing(monthKpi);
Check("Filter outside the KPI period moves to it", outsideFilter.FromQuarter=="Tháng 11-2026" && outsideFilter.ToQuarter=="Tháng 11-2026");
var yearFilter = new KpiFilterState().Showing(new WorkItem { Kind="kpi", KpiType="PLAN", KpiPeriodType="YEAR", Quarter="2026", PeriodStart=new(2026,1,1), PeriodEnd=new(2026,12,31) });
Check("Saved year plan KPI opens the year and plan tab", yearFilter.PeriodType=="YEAR" && yearFilter.FromQuarter=="2026" && yearFilter.KpiType=="PLAN");
Check("Half-day request is shown as half a workday", LeaveRequestPolicy.RequestedDays(Leave(session:"Buổi sáng"),[])==.5m);
Check("Non-leave session keeps calendar days", LeaveRequestPolicy.RequestedDays(Leave("Tạm ứng lương","Chuyển khoản"),[])==null);
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

// --- leave kinds, balance and overlap
foreach (var (subtype, expected) in new[]
{
    ("Nghỉ phép năm","Nghỉ phép"), ("Nghỉ không hưởng lương","Nghỉ không lương"), ("Nghỉ ốm đau / điều trị","Nghỉ ốm"),
    ("Nghỉ thai sản","Nghỉ thai sản"), ("Nghỉ việc riêng (hưởng lương)","Nghỉ việc riêng"), ((string)null,"Nghỉ phép")
}) Check($"Leave form choice '{subtype}' is stored as {expected}", LeaveRequestPolicy.NormalizeLeaveType("Nghỉ phép", subtype) == expected);
Check("Older request keeps the kind written in its reason", LeaveRequestPolicy.NormalizeLeaveType("Nghỉ phép", null, "Tiêu đề: nghỉ\nLoại: Nghỉ không hưởng lương | Thời gian: 1 ngày") == "Nghỉ không lương");
Check("Empty kind in an older reason does not fail", LeaveRequestPolicy.NormalizeLeaveType("Nghỉ phép", null, "Loại: ") == "Nghỉ phép");
Check("Other procedures keep their type", LeaveRequestPolicy.NormalizeLeaveType("Suất ăn", "Nghỉ không hưởng lương") == "Suất ăn");
CreateLeaveRequest Ask(string type, DateTime from, DateTime to, string session="Cả ngày") => new() { LeaveType=type, StartDate=from, EndDate=to, SessionCode=session, Reason="Kiểm thử" };
var friday = new DateTime(2026,9,25);
Check("Weekend-only request is refused", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ phép",friday.AddDays(1),friday.AddDays(2)),20,12,[],[weekday])?.Contains("không có ngày làm việc")==true);
Check("Overlapping request is refused", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ ốm",friday,friday),20,12,[Leave(status:"PENDING_MANAGER")],[weekday])?.Contains("trùng thời gian")==true);
Check("Morning and afternoon of one day do not overlap", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ phép",friday,friday,"Buổi chiều"),20,12,[half],[weekday])==null);
Check("Request above the balance is refused", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ phép",friday.AddDays(3),friday.AddDays(5)),20,2,[],[weekday])?.Contains("còn 2 ngày")==true);
Check("Waiting requests hold their days", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ phép",friday.AddDays(3),friday.AddDays(4)),20,2,[Leave(status:"PENDING_MANAGER")],[weekday])?.Contains("còn 1 ngày")==true);
Check("Unpaid leave does not need a balance", LeaveRequestPolicy.ValidateAgainst(Ask("Nghỉ không lương",friday.AddDays(3),friday.AddDays(7)),20,0,[],[weekday])==null);
Check("Non-leave procedure is not checked against the calendar", LeaveRequestPolicy.ValidateAgainst(Ask("Suất ăn",friday.AddDays(1),friday.AddDays(1),"Cơm trưa"),20,0,[],[weekday])==null);

// --- approval inbox
Check("Leave is routine", ApprovalInbox.From(Leave()).IsRoutine);
Check("Late arrival is routine and shows its minutes", ApprovalInbox.From(new LeaveRequestModel { LeaveType="Đi muộn về sớm", AdjustKind="LATE", AdjustMinutes=25, StartDate=friday, EndDate=friday }) is { IsRoutine: true } late && late.Facts.Any(x => x.Contains("đi muộn 25 phút")));
Check("Salary advance is decided one by one and shows its amount", ApprovalInbox.From(new LeaveRequestModel { LeaveType="Tạm ứng lương", Amount=5_000_000, StartDate=friday, EndDate=friday }) is { IsRoutine: false } advance && advance.Facts.Contains("Số tiền 5.000.000 đ"));
foreach (var kind in new[] { "payroll", "resignation", "offboarding", "transfer" })
 Check($"{kind} is never approved in bulk", !ApprovalInbox.From(new WorkItem { Kind=kind }).IsRoutine);
var payrollCards = ApprovalInbox.FromWork([new WorkItem { Id=1, Kind="payroll", Quarter="2026-11", Actual=10_000_000 }, new WorkItem { Id=2, Kind="payroll", Quarter="2026-11", Actual=5_500_000 }, new WorkItem { Id=3, Kind="overtime" }]).ToList();
Check("A payroll period is one card with its total", payrollCards.Count==2 && payrollCards.Single(x => x.Kind=="payroll") is { IsRoutine: false, When: "2 nhân viên" } payrollCard && payrollCard.Facts.Contains("Tổng thực nhận 15.500.000 đ") && payrollCard.Url.EndsWith("pyPeriod=2026-11"));
var overtimeCard = ApprovalInbox.From(new WorkItem { Kind="overtime", Category=OvertimePolicy.WeekendCategory, Target=3.5m, Weight=2, StartAt=new(2026,9,26,8,0,0), EndAt=new(2026,9,26,11,30,0) });
Check("Overtime card shows date, hours and rate", overtimeCard.When=="26/09/2026 · 08:00–11:30" && overtimeCard.Facts.Contains("3,5 giờ") && overtimeCard.Facts.Any(x => x.EndsWith("hệ số 2")));

// --- overtime rates
var rates = new OvertimeRates();
Check("Fixed holidays are recognized", OvertimePolicy.IsFixedHoliday(new(2026,9,2)) && !OvertimePolicy.IsFixedHoliday(new(2026,9,3)));
Check("Holiday wins over the declared category", OvertimePolicy.ResolveCategory(OvertimePolicy.EveningCategory,new(2026,4,30),true)==OvertimePolicy.HolidayCategory);
Check("Rest day becomes weekend overtime", OvertimePolicy.ResolveCategory(OvertimePolicy.NightCategory,new(2026,9,26),false)==OvertimePolicy.WeekendCategory);
var contradicted=false; try { OvertimePolicy.ResolveCategory(OvertimePolicy.WeekendCategory,friday,true); } catch (InvalidOperationException) { contradicted=true; }
Check("Weekend rate cannot be claimed on a workday", contradicted);
Check("Planned rates follow the day", OvertimePolicy.PlannedRate(OvertimePolicy.EveningCategory,rates)==1.5m && OvertimePolicy.PlannedRate(OvertimePolicy.WeekendCategory,rates)==2m && OvertimePolicy.PlannedRate(OvertimePolicy.HolidayCategory,rates)==3m && OvertimePolicy.PlannedRate(OvertimePolicy.NightCategory,rates)==1.8m);
Check("Night part of a window gets the night extra", OvertimePolicy.WeightedMinutes(new(180,60),OvertimeDayType.Workday,rates)==120*1.5m+60*1.8m);
Check("Workday follows the assigned schedule", !OvertimePolicy.IsWorkday([saturday],friday) && OvertimePolicy.IsWorkday([weekday],friday) && OvertimePolicy.IsWorkday([],friday));

// --- accounts
Check("HR manages staff accounts only", AccountPolicy.AssignableRoles("HR").SequenceEqual(new[]{"EMPLOYEE","MANAGER","ACCOUNTANT"}) && !AccountPolicy.CanManage(Actor("HR",2,1),Actor("DIRECTOR",2,2)) && !AccountPolicy.CanManage(Actor("HR",2,1),Actor("ADMIN",2,2)));
Check("Nobody manages their own account", !AccountPolicy.CanManage(Actor("ADMIN",2,1),Actor("ADMIN",2,1)));
Check("Managers and employees manage no accounts", AccountPolicy.AssignableRoles("MANAGER").Count==0 && AccountPolicy.AssignableRoles("EMPLOYEE").Count==0);
Check("Usernames are plain", AccountPolicy.ValidateUsername("nguyen.van-a_1")==null && AccountPolicy.ValidateUsername("Nguyễn Văn A")!=null && AccountPolicy.ValidateUsername("ab")!=null);
Check("Weak passwords are refused", AccountPolicy.ValidatePassword("short1","user")!=null && AccountPolicy.ValidatePassword("onlyletters","user")!=null && AccountPolicy.ValidatePassword("1234567890","user")!=null && AccountPolicy.ValidatePassword("my-user-2026x","my-user")!=null);
Check("Temporary passwords pass the rule and differ", Enumerable.Range(0,50).Select(_ => AccountPolicy.TemporaryPassword()).ToList() is var issued && issued.All(x => AccountPolicy.ValidatePassword(x,"someone")==null) && issued.Distinct().Count()==50);
var throttle = new LoginThrottle(new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()));
for (var i=0;i<LoginThrottle.MaxPerAccount;i++) { Check($"Attempt {i+1} is allowed", throttle.RetryAfter("user","1.1.1.1")==null); throttle.Fail("user","1.1.1.1"); }
Check("Repeated failures are blocked", throttle.RetryAfter(" USER ","1.1.1.1")>TimeSpan.Zero);
Check("The same account still signs in from elsewhere", throttle.RetryAfter("user","2.2.2.2")==null);
throttle.Succeed("user","1.1.1.1");
Check("A successful sign-in clears the account counter", throttle.RetryAfter("user","1.1.1.1")==null);
for (var i=0;i<LoginThrottle.MaxPerAddress;i++) throttle.Fail("other"+i,"3.3.3.3");
Check("Guessing many accounts from one address is blocked", throttle.RetryAfter("never-tried","3.3.3.3")>TimeSpan.Zero);

// --- profile format
IEnumerable<string> Errors(EmployeeProfileModel input, EmployeeProfileModel stored=null) => EmployeeProfilePolicy.Validate(input,stored,new(2026,11,25)).Select(x => x.Field);
Check("Garbage profile values are refused", Errors(new() { IdentityNumber="abc", MobilePhone="123", PersonalTaxCode="x", SocialInsuranceNumber="1", BankAccountNumber="tk", BasicSalary="nhiều", DateOfBirth=new(2026,1,1) }).Count()==7);
Check("Normal profile values are accepted", !Errors(new() { IdentityNumber="079 123 456 789", MobilePhone="+84 901 234 567", PersonalTaxCode="0312345678-001", SocialInsuranceNumber="7912345678", BankAccountNumber="0071001234567", BasicSalary="15.000.000 đ", DateOfBirth=new(1990,5,1) }).Any());
Check("A value stored earlier does not block other edits", !Errors(new() { IdentityNumber="cũ", MobilePhone="chưa có" }, new() { IdentityNumber="cũ", MobilePhone="chưa có" }).Any());
Check("Money is grouped the Vietnamese way", 1458951350m.ToString("N0",VietnamFormat.Numbers)=="1.458.951.350" && 1.5m.ToString("0.##",VietnamFormat.Numbers)=="1,5");
Console.WriteLine($"Passed {count} policy and payroll tests.");
