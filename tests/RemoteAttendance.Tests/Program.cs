using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

var now = DateTimeOffset.UtcNow;
var day = now.ToOffset(TimeSpan.FromHours(7)).Date;
var employee = new HrmUserAccountModel { Id=1,RoleCode=HrmRoles.Employee,DepartmentId=1,IsActive=true };
var manager = new HrmUserAccountModel { Id=2,RoleCode=HrmRoles.Manager,DepartmentId=1,IsActive=true };
RemoteWorkPlan Plan() => new() { UserId=1,Mode="FIELD",PlaceName="Tuyến kiểm thử",FromDate=day,ToDate=day,WorkDaysMask=127,WindowStart=TimeSpan.Zero,WindowEnd=new TimeSpan(23,59,59),RequiredMinutes=480,BreakMinutes=60,StatusCode="APPROVED" };
RemotePunchRequest Punch() => new() { ClientId=Guid.NewGuid(),OwnerUserId=1,Kind="IN",CapturedAt=now,Latitude=10.77,Longitude=106.69,AccuracyMeters=10,PlaceName="Điểm thử",Note="Giải trình thử" };
void Check(bool condition,string name) { if(!condition) throw new Exception(name); Console.WriteLine("PASS " + name); }
void Reject(Action action,string name) { try { action(); } catch(InvalidOperationException) { Console.WriteLine("PASS "+name); return; } throw new Exception("Expected rejection: "+name); }
void Deny(Action action,string name) { try { action(); } catch(UnauthorizedAccessException) { Console.WriteLine("PASS "+name); return; } throw new Exception("Expected denial: "+name); }

