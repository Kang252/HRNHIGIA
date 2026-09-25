using Dapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using NHIGIA.Modern.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHIGIA.Modern.Infrastructure
{
    public class HrmDataStore
    {
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _protector;

        public HrmDataStore(IConfiguration configuration, IDataProtectionProvider dataProtectionProvider)
        {
            _configuration = configuration;
            _protector = dataProtectionProvider.CreateProtector("NHIGIA", "HanetCredential", "v1");
        }

        private SqlConnection OpenConnection()
        {
            return DatabaseConfiguration.OpenConnection(_configuration);
        }

        public bool CanConnect()
        {
            using var connection = OpenConnection();
            return connection.State == ConnectionState.Open;
        }

        public void EnsureSchema(string scriptPath)
        {
            if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath)) return;
            var script = File.ReadAllText(scriptPath, System.Text.Encoding.UTF8);
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            using (var connection = OpenConnection())
            {
                foreach (var batch in batches.Where(x => !string.IsNullOrWhiteSpace(x))) connection.Execute(batch, commandTimeout: 120);
            }
        }

        public HrmUserAccountModel FindUser(string username)
        {
            const string sql = @"SELECT u.Id, u.Username, u.PasswordHash, u.PasswordSalt, u.DisplayName, u.RoleCode,
                u.DepartmentId, d.Name DepartmentName, u.SupervisorUserId, u.IsActive
                FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE u.Username=@Username";
            using (var connection = OpenConnection()) return connection.QuerySingleOrDefault<HrmUserAccountModel>(sql, new { Username = (username ?? string.Empty).Trim() });
        }

        public HrmUserAccountModel FindUser(int id)
        {
            const string sql = @"SELECT u.Id, u.Username, u.DisplayName, u.RoleCode, u.DepartmentId,
                d.Name DepartmentName, u.SupervisorUserId, u.IsActive
                FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId WHERE u.Id=@Id";
            using (var connection = OpenConnection()) return connection.QuerySingleOrDefault<HrmUserAccountModel>(sql, new { Id = id });
        }

        public EmployeeProfileModel GetEmployeeProfile(int userId)
        {
            const string sql = @"SELECT u.Id UserId, u.Username, u.DisplayName, u.RoleCode, u.DepartmentId,
                d.Name DepartmentName, u.SupervisorUserId, supervisor.DisplayName SupervisorName, u.IsActive,
                p.EmployeeCode,
                CASE WHEN p.AvatarContent IS NOT NULL THEN CONCAT('/Home/Avatar/', p.UserId, '?v=', DATEDIFF_BIG(MILLISECOND, '19700101', COALESCE(p.UpdatedAt, p.CreatedAt))) ELSE p.AvatarUrl END AvatarUrl,
                p.Gender, p.DateOfBirth, p.PlaceOfBirth, p.Nationality,
                p.Ethnicity, p.Religion, p.MaritalStatus, p.MobilePhone, p.OfficePhone, p.HomePhone,
                p.PersonalEmail, p.CompanyEmail, p.PermanentAddress, p.CurrentAddress,
                p.IdentityNumber, p.IdentityIssuedDate, p.IdentityIssuedPlace, p.IdentityExpiryDate,
                p.PassportNumber, p.PassportIssuedDate, p.PassportIssuedPlace, p.PassportExpiryDate,
                p.PersonalTaxCode, p.JobTitle, p.EmploymentStatus, p.WorkLocation, p.TimekeepingCode,
                p.HireDate, p.ProbationDate, p.OfficialDate, p.ContractType, p.ContractNumber,
                p.ContractStartDate, p.ContractEndDate, p.AnnualLeaveDays, p.EducationLevel, p.Degree,
                p.SchoolName, p.Faculty, p.Major, p.GraduationYear, p.GraduationClassification,
                p.BasicSalary, p.BankAccountNumber, p.BankName, p.BankBranch,
                p.SocialInsuranceNumber, p.SocialInsuranceStartDate, p.HealthInsuranceNumber,
                p.HealthInsuranceExpiryDate, p.RegisteredHealthFacility, p.EmergencyContactName,
                p.EmergencyContactRelationship, p.EmergencyContactPhone, p.EmergencyContactEmail,
                p.EmergencyContactAddress, p.Notes
                FROM dbo.HrmUserAccount u
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                LEFT JOIN dbo.HrmUserAccount supervisor ON supervisor.Id=u.SupervisorUserId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                WHERE u.Id=@UserId";
            using (var connection = OpenConnection()) return connection.QuerySingleOrDefault<EmployeeProfileModel>(sql, new { UserId = userId });
        }

        public string GetAvatarUrl(int userId)
        {
            const string sql = @"SELECT CASE WHEN AvatarContent IS NOT NULL
                THEN CONCAT('/Home/Avatar/', UserId, '?v=', DATEDIFF_BIG(MILLISECOND, '19700101', COALESCE(UpdatedAt, CreatedAt)))
                ELSE AvatarUrl END FROM dbo.HrmEmployeeProfile WHERE UserId=@UserId";
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<string>(sql, new { UserId = userId });
        }

        public CommunicationAttachmentModel GetAvatarContent(int userId, HrmUserAccountModel actor)
        {
            const string sql = @"SELECT p.AvatarContent Content, p.AvatarContentType ContentType
                FROM dbo.HrmEmployeeProfile p
                INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
                WHERE p.UserId=@UserId AND p.AvatarContent IS NOT NULL
                  AND u.IsActive=1 AND @ActorId>0";
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<CommunicationAttachmentModel>(sql, new
            {
                UserId = userId,
                ActorId = actor?.Id ?? 0
            });
        }

        public IList<WorkDepartment> GetDepartments()
        {
            using var connection = OpenConnection();
            return connection.Query<WorkDepartment>("SELECT Id,Name FROM dbo.HrmDepartment WHERE IsActive=1 ORDER BY Name").ToList();
        }

        public void UpdateEmployeeProfile(EmployeeProfileModel profile, HrmUserAccountModel actor, string ipAddress)
        {
            const string profileSql = @"UPDATE dbo.HrmEmployeeProfile SET
                    EmployeeCode=@EmployeeCode, Gender=@Gender, DateOfBirth=@DateOfBirth, PlaceOfBirth=@PlaceOfBirth,
                    Nationality=@Nationality, Ethnicity=@Ethnicity, Religion=@Religion, MaritalStatus=@MaritalStatus,
                    MobilePhone=@MobilePhone, OfficePhone=@OfficePhone, HomePhone=@HomePhone,
                    PersonalEmail=@PersonalEmail, CompanyEmail=@CompanyEmail, PermanentAddress=@PermanentAddress,
                    CurrentAddress=@CurrentAddress, IdentityNumber=@IdentityNumber, IdentityIssuedDate=@IdentityIssuedDate,
                    IdentityIssuedPlace=@IdentityIssuedPlace, IdentityExpiryDate=@IdentityExpiryDate,
                    PassportNumber=@PassportNumber, PassportIssuedDate=@PassportIssuedDate,
                    PassportIssuedPlace=@PassportIssuedPlace, PassportExpiryDate=@PassportExpiryDate,
                    PersonalTaxCode=@PersonalTaxCode, JobTitle=@JobTitle, EmploymentStatus=@EmploymentStatus,
                    WorkLocation=@WorkLocation, TimekeepingCode=@TimekeepingCode, HireDate=@HireDate,
                    ProbationDate=@ProbationDate, OfficialDate=@OfficialDate, ContractType=@ContractType,
                    ContractNumber=@ContractNumber, ContractStartDate=@ContractStartDate,
                    ContractEndDate=@ContractEndDate, AnnualLeaveDays=@AnnualLeaveDays,
                    EducationLevel=@EducationLevel, Degree=@Degree, SchoolName=@SchoolName, Faculty=@Faculty,
                    Major=@Major, GraduationYear=@GraduationYear, GraduationClassification=@GraduationClassification,
                    BasicSalary=@BasicSalary, BankAccountNumber=@BankAccountNumber, BankName=@BankName,
                    BankBranch=@BankBranch, SocialInsuranceNumber=@SocialInsuranceNumber,
                    SocialInsuranceStartDate=@SocialInsuranceStartDate, HealthInsuranceNumber=@HealthInsuranceNumber,
                    HealthInsuranceExpiryDate=@HealthInsuranceExpiryDate, RegisteredHealthFacility=@RegisteredHealthFacility,
                    EmergencyContactName=@EmergencyContactName,
                    EmergencyContactRelationship=@EmergencyContactRelationship,
                    EmergencyContactPhone=@EmergencyContactPhone, EmergencyContactEmail=@EmergencyContactEmail,
                    EmergencyContactAddress=@EmergencyContactAddress, Notes=@Notes, UpdatedAt=SYSDATETIME()
                WHERE UserId=@UserId";
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            connection.Execute(@"UPDATE dbo.HrmUserAccount SET DisplayName=@DisplayName,
                    DepartmentId=@DepartmentId, SupervisorUserId=@SupervisorUserId WHERE Id=@UserId",
                profile, transaction);
            if (connection.Execute(profileSql, profile, transaction) == 0)
            {
                connection.Execute(@"INSERT dbo.HrmEmployeeProfile(UserId,EmployeeCode,AvatarUrl,JobTitle,EmploymentStatus,
                        CompanyEmail,TimekeepingCode,AnnualLeaveDays) VALUES
                        (@UserId,@EmployeeCode,@AvatarUrl,@JobTitle,@EmploymentStatus,@CompanyEmail,@TimekeepingCode,@AnnualLeaveDays)",
                    profile, transaction);
                connection.Execute(profileSql, profile, transaction);
            }
            AddAudit(connection, actor.Id, "UPDATE", "HrmEmployeeProfile", profile.UserId.ToString(),
                "Cập nhật hồ sơ nhân viên " + profile.EmployeeCode, ipAddress, transaction);
            transaction.Commit();
        }

        public void UpdateAvatarUrl(int userId, string avatarUrl)
        {
            using var connection = OpenConnection();
            const string sql = @"
                UPDATE dbo.HrmEmployeeProfile SET AvatarUrl=@AvatarUrl, AvatarContent=NULL, AvatarContentType=NULL, UpdatedAt=SYSDATETIME() WHERE UserId=@UserId;
                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.HrmEmployeeProfile (UserId, AvatarUrl, CreatedAt) VALUES (@UserId, @AvatarUrl, SYSDATETIME());
                END";
            connection.Execute(sql, new { UserId = userId, AvatarUrl = avatarUrl });
        }

        public string UpdateAvatarContent(int userId, byte[] content, string contentType)
        {
            using var connection = OpenConnection();
            const string sql = @"
                UPDATE dbo.HrmEmployeeProfile SET AvatarContent=@Content, AvatarContentType=@ContentType,
                    AvatarUrl=NULL, UpdatedAt=SYSDATETIME() WHERE UserId=@UserId;
                IF @@ROWCOUNT = 0
                    THROW 50001, 'Hồ sơ nhân viên chưa tồn tại.', 1;";
            connection.Execute(sql, new { UserId = userId, Content = content, ContentType = contentType });
            return $"/Home/Avatar/{userId}?v={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }

        public void MarkLogin(int userId, string ipAddress)
        {
            using (var connection = OpenConnection())
            {
                connection.Execute("UPDATE dbo.HrmUserAccount SET LastLoginAt=SYSDATETIME() WHERE Id=@Id", new { Id = userId });
                AddAudit(connection, userId, "LOGIN", "HrmUserAccount", userId.ToString(), "Đăng nhập thành công", ipAddress);
            }
        }

        public IList<HrmUserAccountModel> GetVisibleUsers(HrmUserAccountModel actor)
        {
            const string sql = @"SELECT u.Id, u.Username, p.EmployeeCode,
                CASE WHEN p.AvatarContent IS NOT NULL THEN CONCAT('/Home/Avatar/', p.UserId, '?v=', DATEDIFF_BIG(MILLISECOND, '19700101', COALESCE(p.UpdatedAt, p.CreatedAt))) ELSE p.AvatarUrl END AvatarUrl,
                u.DisplayName, u.RoleCode, p.JobTitle, u.DepartmentId, d.Name DepartmentName,
                u.SupervisorUserId, u.IsActive
                FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
                  AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY d.Name, u.DisplayName";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection()) return connection.Query<HrmUserAccountModel>(sql, new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, DepartmentId = actor.DepartmentId }).ToList();
        }

        public IList<ShiftTemplateModel> GetShiftTemplates()
        {
            using (var connection = OpenConnection()) return connection.Query<ShiftTemplateModel>("SELECT Id, Code, Name, StartTime, EndTime, BreakMinutes, GraceMinutes, IsOvernight FROM dbo.HrmShiftTemplate WHERE IsActive=1 ORDER BY Id").ToList();
        }

        public IList<ScheduleModel> GetSchedules(HrmUserAccountModel actor)
        {
            const string sql = @"SELECT s.Id, s.UserId, u.Username, u.DisplayName, d.Name DepartmentName, s.ShiftTemplateId,
                s.ShiftName, s.StartTime, s.EndTime, s.BreakMinutes, s.GraceMinutes, s.WorkDaysMask, s.EffectiveFrom, s.EffectiveTo, s.StatusCode
                FROM dbo.HrmEmployeeSchedule s
                INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY s.EffectiveFrom DESC, u.DisplayName";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection()) return connection.Query<ScheduleModel>(sql, new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, DepartmentId = actor.DepartmentId }).ToList();
        }

        public IList<LeaveRequestModel> GetApprovedScheduleLeaves(HrmUserAccountModel actor, DateTime fromDate, DateTime toDate)
        {
            const string sql = @"SELECT r.Id,r.RequestCode,r.UserId,u.Username,u.DisplayName,hm.PersonId,
                    d.Name DepartmentName,p.EmployeeCode,p.JobTitle,r.LeaveType,r.StartDate,r.EndDate,r.SessionCode,r.StatusCode
                FROM dbo.HrmLeaveRequest r
                INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                OUTER APPLY (SELECT TOP 1 m.PersonId FROM dbo.HrmHanetPersonMap m
                    WHERE m.UserId=u.Id AND m.IsActive=1 ORDER BY m.Id DESC) hm
                WHERE r.StatusCode='APPROVED' AND r.StartDate<=@ToDate AND r.EndDate>=@FromDate
                    AND r.LeaveType IN @LeaveTypes
                    AND (@CanSeeAll=1 OR r.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY r.StartDate,u.DisplayName,r.Id";
            using var connection = OpenConnection();
            return connection.Query<LeaveRequestModel>(sql, new
            {
                FromDate = fromDate.Date, ToDate = toDate.Date, AttendanceLeavePolicy.LeaveTypes,
                CanSeeAll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director,
                ActorId = actor.Id, IsManager = actor.RoleCode == HrmRoles.Manager, actor.DepartmentId
            }).Where(AttendanceLeavePolicy.IsApprovedLeave).ToList();
        }

        public int SaveSchedule(SaveScheduleRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            if (request.Id > 0)
            {
                const string updateSql = @"UPDATE s SET UserId=@UserId, ShiftTemplateId=@ShiftTemplateId, ShiftName=@ShiftName,
                    StartTime=@StartTime, EndTime=@EndTime, BreakMinutes=@BreakMinutes, GraceMinutes=@GraceMinutes, WorkDaysMask=@WorkDaysMask,
                    EffectiveFrom=@EffectiveFrom, EffectiveTo=@EffectiveTo, UpdatedAt=SYSDATETIME()
                    FROM dbo.HrmEmployeeSchedule s
                    INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId
                    WHERE s.Id=@Id AND (@CanSeeAll=1 OR (@IsManager=1 AND u.DepartmentId=@DepartmentId));";
                var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
                using (var connection = OpenConnection())
                {
                    var changed = connection.Execute(updateSql, new
                    {
                        request.Id,
                        request.UserId,
                        request.ShiftTemplateId,
                        request.ShiftName,
                        request.StartTime,
                        request.EndTime,
                        request.BreakMinutes,
                        request.GraceMinutes,
                        request.WorkDaysMask,
                        request.EffectiveFrom,
                        request.EffectiveTo,
                        CanSeeAll = canSeeAll,
                        IsManager = actor.RoleCode == HrmRoles.Manager,
                        DepartmentId = actor.DepartmentId
                    });
                    if (changed > 0) AddAudit(connection, actor.Id, "UPDATE", "HrmEmployeeSchedule", request.Id.ToString(), "Cập nhật lịch làm việc", ipAddress);
                    return changed > 0 ? request.Id : 0;
                }
            }

            const string sql = @"INSERT dbo.HrmEmployeeSchedule(UserId, ShiftTemplateId, ShiftName, StartTime, EndTime, BreakMinutes, GraceMinutes, WorkDaysMask, EffectiveFrom, EffectiveTo, StatusCode, CreatedByUserId)
                VALUES(@UserId, @ShiftTemplateId, @ShiftName, @StartTime, @EndTime, @BreakMinutes, @GraceMinutes, @WorkDaysMask, @EffectiveFrom, @EffectiveTo, 'ACTIVE', @ActorId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var connection = OpenConnection())
            {
                var id = connection.ExecuteScalar<int>(sql, new { request.UserId, request.ShiftTemplateId, request.ShiftName, request.StartTime, request.EndTime, request.BreakMinutes, request.GraceMinutes, request.WorkDaysMask, request.EffectiveFrom, request.EffectiveTo, ActorId = actor.Id });
                AddAudit(connection, actor.Id, "CREATE", "HrmEmployeeSchedule", id.ToString(), "Phân lịch làm việc", ipAddress);
                return id;
            }
        }

        public bool DeleteSchedule(int id, HrmUserAccountModel actor, string ipAddress)
        {
            const string sql = @"DELETE s FROM dbo.HrmEmployeeSchedule s INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId
                WHERE s.Id=@Id AND (@CanSeeAll=1 OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection())
            {
                var changed = connection.Execute(sql, new { Id = id, CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, DepartmentId = actor.DepartmentId }) > 0;
                if (changed) AddAudit(connection, actor.Id, "DELETE", "HrmEmployeeSchedule", id.ToString(), "Xóa lịch làm việc", ipAddress);
                return changed;
            }
        }

        public IList<LeaveRequestModel> GetLeaveRequests(HrmUserAccountModel actor)
        {
            const string sql = @"SELECT r.Id, r.RequestCode, r.UserId, u.Username, u.DisplayName, u.RoleCode, u.DepartmentId, d.Name DepartmentName,
                p.EmployeeCode, p.JobTitle,
                CASE WHEN p.AvatarContent IS NOT NULL THEN CONCAT('/Home/Avatar/', p.UserId, '?v=', DATEDIFF_BIG(MILLISECOND, '19700101', COALESCE(p.UpdatedAt, p.CreatedAt))) ELSE p.AvatarUrl END AvatarUrl,
                p.MobilePhone, p.CompanyEmail,
                r.LeaveType, r.StartDate, r.EndDate, r.SessionCode, r.HandoverTo, r.Reason, r.AttachmentName,
                CAST(CASE WHEN r.AttachmentContent IS NULL THEN 0 ELSE 1 END AS BIT) HasAttachment,
                r.StatusCode, r.ManagerNote, r.HrNote, r.ApprovedByManagerId, r.ApprovedByHrId,
                mgr.DisplayName ManagerName, hr.DisplayName HrName,
                r.CreatedAt, r.UpdatedAt
                FROM dbo.HrmLeaveRequest r
                INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                LEFT JOIN dbo.HrmUserAccount mgr ON mgr.Id=r.ApprovedByManagerId
                LEFT JOIN dbo.HrmUserAccount hr ON hr.Id=r.ApprovedByHrId
                WHERE (@CanSeeAll=1 OR r.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY r.CreatedAt DESC";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection()) return connection.Query<LeaveRequestModel>(sql, new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, DepartmentId = actor.DepartmentId }).ToList();
        }

        public LeaveStatsModel GetLeaveStats(HrmUserAccountModel actor)
        {
            var year = CurrentVietnamTime().Year;
            using var connection = OpenConnection();
            var allowance = connection.QuerySingle<decimal>(@"SELECT CAST(COALESCE(
                (SELECT AnnualLeaveDays FROM dbo.HrmEmployeeProfile WHERE UserId=@Id),12) AS DECIMAL(10,1))", new { actor.Id });
            var requests = connection.Query<LeaveRequestModel>(@"SELECT UserId,LeaveType,Reason,StartDate,EndDate,SessionCode,StatusCode
                FROM dbo.HrmLeaveRequest WHERE UserId=@Id AND StartDate<=@To AND EndDate>=@From",
                new { actor.Id, From = new DateTime(year, 1, 1), To = new DateTime(year, 12, 31) }).ToList();
            var schedules = connection.Query<ScheduleModel>("SELECT * FROM dbo.HrmEmployeeSchedule WHERE UserId=@Id AND StatusCode='ACTIVE'", new { actor.Id });
            return new LeaveStatsModel
            {
                AnnualAllowance = allowance,
                UsedDays = LeaveRequestPolicy.UsedAnnualDays(requests, schedules, year),
                PendingCount = requests.Count(x => x.StatusCode is "PENDING_MANAGER" or "PENDING_HR"),
                ApprovedCount = requests.Count(x => x.StatusCode == "APPROVED")
            };
        }

        private static void NotifyLeave(SqlConnection connection, SqlTransaction transaction, int id, string label, string note, bool pending = false)
        {
            connection.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
                SELECT r.UserId,LEFT(CONCAT(@Label,N': ',r.RequestCode),200),LEFT(CONCAT(r.LeaveType,N' · ',@Label,N'. ',@Note),1000),'/Home/LeaveRequests'
                FROM dbo.HrmLeaveRequest r WHERE r.Id=@Id;
                IF @Pending=1
                INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
                SELECT reviewer.Id,LEFT(CONCAT(N'Chờ duyệt: ',r.RequestCode),200),N'Có yêu cầu nhân sự mới cần xử lý.','/Home/Approvals'
                FROM dbo.HrmLeaveRequest r JOIN dbo.HrmUserAccount owner ON owner.Id=r.UserId
                JOIN dbo.HrmUserAccount reviewer ON reviewer.IsActive=1 AND reviewer.Id<>r.UserId
                  AND (reviewer.RoleCode IN ('ADMIN','HR','DIRECTOR') OR
                    (r.StatusCode='PENDING_MANAGER' AND owner.RoleCode<>'MANAGER' AND reviewer.RoleCode='MANAGER' AND reviewer.DepartmentId=owner.DepartmentId))
                WHERE r.Id=@Id;", new { Id=id, Label=label, Note=note, Pending=pending }, transaction);
        }

        public LeaveRequestModel CreateLeave(CreateLeaveRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            var error = LeaveRequestPolicy.Validate(request);
            if (error != null) throw new InvalidOperationException(error);
            var targetUserId = request.EmployeeId ?? actor.Id;
            var target = FindUser(targetUserId);
            if (!LeaveRequestPolicy.CanCreateFor(actor, target))
                throw new InvalidOperationException("Nhân viên không còn hoạt động hoặc không thuộc phạm vi của bạn.");
            const string sql = @"INSERT dbo.HrmLeaveRequest(UserId, LeaveType, StartDate, EndDate, SessionCode, HandoverTo, Reason, AttachmentName, AttachmentContentType, AttachmentContent, StatusCode)
                VALUES(@UserId, @LeaveType, @StartDate, @EndDate, @SessionCode, @HandoverTo, @Reason, @AttachmentName, @AttachmentContentType, @AttachmentContent, 'PENDING_MANAGER');
                DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
                SELECT r.Id, r.RequestCode, r.UserId, u.Username, u.DisplayName, u.RoleCode, d.Name DepartmentName,
                    r.LeaveType, r.StartDate, r.EndDate, r.SessionCode, r.HandoverTo, r.Reason, r.AttachmentName,
                    CAST(CASE WHEN r.AttachmentContent IS NULL THEN 0 ELSE 1 END AS BIT) HasAttachment, r.StatusCode, r.CreatedAt
                FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId WHERE r.Id=@Id;";
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var result = connection.QuerySingle<LeaveRequestModel>(sql, new { UserId = targetUserId, request.LeaveType, request.StartDate, request.EndDate, request.SessionCode, request.HandoverTo, request.Reason, request.AttachmentName, request.AttachmentContentType, request.AttachmentContent }, transaction);
                AddAudit(connection, actor.Id, "CREATE", "HrmLeaveRequest", result.Id.ToString(), "Gửi đơn yêu cầu " + result.RequestCode, ipAddress, transaction);
                NotifyLeave(connection, transaction, result.Id, "Đã gửi, chờ phê duyệt", null, true);
                transaction.Commit();
                return result;
            }
        }

        public bool CancelLeave(int id, HrmUserAccountModel actor, string ipAddress)
        {
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var changed = connection.Execute("UPDATE dbo.HrmLeaveRequest SET StatusCode='CANCELLED', UpdatedAt=SYSDATETIME() WHERE Id=@Id AND UserId=@UserId AND StatusCode IN ('PENDING_MANAGER','PENDING_HR')", new { Id = id, UserId = actor.Id }, transaction) > 0;
                if (changed) AddAudit(connection, actor.Id, "CANCEL", "HrmLeaveRequest", id.ToString(), "Hủy đơn nghỉ phép", ipAddress, transaction);
                if (changed) NotifyLeave(connection, transaction, id, "Đã hủy yêu cầu", null);
                transaction.Commit();
                return changed;
            }
        }

        public bool DeleteLeave(int id, HrmUserAccountModel actor, string ipAddress)
        {
            if (actor == null || !HrmRoles.CanManagePeople(actor.RoleCode)) return false;
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var changed = connection.Execute(@"DELETE r FROM dbo.HrmLeaveRequest r
                    INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                    WHERE r.Id=@Id AND (@CanSeeAll=1 OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))",
                    new { Id=id, CanSeeAll=HrmRoles.CanPublishCompanyWide(actor.RoleCode), IsManager=actor.RoleCode==HrmRoles.Manager, actor.DepartmentId }, transaction) > 0;
                if (changed) AddAudit(connection, actor.Id, "DELETE", "HrmLeaveRequest", id.ToString(), "Xóa yêu cầu", ipAddress, transaction);
                transaction.Commit();
                return changed;
            }
        }

        public CommunicationAttachmentModel GetLeaveAttachment(int id, HrmUserAccountModel actor)
        {
            const string sql = @"SELECT r.AttachmentName FileName, r.AttachmentContentType ContentType,
                    r.AttachmentContent Content
                FROM dbo.HrmLeaveRequest r
                INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                WHERE r.Id=@Id AND r.AttachmentContent IS NOT NULL
                  AND (@CanSeeAll=1 OR r.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))";
            var canSeeAll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<CommunicationAttachmentModel>(sql,
                new { Id = id, CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, actor.DepartmentId });
        }

        public bool ApproveLeave(ApprovalRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var target = connection.QuerySingleOrDefault<LeaveRequestModel>(@"
                    SELECT r.Id, r.UserId, r.StatusCode, u.RoleCode, u.DepartmentId
                    FROM dbo.HrmLeaveRequest r
                    INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                    WHERE r.Id=@Id", new { request.Id }, transaction);
                if (!LeaveRequestPolicy.CanApprove(actor, target)) return false;
                if (request.Note?.Length > 1000) throw new InvalidOperationException("Ghi chú duyệt tối đa 1.000 ký tự.");
                var sql = @"UPDATE dbo.HrmLeaveRequest SET StatusCode=@Status,
                    ManagerNote=CASE WHEN @IsManager=1 THEN @Note ELSE ManagerNote END,
                    ApprovedByManagerId=CASE WHEN @IsManager=1 THEN @ActorId ELSE ApprovedByManagerId END,
                    HrNote=CASE WHEN @IsManager=0 THEN @Note ELSE HrNote END,
                    ApprovedByHrId=CASE WHEN @IsManager=0 THEN @ActorId ELSE ApprovedByHrId END,
                    UpdatedAt=SYSDATETIME() WHERE Id=@Id AND StatusCode=@ExpectedStatus";
                var param = new { Status = request.Approve ? "APPROVED" : "REJECTED", request.Note,
                    IsManager=actor.RoleCode==HrmRoles.Manager, ActorId = actor.Id, request.Id, ExpectedStatus=target.StatusCode };

                var changed = connection.Execute(sql, param, transaction) > 0;
                if (changed) AddAudit(connection, actor.Id, request.Approve ? "APPROVE" : "REJECT", "HrmLeaveRequest", request.Id.ToString(), request.Note, ipAddress, transaction);
                if (changed) NotifyLeave(connection, transaction, request.Id, request.Approve ? "Đã phê duyệt" : "Đã từ chối", request.Note);
                transaction.Commit();
                return changed;
            }
        }

        public int BulkApproveLeave(BulkApprovalRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            if (request?.Ids == null || !request.Ids.Any()) return 0;
            var processed = 0;
            foreach (var id in request.Ids.Distinct())
            {
                if (ApproveLeave(new ApprovalRequest { Id = id, Approve = request.Approve, Note = request.Note }, actor, ipAddress))
                {
                    processed++;
                }
            }
            return processed;
        }

        public int ApproveAllLeaves(HrmUserAccountModel actor, string note, string ipAddress)
        {
            var ids = GetLeaveRequests(actor).Where(x => LeaveRequestPolicy.CanApprove(actor, x)).Select(x => x.Id).ToList();
            return BulkApproveLeave(new BulkApprovalRequest { Ids=ids, Approve=true, Note=note }, actor, ipAddress);
        }

        public IList<CommunicationModel> GetCommunications(HrmUserAccountModel actor, string keyword, string category, string status = "", int take = 100)
        {
            const string sql = @"SELECT TOP (@Take) c.Id, c.AuthorUserId, u.DisplayName AuthorName,
                CASE WHEN authorProfile.AvatarContent IS NOT NULL
                    THEN CONCAT('/Home/Avatar/',u.Id,'?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(authorProfile.UpdatedAt,authorProfile.CreatedAt)))
                    ELSE authorProfile.AvatarUrl END AuthorAvatarUrl, authorProfile.JobTitle AuthorJobTitle, c.Category, c.ScopeCode,
                c.DepartmentId, c.Title, c.Body, c.AttachmentName, c.AttachmentContentType, c.IsPinned,
                COALESCE(c.StatusCode, CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END) StatusCode,
                c.ApprovedByUserId, approver.DisplayName ApprovedByName, c.ReviewNote,
                COALESCE(c.SubmittedAt,c.PublishedAt) SubmittedAt, c.PublishedAt,
                (SELECT COUNT(1) FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id) LikeCount,
                (SELECT COUNT(1) FROM dbo.HrmCommunicationComment m WHERE m.CommunicationId=c.Id AND m.IsDeleted=0) CommentCount,
                CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id AND r.UserId=@ActorId) THEN 1 ELSE 0 END AS BIT) LikedByCurrentUser,
                CAST(CASE WHEN c.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS BIT) IsMine
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
                LEFT JOIN dbo.HrmEmployeeProfile authorProfile ON authorProfile.UserId=u.Id
                LEFT JOIN dbo.HrmUserAccount approver ON approver.Id=c.ApprovedByUserId
                WHERE (
                    (c.IsPublished=1 AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                        OR (c.ScopeCode='MANAGER' AND @IsManager=1)))
                    OR c.AuthorUserId=@ActorId
                    OR (@CanModerate=1 AND COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)='PENDING')
                  )
                  AND (@Status='' OR COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)=@Status)
                  AND (@Keyword='' OR c.Title LIKE '%' + @Keyword + '%' OR c.Body LIKE '%' + @Keyword + '%')
                  AND (@Category='' OR c.Category=@Category)
                ORDER BY CASE WHEN COALESCE(c.StatusCode,'PUBLISHED')='PENDING' THEN 0 ELSE 1 END,
                    c.IsPinned DESC, COALESCE(c.SubmittedAt,c.PublishedAt) DESC";
            var isManager = actor.RoleCode == HrmRoles.Manager || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director || actor.RoleCode == HrmRoles.Admin;
            var canModerate = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection = OpenConnection();
            var posts = connection.Query<CommunicationModel>(sql, new { Take = take, actor.DepartmentId, IsManager = isManager, CanModerate = canModerate, ActorId = actor.Id, Keyword = keyword ?? string.Empty, Category = category ?? string.Empty, Status = (status ?? string.Empty).ToUpperInvariant() }).ToList();
            var ids = posts.Where(x => x.StatusCode == "PUBLISHED").Select(x => x.Id).ToArray();
            if (ids.Length > 0)
            {
                const string commentSql = @"SELECT m.Id, m.CommunicationId, m.AuthorUserId, u.DisplayName AuthorName,
                    CASE WHEN p.AvatarContent IS NOT NULL
                        THEN CONCAT('/Home/Avatar/',u.Id,'?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
                        ELSE p.AvatarUrl END AuthorAvatarUrl, p.JobTitle AuthorJobTitle, m.Body, m.CreatedAt,
                    CAST(CASE WHEN m.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS BIT) IsMine
                    FROM dbo.HrmCommunicationComment m INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId
                    LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                    WHERE m.IsDeleted=0 AND m.CommunicationId IN @Ids ORDER BY m.CreatedAt";
                var comments = connection.Query<CommunicationCommentModel>(commentSql, new { Ids = ids, ActorId = actor.Id }).ToLookup(x => x.CommunicationId);
                foreach (var post in posts) post.Comments = comments[post.Id].ToList();
            }
            return posts;
        }

        private static void NotifyPublishedPost(SqlConnection connection, SqlTransaction transaction, int id)
        {
            connection.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
                SELECT u.Id,LEFT(CONCAT(N'Bài đăng mới: ',c.Title),200),N'Có bài đăng mới trong truyền thông nội bộ.','/Home/InternalCommunications'
                FROM dbo.HrmCommunication c JOIN dbo.HrmUserAccount u ON u.IsActive=1 AND u.Id<>c.AuthorUserId
                AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND u.DepartmentId=c.DepartmentId)
                  OR (c.ScopeCode='MANAGER' AND u.RoleCode IN ('MANAGER','HR','DIRECTOR','ADMIN')))
                WHERE c.Id=@Id AND c.IsPublished=1",new { Id=id },transaction);
        }

        private static void NotifyCommunication(SqlConnection connection, SqlTransaction transaction, int id, string label, string message, int actorId, bool reviewers = false)
        {
            connection.Execute(@"INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
                SELECT u.Id,LEFT(CONCAT(@Label,N': ',c.Title),200),LEFT(@Message,1000),'/Home/InternalCommunications'
                FROM dbo.HrmCommunication c JOIN dbo.HrmUserAccount u ON u.IsActive=1
                  AND ((@Reviewers=0 AND u.Id=c.AuthorUserId AND u.Id<>@ActorId)
                    OR (@Reviewers=1 AND u.RoleCode IN ('ADMIN','HR','DIRECTOR') AND u.Id<>@ActorId))
                WHERE c.Id=@Id;",new { Id=id, Label=label, Message=message ?? label, ActorId=actorId, Reviewers=reviewers },transaction);
        }

        public CommunicationModel CreateCommunication(CreateCommunicationRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            var scope = (request.ScopeCode ?? "DEPARTMENT").ToUpperInvariant();
            var departmentId = scope == "DEPARTMENT" ? actor.DepartmentId : null;
            var publishImmediately = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            const string sql = @"INSERT dbo.HrmCommunication(AuthorUserId, Category, ScopeCode, DepartmentId, Title, Body, AttachmentName, AttachmentContentType, AttachmentContent, IsPinned, IsPublished, StatusCode, SubmittedAt, ApprovedByUserId, ReviewedAt)
                VALUES(@AuthorUserId, @Category, @ScopeCode, @DepartmentId, @Title, @Body, @AttachmentName, @AttachmentContentType, @AttachmentContent, @IsPinned, @IsPublished, @StatusCode, SYSDATETIME(), @ApprovedByUserId, @ReviewedAt);
                DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
                SELECT c.Id, c.AuthorUserId, u.DisplayName AuthorName,
                    CASE WHEN p.AvatarContent IS NOT NULL
                        THEN CONCAT('/Home/Avatar/',u.Id,'?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
                        ELSE p.AvatarUrl END AuthorAvatarUrl, p.JobTitle AuthorJobTitle, c.Category, c.ScopeCode, c.DepartmentId,
                    c.Title, c.Body, c.AttachmentName, c.AttachmentContentType, c.IsPinned, c.StatusCode,
                    c.SubmittedAt, c.PublishedAt, CAST(1 AS BIT) IsMine
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id WHERE c.Id=@Id;";
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var result = connection.QuerySingle<CommunicationModel>(sql, new { AuthorUserId = actor.Id, request.Category, ScopeCode = scope, DepartmentId = departmentId, request.Title, request.Body, request.AttachmentName, request.AttachmentContentType, request.AttachmentContent, IsPinned = publishImmediately && request.IsPinned, IsPublished = publishImmediately, StatusCode = publishImmediately ? "PUBLISHED" : "PENDING", ApprovedByUserId = publishImmediately ? actor.Id : (int?)null, ReviewedAt = publishImmediately ? DateTime.Now : (DateTime?)null }, transaction);
                AddAudit(connection, actor.Id, publishImmediately ? "PUBLISH" : "SUBMIT", "HrmCommunication", result.Id.ToString(), result.Title, ipAddress, transaction);
                if (publishImmediately) NotifyPublishedPost(connection, transaction, result.Id);
                if (!publishImmediately) NotifyCommunication(connection, transaction, result.Id, "Bài đăng chờ duyệt", "Có bài đăng mới cần duyệt.", actor.Id, true);
                transaction.Commit();
                return result;
            }
        }

        public bool UpdateCommunication(CreateCommunicationRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            var canModerate = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            var scope = (request.ScopeCode ?? "DEPARTMENT").ToUpperInvariant();
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            var current = connection.QuerySingleOrDefault<(int AuthorUserId, int? DepartmentId, string Title)>(
                "SELECT AuthorUserId,DepartmentId,Title FROM dbo.HrmCommunication WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1)",
                new { request.Id, ActorId = actor.Id, CanModerate = canModerate }, transaction);
            if (current.AuthorUserId == 0) return false;
            var departmentId = scope == "DEPARTMENT"
                ? (current.AuthorUserId == actor.Id ? actor.DepartmentId : current.DepartmentId)
                : null;
            const string sql = @"UPDATE dbo.HrmCommunication SET Category=@Category,ScopeCode=@ScopeCode,DepartmentId=@DepartmentId,
                    Title=@Title,Body=@Body,
                    AttachmentName=CASE WHEN @HasAttachment=1 THEN @AttachmentName WHEN @RemoveAttachment=1 THEN NULL ELSE AttachmentName END,
                    AttachmentContentType=CASE WHEN @HasAttachment=1 THEN @AttachmentContentType WHEN @RemoveAttachment=1 THEN NULL ELSE AttachmentContentType END,
                    AttachmentContent=CASE WHEN @HasAttachment=1 THEN @AttachmentContent WHEN @RemoveAttachment=1 THEN NULL ELSE AttachmentContent END,
                    IsPinned=CASE WHEN @CanModerate=1 THEN @IsPinned ELSE IsPinned END,
                    IsPublished=CASE WHEN @CanModerate=1 THEN IsPublished ELSE 0 END,
                    StatusCode=CASE WHEN @CanModerate=1 THEN StatusCode ELSE 'PENDING' END,
                    SubmittedAt=CASE WHEN @CanModerate=1 THEN SubmittedAt ELSE SYSDATETIME() END,
                    ApprovedByUserId=CASE WHEN @CanModerate=1 THEN ApprovedByUserId ELSE NULL END,
                    ReviewedAt=CASE WHEN @CanModerate=1 THEN ReviewedAt ELSE NULL END,
                    ReviewNote=CASE WHEN @CanModerate=1 THEN ReviewNote ELSE NULL END
                WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1);";
            var changed = connection.Execute(sql, new
            {
                request.Id, request.Category, ScopeCode = scope, DepartmentId = departmentId, request.Title, request.Body,
                request.AttachmentName, request.AttachmentContentType, request.AttachmentContent,
                HasAttachment = request.AttachmentContent?.Length > 0, request.RemoveAttachment,
                IsPinned = canModerate && request.IsPinned, CanModerate = canModerate, ActorId = actor.Id
            }, transaction) == 1;
            if (changed) AddAudit(connection, actor.Id, "UPDATE", "HrmCommunication", request.Id.ToString(), request.Title, ipAddress, transaction);
            if (changed && !canModerate) NotifyCommunication(connection, transaction, request.Id, "Bài đăng đã sửa, chờ duyệt", "Bài đăng vừa được tác giả cập nhật và cần duyệt lại.", actor.Id, true);
            transaction.Commit();
            return changed;
        }

        public bool DeleteCommunication(int id, HrmUserAccountModel actor, string ipAddress)
        {
            var canModerate = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            var post = connection.QuerySingleOrDefault<(int AuthorUserId, string Title)>(
                "SELECT AuthorUserId,Title FROM dbo.HrmCommunication WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1)",
                new { Id = id, ActorId = actor.Id, CanModerate = canModerate }, transaction);
            if (post.AuthorUserId == 0) return false;
            connection.Execute("DELETE dbo.HrmCommunicationReaction WHERE CommunicationId=@Id; DELETE dbo.HrmCommunicationComment WHERE CommunicationId=@Id;", new { Id = id }, transaction);
            var changed = connection.Execute("DELETE dbo.HrmCommunication WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1)", new { Id = id, ActorId = actor.Id, CanModerate = canModerate }, transaction) == 1;
            if (changed) AddAudit(connection, actor.Id, "DELETE", "HrmCommunication", id.ToString(), post.Title, ipAddress, transaction);
            transaction.Commit();
            return changed;
        }

        public bool ModerateCommunication(ModerateCommunicationRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            if (actor.RoleCode is not (HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director)) return false;
            const string sql = @"UPDATE dbo.HrmCommunication SET StatusCode=@StatusCode, IsPublished=@IsPublished,
                    ApprovedByUserId=@ActorId, ReviewedAt=SYSDATETIME(), ReviewNote=@Note,
                    PublishedAt=CASE WHEN @IsPublished=1 THEN SYSDATETIME() ELSE PublishedAt END
                WHERE Id=@Id AND COALESCE(StatusCode,CASE WHEN IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)='PENDING';";
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            var changed = connection.Execute(sql, new { request.Id, StatusCode = request.Approve ? "PUBLISHED" : "REJECTED", IsPublished = request.Approve, ActorId = actor.Id, Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim() }, transaction) == 1;
            if (changed) AddAudit(connection, actor.Id, request.Approve ? "APPROVE" : "REJECT", "HrmCommunication", request.Id.ToString(), request.Note, ipAddress, transaction);
            if (changed && request.Approve) NotifyPublishedPost(connection, transaction, request.Id);
            if (changed) NotifyCommunication(connection, transaction, request.Id, request.Approve ? "Bài đăng đã duyệt" : "Bài đăng bị từ chối", request.Note, actor.Id);
            transaction.Commit();
            return changed;
        }

        public object ToggleCommunicationReaction(int id, HrmUserAccountModel actor, string ipAddress)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            const string visibleSql = @"SELECT COUNT(1) FROM dbo.HrmCommunication c WHERE c.Id=@Id AND c.IsPublished=1
                AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                    OR (c.ScopeCode='MANAGER' AND @IsManager=1) OR c.AuthorUserId=@ActorId)";
            var isManager = actor.RoleCode is HrmRoles.Manager or HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin;
            if (connection.ExecuteScalar<int>(visibleSql, new { Id = id, actor.DepartmentId, IsManager = isManager, ActorId = actor.Id }, transaction) == 0) throw new InvalidOperationException("Bài đăng không tồn tại hoặc chưa được xuất bản.");
            var liked = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmCommunicationReaction WHERE CommunicationId=@Id AND UserId=@UserId", new { Id = id, UserId = actor.Id }, transaction) > 0;
            if (liked) connection.Execute("DELETE dbo.HrmCommunicationReaction WHERE CommunicationId=@Id AND UserId=@UserId", new { Id = id, UserId = actor.Id }, transaction);
            else connection.Execute("INSERT dbo.HrmCommunicationReaction(CommunicationId,UserId) VALUES(@Id,@UserId)", new { Id = id, UserId = actor.Id }, transaction);
            var count = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.HrmCommunicationReaction WHERE CommunicationId=@Id", new { Id = id }, transaction);
            if (!liked) NotifyCommunication(connection, transaction, id, "Tương tác mới", $"{actor.DisplayName} đã thích bài đăng của bạn.", actor.Id);
            transaction.Commit();
            return new { Liked = !liked, Count = count };
        }

        public CommunicationCommentModel AddCommunicationComment(int id, string body, HrmUserAccountModel actor, string ipAddress)
        {
            const string sql = @"IF EXISTS(SELECT 1 FROM dbo.HrmCommunication c WHERE c.Id=@CommunicationId AND c.IsPublished=1
                    AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                        OR (c.ScopeCode='MANAGER' AND @IsManager=1) OR c.AuthorUserId=@AuthorUserId))
                BEGIN
                    INSERT dbo.HrmCommunicationComment(CommunicationId,AuthorUserId,Body) VALUES(@CommunicationId,@AuthorUserId,@Body);
                    DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
                    SELECT m.Id,m.CommunicationId,m.AuthorUserId,u.DisplayName AuthorName,
                        CASE WHEN p.AvatarContent IS NOT NULL
                            THEN CONCAT('/Home/Avatar/',u.Id,'?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
                            ELSE p.AvatarUrl END AuthorAvatarUrl, p.JobTitle AuthorJobTitle,
                        m.Body,m.CreatedAt,CAST(1 AS BIT) IsMine
                    FROM dbo.HrmCommunicationComment m INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId
                    LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id WHERE m.Id=@Id;
                END";
            var isManager = actor.RoleCode is HrmRoles.Manager or HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin;
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            var comment = connection.QuerySingleOrDefault<CommunicationCommentModel>(sql, new { CommunicationId = id, AuthorUserId = actor.Id, Body = body.Trim(), actor.DepartmentId, IsManager = isManager }, transaction);
            if (comment == null) throw new InvalidOperationException("Bài đăng không tồn tại hoặc chưa được xuất bản.");
            AddAudit(connection, actor.Id, "COMMENT", "HrmCommunication", id.ToString(), body, ipAddress, transaction);
            NotifyCommunication(connection, transaction, id, "Bình luận mới", $"{actor.DisplayName}: {body}", actor.Id);
            transaction.Commit();
            return comment;
        }

        public CommunicationAttachmentModel GetCommunicationAttachment(int id, HrmUserAccountModel actor)
        {
            const string sql = @"SELECT c.AttachmentName FileName, c.AttachmentContentType ContentType,
                    c.AttachmentContent Content
                FROM dbo.HrmCommunication c
                WHERE c.Id=@Id AND c.AttachmentContent IS NOT NULL
                  AND (c.AuthorUserId=@ActorId OR @CanModerate=1 OR (c.IsPublished=1 AND
                    (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                     OR (c.ScopeCode='MANAGER' AND @IsManager=1))))";
            var isManager = actor.RoleCode is HrmRoles.Manager or HrmRoles.Hr or HrmRoles.Director or HrmRoles.Admin;
            var canModerate = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<CommunicationAttachmentModel>(sql,
                new { Id = id, actor.DepartmentId, IsManager = isManager, CanModerate = canModerate, ActorId = actor.Id });
        }

        public static DateTime CurrentVietnamTime()
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime;
        }

        public IList<AttendanceRecordModel> GetAttendance(HrmUserAccountModel actor, DateTime fromDate, DateTime toDate)
        {
            if (toDate.Date < fromDate.Date || (toDate.Date - fromDate.Date).TotalDays > 366)
                throw new InvalidOperationException("Khoảng lọc tối đa là 366 ngày.");
            const string sql = @"WITH EventRaw AS (
                    SELECT e.UserId, CAST(e.CheckTime AS DATE) WorkDate, MIN(e.CheckTime) CheckIn,
                        MAX(e.CheckTime) LastSeen, COUNT(1) EventCount
                    FROM dbo.HrmAttendanceEvent e
                    LEFT JOIN dbo.HrmAttendancePeriod ap ON ap.Period=CONVERT(char(7),e.CheckTime,126)
                    WHERE e.UserId IS NOT NULL AND e.CheckTime>=@FromDate AND e.CheckTime<DATEADD(DAY,1,@ToDate)
                      AND (ap.StatusCode IS NULL OR ap.StatusCode<>'LOCKED' OR e.ReceivedAt<=ap.LockedAt)
                    GROUP BY e.UserId, CAST(e.CheckTime AS DATE)
                ), Dates AS (
                    SELECT UserId,WorkDate FROM EventRaw UNION
                    SELECT UserId,WorkDate FROM dbo.HrmAttendanceAdjustment
                    WHERE StatusCode='APPROVED' AND WorkDate BETWEEN @FromDate AND @ToDate
                ), Events AS (
                    SELECT d.UserId,d.WorkDate,e.CheckIn,e.LastSeen,COALESCE(e.EventCount,0) EventCount
                    FROM Dates d LEFT JOIN EventRaw e ON e.UserId=d.UserId AND e.WorkDate=d.WorkDate
                )
                SELECT e.UserId, hm.PersonId, p.EmployeeCode, u.DisplayName, p.JobTitle, d.Name DepartmentName, e.WorkDate, s.ShiftName,
                    s.StartTime ScheduledStart, s.EndTime ScheduledEnd, s.GraceMinutes, s.BreakMinutes,
                    COALESCE(a.RequestedCheckIn,e.CheckIn) CheckIn,
                    COALESCE(a.RequestedCheckOut,CASE WHEN e.EventCount>1 AND e.LastSeen<>e.CheckIn THEN e.LastSeen END) CheckOut,
                    e.LastSeen, e.EventCount, CASE WHEN a.Id IS NULL THEN 'HANET' ELSE N'Điều chỉnh đã duyệt' END Source
                FROM Events e INNER JOIN dbo.HrmUserAccount u ON u.Id=e.UserId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                LEFT JOIN dbo.HrmHanetPersonMap hm ON hm.UserId=u.Id AND hm.IsActive=1
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                OUTER APPLY (SELECT TOP 1 x.Id,x.RequestedCheckIn,x.RequestedCheckOut
                    FROM dbo.HrmAttendanceAdjustment x WHERE x.UserId=e.UserId AND x.WorkDate=e.WorkDate AND x.StatusCode='APPROVED'
                    ORDER BY x.ReviewedAt DESC,x.Id DESC) a
                OUTER APPLY (SELECT TOP 1 x.ShiftName, x.StartTime, x.EndTime, x.GraceMinutes, x.BreakMinutes
                    FROM dbo.HrmEmployeeSchedule x WHERE x.UserId=e.UserId AND x.StatusCode='ACTIVE'
                      AND x.EffectiveFrom<=e.WorkDate AND (x.EffectiveTo IS NULL OR x.EffectiveTo>=e.WorkDate)
                      AND (x.WorkDaysMask & CASE ((DATEDIFF(DAY, CONVERT(date,'19000107'), e.WorkDate) % 7 + 7) % 7)
                          WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 4 WHEN 3 THEN 8
                          WHEN 4 THEN 16 WHEN 5 THEN 32 WHEN 6 THEN 64 END) <> 0
                    ORDER BY x.EffectiveFrom DESC, x.Id DESC) s
                WHERE (@CanSeeAll=1 OR e.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY e.WorkDate DESC, u.DisplayName";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection())
            {
                var rows = connection.Query<AttendanceRecordModel>(sql, new { FromDate = fromDate.Date, ToDate = toDate.Date, CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, actor.DepartmentId }).ToList();
                return AttendanceLeavePolicy.Project(rows, GetSchedules(actor).ToList(),
                    GetApprovedScheduleLeaves(actor, fromDate, toDate).ToList(), fromDate, toDate, CurrentVietnamTime());
            }
        }

        public DashboardModel GetDashboard(HrmUserAccountModel actor)
        {
            var today = CurrentVietnamTime().Date;
            var attendance = GetAttendance(actor, today, today);
            var communications = GetCommunications(actor, string.Empty, string.Empty, "PUBLISHED", 3);
            const string employeeSql = @"SELECT COUNT(1) FROM dbo.HrmUserAccount u WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
                AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))";
            const string pendingSql = @"SELECT COUNT(1) FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                WHERE ((@Role='MANAGER' AND r.StatusCode='PENDING_MANAGER' AND u.DepartmentId=@DepartmentId)
                    OR (@Role IN ('HR','ADMIN','DIRECTOR') AND r.StatusCode IN ('PENDING_MANAGER','PENDING_HR'))
                    OR (@Role='EMPLOYEE' AND r.UserId=@ActorId AND r.StatusCode IN ('PENDING_MANAGER','PENDING_HR')))";
            const string unmappedSql = @"SELECT COUNT(1) FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmHanetPersonMap m ON m.UserId=u.Id AND m.IsActive=1
                WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN' AND m.Id IS NULL
                    AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection())
            {
                var args = new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, actor.DepartmentId, Role = actor.RoleCode };
                return new DashboardModel
                {
                    TotalEmployees = connection.ExecuteScalar<int>(employeeSql, args),
                    PresentToday = attendance.Where(x => x.EventCount > 0).Select(x => x.UserId).Distinct().Count(),
                    OnLeaveToday = attendance.Where(x => x.StatusCode == "ON_LEAVE" && x.EventCount == 0).Select(x => x.UserId).Distinct().Count(),
                    LateOrEarlyToday = attendance.Count(x => x.LateMinutes > 0 || x.EarlyMinutes > 0),
                    PendingApprovals = connection.ExecuteScalar<int>(pendingSql, args),
                    UnmappedHanetUsers = connection.ExecuteScalar<int>(unmappedSql, args),
                    AttendanceToday = attendance.Take(8).ToList(),
                    Communications = communications.ToList()
                };
            }
        }

        public IList<HanetDeviceModel> GetHanetDevices()
        {
            using var db = OpenConnection();
            return db.Query<HanetDeviceModel>("SELECT Id,DeviceId,Name,PlaceId,Location,Notes,IsActive FROM dbo.HrmHanetDevice ORDER BY Name,Id").ToList();
        }

        public int SaveHanetDevice(HanetDeviceModel device, HrmUserAccountModel actor, string ip)
        {
            using var db = OpenConnection();
            using var tx = db.BeginTransaction();
            int id;
            if (device.Id == 0)
                id = db.ExecuteScalar<int>(@"INSERT dbo.HrmHanetDevice(DeviceId,Name,PlaceId,Location,Notes,IsActive)
                    OUTPUT INSERTED.Id VALUES(@DeviceId,@Name,@PlaceId,@Location,@Notes,@IsActive)", device, tx);
            else
            {
                if (db.Execute(@"UPDATE dbo.HrmHanetDevice SET DeviceId=@DeviceId,Name=@Name,PlaceId=@PlaceId,
                    Location=@Location,Notes=@Notes,IsActive=@IsActive,UpdatedAt=SYSDATETIME() WHERE Id=@Id", device, tx) == 0)
                    throw new InvalidOperationException("Thiết bị không còn tồn tại.");
                id = device.Id;
            }
            AddAudit(db, actor.Id, device.Id == 0 ? "CREATE" : "UPDATE", "HrmHanetDevice", id.ToString(), "Lưu thiết bị HANET " + device.DeviceId, ip, tx);
            tx.Commit();
            return id;
        }

        public bool DeleteHanetDevice(int id, HrmUserAccountModel actor, string ip)
        {
            using var db = OpenConnection();
            using var tx = db.BeginTransaction();
            var changed = db.Execute("DELETE dbo.HrmHanetDevice WHERE Id=@Id", new { Id=id }, tx) > 0;
            if (changed) AddAudit(db, actor.Id, "DELETE", "HrmHanetDevice", id.ToString(), "Xóa bản ghi quản lý thiết bị HANET", ip, tx);
            tx.Commit();
            return changed;
        }

        public HanetSettingsModel GetHanetSettings(bool includeSecrets)
        {
            const string sql = @"SELECT ApiBaseUrl, OAuthTokenUrl, ClientId, ProtectedClientSecret ClientSecret,
                ProtectedAccessToken AccessToken, PlaceId, WebhookSecret, IsEnabled, LastSyncAt, LastSyncStatus, LastSyncMessage
                FROM dbo.HrmHanetSettings WHERE Id=1";
            using (var connection = OpenConnection())
            {
                var settings = connection.QuerySingle<HanetSettingsModel>(sql);
                if (includeSecrets)
                {
                    settings.ClientSecret = Unprotect(settings.ClientSecret);
                    settings.AccessToken = Unprotect(settings.AccessToken);
                }
                else
                {
                    settings.ClientSecret = string.IsNullOrEmpty(settings.ClientSecret) ? null : "********";
                    settings.AccessToken = string.IsNullOrEmpty(settings.AccessToken) ? null : "********";
                }
                return settings;
            }
        }

        public void SaveHanetSettings(HanetSettingsModel settings, HrmUserAccountModel actor, string ipAddress)
        {
            settings.AccessToken = settings.AccessToken?.Trim();
            settings.PlaceId = settings.PlaceId?.Trim();
            settings.ApiBaseUrl = settings.ApiBaseUrl?.Trim().TrimEnd('/');
            settings.OAuthTokenUrl = settings.OAuthTokenUrl?.Trim();

            const string sql = @"UPDATE dbo.HrmHanetSettings SET ApiBaseUrl=@ApiBaseUrl, OAuthTokenUrl=@OAuthTokenUrl,
                ClientId=@ClientId, ProtectedClientSecret=COALESCE(@ProtectedClientSecret, ProtectedClientSecret),
                ProtectedAccessToken=COALESCE(@ProtectedAccessToken, ProtectedAccessToken), PlaceId=@PlaceId,
                WebhookSecret=@WebhookSecret, IsEnabled=@IsEnabled, UpdatedByUserId=@UserId, UpdatedAt=SYSDATETIME() WHERE Id=1";
            var protectedSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret) && settings.ClientSecret != "********" ? Protect(settings.ClientSecret) : null;
            var protectedToken = !string.IsNullOrWhiteSpace(settings.AccessToken) && settings.AccessToken != "********" ? Protect(settings.AccessToken) : null;
            using (var connection = OpenConnection())
            {
                connection.Execute(sql, new { settings.ApiBaseUrl, settings.OAuthTokenUrl, settings.ClientId, ProtectedClientSecret = protectedSecret, ProtectedAccessToken = protectedToken, settings.PlaceId, settings.WebhookSecret, settings.IsEnabled, UserId = actor.Id });
                AddAudit(connection, actor.Id, "UPDATE", "HrmHanetSettings", "1", "Cập nhật cấu hình HANET", ipAddress);
            }
        }

        public void UpdateHanetSyncStatus(string status, string message)
        {
            using (var connection = OpenConnection()) connection.Execute("UPDATE dbo.HrmHanetSettings SET LastSyncAt=SYSDATETIME(), LastSyncStatus=@Status, LastSyncMessage=@Message WHERE Id=1", new { Status = status, Message = message });
        }

        public void AddHanetSyncRun(DateTime workDate, DateTime startedAt, DateTime finishedAt, string status, int received, int inserted, string message)
        {
            using var connection = OpenConnection();
            connection.Execute(@"INSERT dbo.HrmHanetSyncRun(WorkDate,StartedAt,FinishedAt,StatusCode,ReceivedCount,InsertedCount,Message)
                VALUES(@WorkDate,@StartedAt,@FinishedAt,@Status,@Received,@Inserted,@Message)",
                new { WorkDate=workDate.Date, StartedAt=startedAt, FinishedAt=finishedAt, Status=status, Received=received, Inserted=inserted, Message=message });
        }

        public IList<HanetSyncRunModel> GetHanetReconciliation(DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate || (toDate-fromDate).TotalDays > 31) throw new InvalidOperationException("Đối soát tối đa 31 ngày.");
            using var connection = OpenConnection();
            return connection.Query<HanetSyncRunModel>(@"WITH EventStats AS (
                    SELECT CAST(CheckTime AS date) WorkDate, COUNT(1) TotalEvents,
                        SUM(CASE WHEN UserId IS NULL THEN 1 ELSE 0 END) UnmappedEvents,
                        SUM(CASE WHEN UserId IS NOT NULL THEN 1 ELSE 0 END) MappedEvents,
                        COUNT(DISTINCT UserId) MappedEmployees
                    FROM dbo.HrmAttendanceEvent WHERE CheckTime>=@From AND CheckTime<DATEADD(day,1,@To)
                    GROUP BY CAST(CheckTime AS date)), Latest AS (
                    SELECT *,ROW_NUMBER() OVER(PARTITION BY WorkDate ORDER BY Id DESC) rn FROM dbo.HrmHanetSyncRun
                    WHERE WorkDate BETWEEN @From AND @To)
                SELECT l.Id,l.WorkDate,l.StartedAt,l.FinishedAt,l.StatusCode,l.ReceivedCount,l.InsertedCount,l.Message,
                    COALESCE(e.MappedEvents,0) MappedEvents,COALESCE(e.UnmappedEvents,0) UnmappedEvents,COALESCE(e.MappedEmployees,0) MappedEmployees
                FROM Latest l LEFT JOIN EventStats e ON e.WorkDate=l.WorkDate WHERE l.rn=1 ORDER BY l.WorkDate DESC",
                new { From=fromDate.Date, To=toDate.Date }).ToList();
        }

        public AttendancePeriodModel GetAttendancePeriod(string period, HrmUserAccountModel actor)
        {
            if (!DateTime.TryParseExact(period+"-01","yyyy-MM-dd",null,System.Globalization.DateTimeStyles.None,out _))
                throw new InvalidOperationException("Kỳ chấm công không hợp lệ.");
            using var connection=OpenConnection();
            return connection.QuerySingle<AttendancePeriodModel>(@"SELECT @Period Period,COALESCE(p.StatusCode,'OPEN') StatusCode,
                    CONVERT(bit,CASE WHEN c.UserId IS NULL THEN 0 ELSE 1 END) IsConfirmed,c.SubmittedAt,p.LockedAt,u.DisplayName LockedByName,p.UnlockReason,
                    (SELECT COUNT(1) FROM dbo.HrmUserAccount x WHERE x.IsActive=1 AND x.RoleCode<>'ADMIN') EmployeeCount,
                    (SELECT COUNT(1) FROM dbo.HrmAttendancePeriodConfirmation x INNER JOIN dbo.HrmUserAccount a ON a.Id=x.UserId WHERE x.Period=@Period AND x.StatusCode='SUBMITTED' AND a.IsActive=1 AND a.RoleCode<>'ADMIN') ConfirmedCount,
                    (SELECT COUNT(1) FROM dbo.HrmAttendanceAdjustment x WHERE CONVERT(char(7),x.WorkDate,126)=@Period AND x.StatusCode='PENDING') PendingAdjustmentCount
                FROM (SELECT 1 n) seed LEFT JOIN dbo.HrmAttendancePeriod p ON p.Period=@Period
                LEFT JOIN dbo.HrmAttendancePeriodConfirmation c ON c.Period=@Period AND c.UserId=@UserId
                LEFT JOIN dbo.HrmUserAccount u ON u.Id=p.LockedByUserId",new {Period=period,UserId=actor.Id});
        }

        public bool ConfirmAttendancePeriod(string period, HrmUserAccountModel actor, string ip)
        {
            var state=GetAttendancePeriod(period,actor);
            if(state.StatusCode=="LOCKED") throw new InvalidOperationException("Kỳ chấm công đã khóa.");
            using var connection=OpenConnection(); using var tx=connection.BeginTransaction();
            connection.Execute(@"MERGE dbo.HrmAttendancePeriodConfirmation AS t USING(SELECT @Period Period,@UserId UserId)s
                ON t.Period=s.Period AND t.UserId=s.UserId WHEN MATCHED THEN UPDATE SET StatusCode='SUBMITTED',SubmittedAt=SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT(Period,UserId) VALUES(@Period,@UserId);",new {Period=period,UserId=actor.Id},tx);
            AddAudit(connection,actor.Id,"SUBMIT","AttendancePeriod",period,"Xác nhận dữ liệu chấm công tháng",ip,tx);tx.Commit();return true;
        }

        public bool SetAttendancePeriodLock(string period, bool locked, string reason, HrmUserAccountModel actor, string ip)
        {
            if (!DateTime.TryParseExact(period+"-01","yyyy-MM-dd",null,System.Globalization.DateTimeStyles.None,out _)) throw new InvalidOperationException("Kỳ chấm công không hợp lệ.");
            if(!locked && string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("Vui lòng nhập lý do mở khóa.");
            var state=GetAttendancePeriod(period,actor);
            if(locked && state.PendingAdjustmentCount>0) throw new InvalidOperationException("Còn yêu cầu điều chỉnh chấm công đang chờ xử lý.");
            if(locked && state.ConfirmedCount<state.EmployeeCount) throw new InvalidOperationException($"Chưa đủ xác nhận chấm công ({state.ConfirmedCount}/{state.EmployeeCount} nhân viên).");
            using var connection=OpenConnection();using var tx=connection.BeginTransaction();
            connection.Execute(@"MERGE dbo.HrmAttendancePeriod AS t USING(SELECT @Period Period)s ON t.Period=s.Period
                WHEN MATCHED THEN UPDATE SET StatusCode=@Status,LockedByUserId=CASE WHEN @Locked=1 THEN @UserId ELSE NULL END,
                    LockedAt=CASE WHEN @Locked=1 THEN SYSDATETIME() ELSE NULL END,UnlockReason=CASE WHEN @Locked=0 THEN @Reason ELSE NULL END,UpdatedAt=SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT(Period,StatusCode,LockedByUserId,LockedAt,UnlockReason)
                    VALUES(@Period,@Status,CASE WHEN @Locked=1 THEN @UserId END,CASE WHEN @Locked=1 THEN SYSDATETIME() END,CASE WHEN @Locked=0 THEN @Reason END);",
                new {Period=period,Status=locked?"LOCKED":"OPEN",Locked=locked,UserId=actor.Id,Reason=reason?.Trim()},tx);
            AddAudit(connection,actor.Id,locked?"LOCK":"UNLOCK","AttendancePeriod",period,locked?"Khóa kỳ chấm công":"Mở khóa kỳ chấm công: "+reason,ip,tx);tx.Commit();return true;
        }

        public IList<AttendanceAdjustmentModel> GetAttendanceAdjustments(HrmUserAccountModel actor,string period)
        {
            if (!DateTime.TryParseExact(period+"-01","yyyy-MM-dd",null,System.Globalization.DateTimeStyles.None,out _)) throw new InvalidOperationException("Kỳ chấm công không hợp lệ.");
            var canSeeAll=actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection=OpenConnection();
            return connection.Query<AttendanceAdjustmentModel>(@"SELECT a.*,u.DisplayName,d.Name DepartmentName FROM dbo.HrmAttendanceAdjustment a
                INNER JOIN dbo.HrmUserAccount u ON u.Id=a.UserId LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE CONVERT(char(7),a.WorkDate,126)=@Period AND (@All=1 OR a.UserId=@ActorId OR (@Manager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY a.CreatedAt DESC",new {Period=period,All=canSeeAll,ActorId=actor.Id,Manager=actor.RoleCode==HrmRoles.Manager,actor.DepartmentId}).ToList();
        }

        public long CreateAttendanceAdjustment(AttendanceAdjustmentModel item,HrmUserAccountModel actor,string ip)
        {
            if(item.WorkDate==default || item.WorkDate>CurrentVietnamTime().Date || string.IsNullOrWhiteSpace(item.Reason) || item.Reason.Length>1000)
                throw new InvalidOperationException("Ngày và lý do điều chỉnh không hợp lệ.");
            if(!item.RequestedCheckIn.HasValue && !item.RequestedCheckOut.HasValue) throw new InvalidOperationException("Vui lòng nhập giờ vào hoặc giờ ra cần điều chỉnh.");
            if(item.RequestedCheckIn.HasValue && item.RequestedCheckIn.Value.Date!=item.WorkDate.Date) throw new InvalidOperationException("Giờ vào phải thuộc ngày cần điều chỉnh.");
            if(item.RequestedCheckOut.HasValue && item.RequestedCheckOut.Value.Date!=item.WorkDate.Date) throw new InvalidOperationException("Giờ ra phải thuộc ngày cần điều chỉnh.");
            if(item.RequestedCheckIn.HasValue && item.RequestedCheckOut.HasValue && item.RequestedCheckIn>=item.RequestedCheckOut) throw new InvalidOperationException("Giờ ra phải sau giờ vào.");
            var period=item.WorkDate.ToString("yyyy-MM"); if(GetAttendancePeriod(period,actor).StatusCode=="LOCKED") throw new InvalidOperationException("Kỳ chấm công đã khóa.");
            using var connection=OpenConnection();using var tx=connection.BeginTransaction();
            long id;
            try
            {
                id=connection.ExecuteScalar<long>(@"INSERT dbo.HrmAttendanceAdjustment(UserId,WorkDate,RequestedCheckIn,RequestedCheckOut,Reason)
                    OUTPUT INSERTED.Id VALUES(@UserId,@WorkDate,@RequestedCheckIn,@RequestedCheckOut,@Reason)",new {UserId=actor.Id,item.WorkDate,item.RequestedCheckIn,item.RequestedCheckOut,Reason=item.Reason.Trim()},tx);
            }
            catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number is 2601 or 2627)
            {
                throw new InvalidOperationException("Ngày này đã có một yêu cầu điều chỉnh đang chờ xử lý.");
            }
            AddAudit(connection,actor.Id,"CREATE","AttendanceAdjustment",id.ToString(),"Yêu cầu điều chỉnh chấm công",ip,tx);tx.Commit();return id;
        }

        public bool DecideAttendanceAdjustment(long id,bool approve,string note,HrmUserAccountModel actor,string ip)
        {
            using var connection=OpenConnection();using var tx=connection.BeginTransaction();
            var changed=connection.Execute(@"UPDATE a SET StatusCode=@Status,ReviewNote=@Note,ReviewedByUserId=@ActorId,ReviewedAt=SYSDATETIME()
                FROM dbo.HrmAttendanceAdjustment a INNER JOIN dbo.HrmUserAccount u ON u.Id=a.UserId
                LEFT JOIN dbo.HrmAttendancePeriod p ON p.Period=CONVERT(char(7),a.WorkDate,126)
                WHERE a.Id=@Id AND a.StatusCode='PENDING' AND COALESCE(p.StatusCode,'OPEN')<>'LOCKED' AND a.UserId<>@ActorId
                  AND (@All=1 OR (@Manager=1 AND u.RoleCode='EMPLOYEE' AND u.DepartmentId=@DepartmentId))",
                new {Id=id,Status=approve?"APPROVED":"REJECTED",Note=note?.Trim(),ActorId=actor.Id,
                    All=actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director,Manager=actor.RoleCode==HrmRoles.Manager,actor.DepartmentId},tx)>0;
            if(changed)AddAudit(connection,actor.Id,approve?"APPROVE":"REJECT","AttendanceAdjustment",id.ToString(),note,ip,tx);tx.Commit();return changed;
        }

        public void SaveHanetPersonMap(HanetPersonMapRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            request.PersonId = string.IsNullOrWhiteSpace(request.PersonId) ? null : request.PersonId.Trim();
            request.AliasId = string.IsNullOrWhiteSpace(request.AliasId) ? null : request.AliasId.Trim();
            request.PlaceId = string.IsNullOrWhiteSpace(request.PlaceId) ? null : request.PlaceId.Trim();
            const string clearConflictsSql = @"UPDATE dbo.HrmHanetPersonMap WITH (UPDLOCK, SERIALIZABLE)
                SET PersonId=NULL, AliasId=NULL, IsActive=0, UpdatedAt=SYSDATETIME()
                WHERE UserId<>@UserId AND ((@PersonId IS NOT NULL AND PersonId=@PersonId) OR (@AliasId IS NOT NULL AND AliasId=@AliasId));";
            const string saveSql = @"MERGE dbo.HrmHanetPersonMap WITH (HOLDLOCK) AS target USING (SELECT @UserId UserId) AS source ON target.UserId=source.UserId
                WHEN MATCHED THEN UPDATE SET AliasId=@AliasId, PersonId=@PersonId, PlaceId=@PlaceId, IsActive=1, UpdatedAt=SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT(UserId, AliasId, PersonId, PlaceId) VALUES(@UserId, @AliasId, @PersonId, @PlaceId);";
            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();
                var replacedUserIds = connection.Query<int>(@"SELECT UserId FROM dbo.HrmHanetPersonMap WITH (UPDLOCK, SERIALIZABLE)
                    WHERE UserId<>@UserId AND ((@PersonId IS NOT NULL AND PersonId=@PersonId) OR (@AliasId IS NOT NULL AND AliasId=@AliasId));", request, transaction).ToArray();
                connection.Execute(clearConflictsSql, request, transaction);
                connection.Execute(saveSql, request, transaction);
                var detail = replacedUserIds.Length == 0
                    ? "Ánh xạ nhân viên HANET"
                    : "Chuyển ánh xạ HANET từ nhân viên " + string.Join(", ", replacedUserIds);
                AddAudit(connection, actor.Id, "UPDATE", "HrmHanetPersonMap", request.UserId.ToString(), detail, ipAddress, transaction);
                transaction.Commit();
            }
        }

        public bool SaveAttendanceEvent(HanetWebhookEvent item)
        {
            const string sql = @"DECLARE @UserId INT=(SELECT TOP 1 UserId FROM dbo.HrmHanetPersonMap WHERE IsActive=1 AND ((@PersonId IS NOT NULL AND PersonId=@PersonId) OR (@AliasId IS NOT NULL AND AliasId=@AliasId)));
                INSERT dbo.HrmAttendanceEvent(EventKey, UserId, PersonId, AliasId, PlaceId, DeviceId, CheckTime, EventType, PayloadJson)
                VALUES(@EventKey, @UserId, @PersonId, @AliasId, @PlaceId, @DeviceId, @CheckTime, @EventType, @PayloadJson);";
            try
            {
                using (var connection = OpenConnection()) return connection.Execute(sql, item) > 0;
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627) return false;
                throw;
            }
        }

        public int SaveAttendanceEvents(IEnumerable<HanetWebhookEvent> items)
        {
            var rows = (items ?? Enumerable.Empty<HanetWebhookEvent>()).Where(x => !string.IsNullOrWhiteSpace(x.EventKey)).ToList();
            if (rows.Count == 0) return 0;
            const string sql = @"WITH SourceRaw AS (
                    SELECT EventKey,PersonId,AliasId,PlaceId,DeviceId,CheckTime,EventType,PayloadJson,
                        ROW_NUMBER() OVER(PARTITION BY EventKey ORDER BY CheckTime, PersonId) AS RowNumber
                    FROM OPENJSON(@Json) WITH (
                        EventKey NVARCHAR(200), PersonId NVARCHAR(100), AliasId NVARCHAR(100), PlaceId NVARCHAR(100),
                        DeviceId NVARCHAR(100), CheckTime DATETIME2, EventType NVARCHAR(50), PayloadJson NVARCHAR(MAX))
                ), Source AS (
                    SELECT EventKey,PersonId,AliasId,PlaceId,DeviceId,CheckTime,EventType,PayloadJson
                    FROM SourceRaw WHERE RowNumber=1
                )
                INSERT dbo.HrmAttendanceEvent(EventKey,UserId,PersonId,AliasId,PlaceId,DeviceId,CheckTime,EventType,PayloadJson)
                SELECT s.EventKey,m.UserId,s.PersonId,s.AliasId,s.PlaceId,s.DeviceId,s.CheckTime,s.EventType,s.PayloadJson
                FROM Source s
                OUTER APPLY (SELECT TOP 1 UserId FROM dbo.HrmHanetPersonMap
                    WHERE IsActive=1 AND ((s.PersonId IS NOT NULL AND PersonId=s.PersonId) OR (s.AliasId IS NOT NULL AND AliasId=s.AliasId))) m
                WHERE NOT EXISTS(SELECT 1 FROM dbo.HrmAttendanceEvent e WITH (UPDLOCK, HOLDLOCK) WHERE e.EventKey=s.EventKey);
                SELECT @@ROWCOUNT;";
            var json = System.Text.Json.JsonSerializer.Serialize(rows);
            using var connection = OpenConnection();
            return connection.ExecuteScalar<int>(sql, new { Json = json });
        }

        private static void AddAudit(IDbConnection connection, int? userId, string action, string entityType, string entityId, string detail, string ipAddress, IDbTransaction transaction = null)
        {
            connection.Execute("INSERT dbo.HrmAuditLog(UserId, ActionCode, EntityType, EntityId, Detail, IpAddress) VALUES(@UserId,@Action,@EntityType,@EntityId,@Detail,@IpAddress)", new { UserId = userId, Action = action, EntityType = entityType, EntityId = entityId, Detail = detail, IpAddress = ipAddress }, transaction);
        }

        private string Protect(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : _protector.Protect(value);
        }

        private string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            try { return _protector.Unprotect(value); }
            catch { return null; }
        }
    }
}