Check(!RemoteAttendancePolicy.CanReview(employee,1,1,HrmRoles.Employee),"self approval forbidden");
Check(RemoteAttendancePolicy.CanReview(manager,1,1,HrmRoles.Employee),"manager reviews department employee");
Check(!RemoteAttendancePolicy.CanReview(manager,1,2,HrmRoles.Employee),"cross department denied");
Check(!RemoteAttendancePolicy.CanReview(manager,3,1,HrmRoles.Manager),"manager cannot review peer manager");
Check(!RemoteAttendancePolicy.CanReview(new(){Id=2,RoleCode=HrmRoles.Manager,IsActive=true},1,null,HrmRoles.Employee),"unassigned manager denied");
var home=Plan();home.Mode="HOME";
Reject(()=>RemoteAttendancePolicy.ValidatePlan(home),"WFH requires approved coordinates");
home.Latitude=10.77; home.Longitude=106.69; home.RadiusMeters=200;
RemoteAttendancePolicy.ValidatePlan(home);
Check(RemoteAttendancePolicy.Evaluate(home,Punch(),now).Reason=="","inside home geofence accepted");
var outside=Punch();outside.Latitude=10.78;
Check(RemoteAttendancePolicy.Evaluate(home,outside,now).Reason.Contains("Ngoài vùng"),"outside geofence goes to review");
var weak=Punch();weak.AccuracyMeters=201;
Check(RemoteAttendancePolicy.Evaluate(home,weak,now).Reason.Contains("GPS"),"weak GPS and crossing boundary reviewed");
var fresh=Punch();fresh.CapturedAt=now.AddSeconds(-5);
Check(RemoteAttendancePolicy.Evaluate(Plan(),fresh,now).CheckTime==now.ToOffset(TimeSpan.FromHours(7)).DateTime,"online timestamp comes from server");
var delayed=Punch();delayed.CapturedAt=now.AddHours(-1);
Check(RemoteAttendancePolicy.Evaluate(Plan(),delayed,now).Reason.Contains("Đồng bộ trễ"),"cannot bypass offline review by claiming online");
var missing=Punch();missing.Latitude=null;missing.Longitude=null;missing.AccuracyMeters=null;
Check(RemoteAttendancePolicy.Evaluate(Plan(),missing,now).Reason.Contains("Thiếu vị trí"),"missing GPS retained for review");
missing.Note="";Reject(()=>RemoteAttendancePolicy.Evaluate(Plan(),missing,now),"GPS exception requires explanation");
var future=Punch();future.CapturedAt=now.AddMinutes(6);Reject(()=>RemoteAttendancePolicy.Evaluate(Plan(),future,now),"future timestamp rejected");
future.CapturedAt=now.AddDays(-8);Reject(()=>RemoteAttendancePolicy.Evaluate(Plan(),future,now),"offline records over 7 days rejected");
Reject(()=>RemoteAttendancePolicy.ValidateCoordinates(double.NaN,106),"nonfinite coordinates rejected");
var invalidPlan=Plan();invalidPlan.WindowStart=TimeSpan.FromHours(22);invalidPlan.WindowEnd=TimeSpan.FromHours(8);Reject(()=>RemoteAttendancePolicy.ValidatePlan(invalidPlan),"overnight remote windows rejected explicitly");
AttendanceRecordModel Row() => new() {UserId=1,WorkDate=day,ScheduledStart=TimeSpan.FromHours(8),ScheduledEnd=TimeSpan.FromHours(22),IsFlexible=true,RequiredMinutes=480,BreakMinutes=60,CheckIn=day.AddHours(9),CheckOut=day.AddHours(18),LastSeen=day.AddHours(18),EventCount=2,HasExplicitPunches=true};
AttendanceRecordModel Project(AttendanceRecordModel row) => AttendanceLeavePolicy.Project([row],[],[],day,day,day.AddDays(1)).Single();
Check(Project(Row()).WorkedMinutes==480 && Project(Row()).StatusCode=="ON_TIME","flex shift deducts break and ignores fixed late rule");
var shortRow=Row();shortRow.CheckOut=day.AddHours(17);Check(Project(shortRow).StatusCode=="INSUFFICIENT_HOURS","flex short duration detected");
var repeatedIn=Row();repeatedIn.IsFlexible=false;repeatedIn.CheckOut=null;Check(Project(repeatedIn).StatusCode=="MISSING_CHECK" && Project(repeatedIn).CheckOut==null,"two INs do not manufacture checkout");
var onlyOut=Row();onlyOut.CheckIn=null;Check(Project(onlyOut).StatusCode=="MISSING_CHECK","OUT alone does not manufacture checkin");
if(!args.Contains("--integration")) return;

// Dedicated disposable database on LOCALDB only. Never reads production configuration.
var database="NHIGIA_RemoteTests_"+Guid.NewGuid().ToString("N");
var masterString="Server=(localdb)\\NHIGIA;Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=15";
using var master=new SqlConnection(masterString);master.Open();master.Execute($"CREATE DATABASE [{database}]");
var testString=new SqlConnectionStringBuilder(masterString){InitialCatalog=database}.ConnectionString;
var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HRM_CONNECTION_STRING",testString}}).Build();
var root=Path.GetFullPath("NHIGIA.Modern");
var artifactPath=Path.Combine(Path.GetTempPath(),database);Directory.CreateDirectory(artifactPath);
var protection=DataProtectionProvider.Create(new DirectoryInfo(artifactPath),b=>b.SetApplicationName("NHIGIA.Modern.v1"));
var store=new HrmDataStore(config,protection);
Process? host=null;
try
{
    store.EnsureSchema(Path.Combine(root,"App_Data/hrm-mvp.sql"));
    store.EnsureSchema(Path.Combine(root,"App_Data/remote-attendance.sql"));
    store.EnsureSchema(Path.Combine(root,"App_Data/remote-attendance.sql"));
    store.EnsureSchema(Path.Combine(root,"App_Data/face-enrollment.sql"));
    store.EnsureSchema(Path.Combine(root,"App_Data/face-enrollment.sql"));
    using var db=new SqlConnection(testString);db.Open();
    var dep=db.ExecuteScalar<int>("INSERT dbo.HrmDepartment(Code,Name) OUTPUT INSERTED.Id VALUES('TEST_REMOTE',N'Test remote')");
    int AddUser(string username,string role,int? department) => db.ExecuteScalar<int>("INSERT dbo.HrmUserAccount(Username,DisplayName,PasswordHash,PasswordSalt,RoleCode,DepartmentId) OUTPUT INSERTED.Id VALUES(@username,@username,'unused','unused',@role,@department)",new{username,role,department});
    var owner=store.FindUser(AddUser("remote_test_employee",HrmRoles.Employee,dep));
    var reviewer=store.FindUser(AddUser("remote_test_manager",HrmRoles.Manager,dep));
    var outsider=store.FindUser(AddUser("remote_test_outsider",HrmRoles.Employee,null));
    var hr=store.FindUser(AddUser("remote_test_hr",HrmRoles.Hr,null));
    var browserFaceOwner=store.FindUser(AddUser("face_browser_employee",HrmRoles.Employee,dep));
    var plan=Plan();plan.FromDate=day.AddDays(-1);plan.ToDate=day.AddDays(1);plan.WindowStart=TimeSpan.FromHours(8);plan.WindowEnd=TimeSpan.FromHours(22);plan.IsFlexible=true;
    var planId=store.CreateRemotePlan(plan,owner,"127.0.0.1");
    Reject(()=>store.DecideRemotePlan(planId,true,"self",owner,"127.0.0.1"),"SQL self approval blocked");
    store.DecideRemotePlan(planId,true,"Lịch phù hợp",reviewer,"127.0.0.1");
    Check(store.GetRemotePlans(outsider,day.AddDays(-1),day.AddDays(1)).Count==0,"SQL plans scoped to account");
    var from=day.AddDays(-1).AddHours(9);var to=day.AddDays(-1).AddHours(18);
    RemotePunchRequest DbPunch(string kind,DateTime date) => new(){ClientId=Guid.NewGuid(),OwnerUserId=owner.Id,PlanId=planId,Kind=kind,CapturedAt=new DateTimeOffset(date,TimeSpan.FromHours(7)),WasOffline=true,PlaceName="Khách hàng thử",Latitude=10.77,Longitude=106.69,AccuracyMeters=10,Note="Kiểm thử offline"};
    var input=DbPunch("IN",from);var photo=new byte[]{255,216,255,217};
    Check(store.GetFaceEnrollmentState(owner).Enrollment==null,"new employee has no face enrollment");
    Reject(()=>store.RecordRemotePunch(input,photo,owner,"127.0.0.1"),"remote IN requires active face enrollment");
    Reject(()=>store.RecordRemotePunch(DbPunch("OUT",to),photo,owner,"127.0.0.1"),"remote OUT requires active face enrollment");
    Reject(()=>store.SubmitFaceEnrollment(photo,false,owner,"127.0.0.1"),"face enrollment requires explicit consent");
    Reject(()=>store.SubmitFaceEnrollment([1,2,3,4],true,owner,"127.0.0.1"),"face enrollment rejects invalid image");
    var enrollmentId=store.SubmitFaceEnrollment(photo,true,owner,"127.0.0.1");
    var faceState=store.GetFaceEnrollmentState(owner);
    Check(faceState.Enrollment?.StatusCode=="PENDING" && faceState.Enrollment.CreatedAt.Kind==DateTimeKind.Utc && !faceState.CanReview && faceState.Pending.Count==0,"enrollment awaits approval and exposes UTC dates");
    Check(!db.QuerySingle<byte[]>("SELECT PhotoProtected FROM dbo.HrmFaceEnrollment WHERE Id=@Id",new{Id=enrollmentId}).SequenceEqual(photo),"reference photo is encrypted at rest");
    Check(store.GetFaceEnrollmentPhoto(enrollmentId,owner).SequenceEqual(photo) && store.GetFaceEnrollmentPhoto(enrollmentId,hr).SequenceEqual(photo),"owner and HR can decrypt reference");
    Check(store.GetFaceEnrollmentPhoto(enrollmentId,outsider)==null && store.GetFaceEnrollmentPhoto(enrollmentId,reviewer)==null,"reference directory is hidden from unrelated employee and manager");
    Check(store.GetFaceEnrollmentState(hr).Pending.Any(x=>x.Id==enrollmentId && x.DisplayName==owner.DisplayName),"HR receives pending enrollment");
    Deny(()=>store.ReviewFaceEnrollment(enrollmentId,true,"manager attempt",reviewer,"127.0.0.1"),"manager cannot approve identity registration");
    Reject(()=>store.SubmitFaceEnrollment(photo,true,owner,"127.0.0.1"),"duplicate pending enrollment refused");
    Reject(()=>store.RecordRemotePunch(input,photo,owner,"127.0.0.1"),"pending face enrollment does not enable attendance");
    var hrEnrollmentId=store.SubmitFaceEnrollment(photo,true,hr,"127.0.0.1");
    Reject(()=>store.ReviewFaceEnrollment(hrEnrollmentId,true,"self",hr,"127.0.0.1"),"HR cannot self approve face");
    Check(!store.GetFaceEnrollmentState(hr).Pending.Any(x=>x.Id==hrEnrollmentId),"HR own pending enrollment omitted from review queue");
    store.ReviewFaceEnrollment(enrollmentId,true,"Đã đối chiếu hồ sơ nhân sự",hr,"127.0.0.1");
    Check(store.GetFaceEnrollmentState(owner).Enrollment?.StatusCode=="ACTIVE","HR approval activates reference");
    Reject(()=>store.SubmitFaceEnrollment(photo,true,owner,"127.0.0.1"),"active reference cannot be silently replaced");
    Reject(()=>store.RecordRemotePunch(input,photo,owner,"127.0.0.1"),"capture before reference approval is refused");
    // Historical attendance fixture only: this isolated database's reference predates yesterday's captures.
    db.Execute("UPDATE dbo.HrmFaceEnrollment SET ReviewedAt=@ReviewedAt WHERE Id=@Id",new{Id=enrollmentId,ReviewedAt=new DateTimeOffset(from.AddHours(-1),TimeSpan.FromHours(7)).UtcDateTime});
    store.RecordRemotePunch(input,photo,owner,"127.0.0.1");store.RecordRemotePunch(input,photo,owner,"127.0.0.1");
    Check(db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.HrmRemotePunch WHERE ClientId=@ClientId",input)==1,"SQL retry idempotent");
    var pending=store.GetRemotePunches(owner,from.Date,to.Date).Single();
    Check(pending.StatusCode=="PENDING" && pending.FaceEnrollmentId==enrollmentId && pending.FaceVerificationStatus=="MANUAL_REQUIRED","punch links approved identity but never fabricates face AI result");
    Check(store.GetAttendance(owner,from.Date,to.Date).Count==0,"pending not counted in monthly attendance");
    Check(store.GetRemotePhoto(pending.Id,outsider)==null,"photo scoped to owner/reviewer");
    Reject(()=>store.DecideRemotePunch(pending.Id,true,"self",owner,"127.0.0.1"),"SQL own punch review forbidden");
    store.DecideRemotePunch(pending.Id,true,"Đối chiếu ảnh/GPS",reviewer,"127.0.0.1");
    Check(store.GetAttendance(owner,from.Date,to.Date).Single().CheckOut==null,"approved IN alone has no checkout");
    store.RecordRemotePunch(DbPunch("VISIT",from.AddHours(1)),photo,owner,"127.0.0.1");
    var visit=store.GetRemotePunches(owner,from.Date,to.Date).Single(x=>x.Kind=="VISIT");store.DecideRemotePunch(visit.Id,true,"Điểm bán",reviewer,"127.0.0.1");
    Check(store.GetAttendance(owner,from.Date,to.Date).Single().EventCount==1,"visit evidence excluded from attendance punches");
    store.RecordRemotePunch(DbPunch("OUT",to),photo,owner,"127.0.0.1");
    var output=store.GetRemotePunches(owner,from.Date,to.Date).Single(x=>x.Kind=="OUT");
    Check(store.GetAttendancePeriod(from.ToString("yyyy-MM"),hr).PendingRemoteCount==1,"period counts pending remote punches");
    store.ConfirmAttendancePeriod(from.ToString("yyyy-MM"),owner,"127.0.0.1");
    store.DecideRemotePunch(output.Id,true,"Đối chiếu cuối ngày",reviewer,"127.0.0.1");
    Check(!store.GetAttendancePeriod(from.ToString("yyyy-MM"),owner).IsConfirmed,"new approved evidence invalidates old employee confirmation");
    var actual=store.GetAttendance(owner,from.Date,to.Date).Single();
    Check(actual.CheckIn==from && actual.CheckOut==to && actual.WorkedMinutes==480 && actual.Source=="GPS + Photo","SQL projection uses typed punches and flexible plan");
    store.SetAttendancePeriodLock(from.ToString("yyyy-MM"),true,"test lock",true,hr,"127.0.0.1");
    Reject(()=>store.RecordRemotePunch(DbPunch("IN",from.AddMinutes(1)),photo,owner,"127.0.0.1"),"locked period refuses late upload");
    store.RecordRemotePunch(input,photo,owner,"127.0.0.1");
    Check(store.GetAttendance(owner,from.Date,to.Date).Single().WorkedMinutes==480,"locked period snapshot and idempotent retry stable");
    store.SetAttendancePeriodLock(from.ToString("yyyy-MM"),false,"test unlock",false,hr,"127.0.0.1");
    store.SaveAttendanceEvent(new HanetWebhookEvent{EventKey="remote-test-mixed",PersonId="unmapped",CheckTime=from.AddMinutes(-10),PayloadJson="{}"});
    db.Execute("UPDATE dbo.HrmAttendanceEvent SET UserId=@Id WHERE EventKey='remote-test-mixed'",new{owner.Id});
    Check(store.GetAttendance(owner,from.Date,to.Date).Single().Source=="HANET + GPS","HANET and remote merged without duplicate day");
    var forged=DbPunch("IN",from);forged.OwnerUserId=outsider.Id;
    Reject(()=>store.RecordRemotePunch(forged,photo,owner,"127.0.0.1"),"queue cannot be submitted as another user");

    // Valid real-time GPS still needs manual identity verification while no biometric provider is configured.
    var liveOwner=store.FindUser(AddUser("face_live_employee",HrmRoles.Employee,dep));
    var livePlan=Plan();var livePlanId=store.CreateRemotePlan(livePlan,liveOwner,"127.0.0.1");
    store.DecideRemotePlan(livePlanId,true,"Test live shift",reviewer,"127.0.0.1");
    var liveEnrollment=store.SubmitFaceEnrollment(photo,true,liveOwner,"127.0.0.1");
    store.ReviewFaceEnrollment(liveEnrollment,true,"Test verified identity",hr,"127.0.0.1");
    var livePunch=new RemotePunchRequest { ClientId=Guid.NewGuid(),OwnerUserId=liveOwner.Id,PlanId=livePlanId,Kind="IN",CapturedAt=DateTimeOffset.UtcNow,
        Latitude=10.77,Longitude=106.69,AccuracyMeters=10,PlaceName="Điểm GPS đúng",Note="" };
    store.RecordRemotePunch(livePunch,photo,liveOwner,"127.0.0.1");
    var livePending=store.GetRemotePunches(liveOwner,day,day).Single();
    Check(livePending.StatusCode=="PENDING" && livePending.FaceVerificationStatus=="MANUAL_REQUIRED","accurate online GPS does not bypass manual face verification");
    Deny(()=>store.RevokeFaceEnrollment(liveEnrollment,"forged",outsider,"127.0.0.1"),"unrelated employee cannot revoke face");
    store.RevokeFaceEnrollment(liveEnrollment,"Tôi thu hồi đồng ý lưu ảnh",liveOwner,"127.0.0.1");
    Check(store.GetFaceEnrollmentState(liveOwner).Enrollment?.StatusCode=="REVOKED" && store.GetFaceEnrollmentPhoto(liveEnrollment,hr)==null &&
        db.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.HrmFaceEnrollment WHERE Id=@Id AND PhotoProtected IS NULL",new{Id=liveEnrollment})==1,"revocation purges encrypted photo and preserves status audit");
    Reject(()=>store.DecideRemotePunch(livePending.Id,true,"already revoked",reviewer,"127.0.0.1"),"revoked reference cannot approve pending punch");
    var afterRevoke=new RemotePunchRequest { ClientId=Guid.NewGuid(),OwnerUserId=liveOwner.Id,PlanId=livePlanId,Kind="OUT",CapturedAt=DateTimeOffset.UtcNow };
    Reject(()=>store.RecordRemotePunch(afterRevoke,photo,liveOwner,"127.0.0.1"),"revocation blocks new checkouts");
    store.RecordRemotePunch(livePunch,photo,liveOwner,"127.0.0.1");
    Check(store.GetRemotePunches(liveOwner,day,day).Count==1,"retry after revoke remains idempotent");
    Reject(()=>store.SubmitFaceEnrollment(photo,true,liveOwner,"127.0.0.1"),"enrollment immediate resubmit throttled");
    db.Execute("UPDATE dbo.HrmFaceEnrollment SET CreatedAt=DATEADD(minute,-2,SYSUTCDATETIME()) WHERE Id=@Id",new{Id=liveEnrollment});
    var replacement=store.SubmitFaceEnrollment(photo,true,liveOwner,"127.0.0.1");
    Check(store.GetFaceEnrollmentState(liveOwner).Enrollment?.StatusCode=="PENDING","replacement requires fresh HR approval");
    store.ReviewFaceEnrollment(replacement,false,"Ảnh chưa đúng người",hr,"127.0.0.1");
    Check(store.GetFaceEnrollmentState(liveOwner).Enrollment?.StatusCode=="REJECTED" && store.GetFaceEnrollmentPhoto(replacement,liveOwner)==null,"rejection purges replacement photo");
    Check(store.GetAttendance(owner,from.Date,to.Date).Single().WorkedMinutes==490,"new enrollment lifecycle preserves existing HANET plus remote attendance");

    var portListener=new System.Net.Sockets.TcpListener(IPAddress.Loopback,0);portListener.Start();var port=((IPEndPoint)portListener.LocalEndpoint).Port;portListener.Stop();
    var url=$"http://127.0.0.1:{port}";
    var start=new ProcessStartInfo("dotnet"){WorkingDirectory=root,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
    start.ArgumentList.Add(Path.Combine(root,"bin/Release/net8.0/NHIGIA.Modern.dll"));start.ArgumentList.Add("--urls");start.ArgumentList.Add(url);
    start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";start.Environment["HRM_CONNECTION_STRING"]=testString;start.Environment["HRM_DATA_PROTECTION_PATH"]=artifactPath;
    start.Environment["HRM_EXPECTED_DATABASE"]=database;
    host=Process.Start(start)!;host.OutputDataReceived+=(_,_)=>{};host.ErrorDataReceived+=(_,_)=>{};host.BeginOutputReadLine();host.BeginErrorReadLine();
    using var client=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=new Uri(url)};
    var ready=false;
    for(var i=0;i<40;i++){try{ready=(await client.GetAsync("/Health")).IsSuccessStatusCode;}catch(HttpRequestException){}if(ready)break;await Task.Delay(500);}
    Check(ready,"isolated HTTP host starts");
    var ticket=new TicketDataFormat(protection.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware","Cookies","v2"));
    string Cookie(HrmUserAccountModel user) => ticket.Protect(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),new Claim(ClaimTypes.Name,user.Username),new Claim(ClaimTypes.Role,user.RoleCode),new Claim("display_name",user.DisplayName),new Claim("department_id",(user.DepartmentId??0).ToString())},"Cookies")),new AuthenticationProperties{ExpiresUtc=DateTimeOffset.UtcNow.AddMinutes(20)},"Cookies"));
    Check((await client.GetAsync("/RemoteAttendance")).StatusCode==HttpStatusCode.Redirect,"remote page requires login");
    Check((await client.GetAsync("/FaceEnrollment")).StatusCode==HttpStatusCode.Redirect,"face page requires login");
    client.DefaultRequestHeaders.Add("Cookie","NHIGIA.Auth.v3="+Cookie(owner));
    var html=await client.GetStringAsync("/RemoteAttendance");
    Check(html.Contains("remotePunchForm") && !html.Contains("remotePlanForm"),"attendance page only renders punch form");
    var procedures=await client.GetStringAsync("/Home/LeaveRequests");
    Check(procedures.Contains("remotePlanForm") && !procedures.Contains("remotePunchForm"),"procedures page renders remote registration");
    Check((await client.PostAsync("/RemoteAttendance/CancelPlan",new FormUrlEncodedContent(new Dictionary<string,string>{{"id",planId.ToString()}}))).StatusCode==HttpStatusCode.BadRequest,"mutations require antiforgery");
    client.DefaultRequestHeaders.Remove("Cookie");client.DefaultRequestHeaders.Add("Cookie","NHIGIA.Auth.v3="+Cookie(outsider));
    Check((await client.GetAsync("/RemoteAttendance/Photo/"+pending.Id)).StatusCode==HttpStatusCode.NotFound,"HTTP photo access denied outside scope");
    Check((await client.GetAsync("/FaceEnrollment/Photo/"+enrollmentId)).StatusCode==HttpStatusCode.NotFound,"HTTP reference photo hidden from unrelated employee");
    Check((await client.GetAsync("/RemoteAttendance/FaceReference/"+pending.Id)).StatusCode==HttpStatusCode.NotFound,"HTTP punch reference hidden outside attendance scope");
    async Task<string> FaceToken(HrmUserAccountModel user)
    {
        client.DefaultRequestHeaders.Remove("Cookie");client.DefaultRequestHeaders.Add("Cookie","NHIGIA.Auth.v3="+Cookie(user));
        var faceHtml=await client.GetStringAsync("/FaceEnrollment");
        var match=Regex.Match(faceHtml,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Check(match.Success,"face page renders anti-forgery token");return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
    async Task<HttpResponseMessage> FacePost(string action,string token,params (string Name,string Value)[] fields)
    {
        var form=fields.ToDictionary(x=>x.Name,x=>x.Value);form["__RequestVerificationToken"]=token;
        return await client.PostAsync("/FaceEnrollment/"+action,new FormUrlEncodedContent(form));
    }
    Check((await client.PostAsync("/FaceEnrollment/Revoke",new FormUrlEncodedContent(new Dictionary<string,string>{{"id",enrollmentId.ToString()},{"note","CSRF attempt"}}))).StatusCode==HttpStatusCode.BadRequest,"face mutation requires anti-forgery");
    var managerToken=await FaceToken(reviewer);
    Check((await FacePost("Review",managerToken,("id",hrEnrollmentId.ToString()),("approve","true"),("note","manager attempt"))).StatusCode==HttpStatusCode.Forbidden,"HTTP manager denied enrollment review");
    Check((await client.GetAsync("/FaceEnrollment/Photo/"+enrollmentId)).StatusCode==HttpStatusCode.NotFound,"HTTP manager cannot browse reference gallery");
    var scopedReference=await client.GetAsync("/RemoteAttendance/FaceReference/"+pending.Id);
    Check(scopedReference.IsSuccessStatusCode && (await scopedReference.Content.ReadAsByteArrayAsync()).SequenceEqual(photo),"HTTP attendance reviewer can compare exact linked active reference");
    var hrToken=await FaceToken(hr);
    Check((await FacePost("Review",hrToken,("id",hrEnrollmentId.ToString()),("approve","true"),("note","self attempt"))).StatusCode==HttpStatusCode.BadRequest,"HTTP HR self approval rejected");
    var referenceResponse=await client.GetAsync("/FaceEnrollment/Photo/"+enrollmentId);
    Check(referenceResponse.IsSuccessStatusCode && referenceResponse.Headers.CacheControl?.NoStore==true && referenceResponse.Headers.GetValues("X-Content-Type-Options").Contains("nosniff"),"HTTP reference is not cached and disables content sniffing");
    Check((await client.GetAsync("/FaceEnrollment/Photo/"+liveEnrollment)).StatusCode==HttpStatusCode.NotFound,"HTTP revoked reference stays unavailable even to HR");
    var outsiderToken=await FaceToken(outsider);
    using var nonconsent=new MultipartFormDataContent();
    nonconsent.Add(new ByteArrayContent(photo),"photo","camera.jpg");nonconsent.Add(new StringContent("false"),"consent");nonconsent.Add(new StringContent(outsiderToken),"__RequestVerificationToken");
    Check((await client.PostAsync("/FaceEnrollment/Submit",nonconsent)).StatusCode==HttpStatusCode.BadRequest,"HTTP enrollment requires consent despite valid authenticated request");
    db.Execute("UPDATE dbo.HrmUserAccount SET IsActive=0 WHERE Id=@Id",new{outsider.Id});
    Check((await client.GetAsync("/FaceEnrollment/State")).StatusCode==HttpStatusCode.Forbidden,"HTTP reloads inactive account from database despite valid cookie");
    db.Execute("UPDATE dbo.HrmUserAccount SET IsActive=1 WHERE Id=@Id",new{outsider.Id});
    if(args.Contains("--browser"))
    {
        var browser=new ProcessStartInfo("node"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        browser.ArgumentList.Add(Path.GetFullPath("tests/RemoteAttendance.Tests/browser.cjs"));
        browser.Environment["HRM_REMOTE_TEST_URL"]=url;browser.Environment["HRM_REMOTE_TEST_COOKIE"]=Cookie(owner);browser.Environment["HRM_REMOTE_TEST_OUTPUT"]=artifactPath;
        browser.Environment["HRM_FACE_TEST_COOKIE"]=Cookie(browserFaceOwner);browser.Environment["HRM_FACE_REVIEW_COOKIE"]=Cookie(hr);
        using var run=Process.Start(browser)!;var stdout=run.StandardOutput.ReadToEndAsync();var stderr=run.StandardError.ReadToEndAsync();await run.WaitForExitAsync();Console.WriteLine(await stdout);Console.WriteLine(await stderr);Check(run.ExitCode==0,"browser mobile/desktop and offline queue");
    }
    Console.WriteLine("Artifacts: "+artifactPath);
}
finally
{
    if(host is {HasExited:false}){host.Kill(entireProcessTree:true);await host.WaitForExitAsync();}host?.Dispose();
    SqlConnection.ClearAllPools();
    if(!Regex.IsMatch(database,"^NHIGIA_RemoteTests_[a-f0-9]{32}$"))throw new Exception("Unsafe test database identifier");
    master.Execute($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]");
}
