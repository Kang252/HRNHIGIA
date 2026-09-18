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
                  AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))";
            var canSeeAll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<CommunicationAttachmentModel>(sql, new
            {
                UserId = userId,
                CanSeeAll = canSeeAll,
                IsManager = actor.RoleCode == HrmRoles.Manager,
                ActorId = actor.Id,
                actor.DepartmentId
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
                u.DisplayName, u.RoleCode, u.DepartmentId, d.Name DepartmentName,
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
                s.ShiftName, s.StartTime, s.EndTime, s.BreakMinutes, s.GraceMinutes, s.EffectiveFrom, s.EffectiveTo, s.StatusCode
                FROM dbo.HrmEmployeeSchedule s
                INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY s.EffectiveFrom DESC, u.DisplayName";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection()) return connection.Query<ScheduleModel>(sql, new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, DepartmentId = actor.DepartmentId }).ToList();
        }

        public int SaveSchedule(SaveScheduleRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            if (request.Id > 0)
            {
                const string updateSql = @"UPDATE s SET UserId=@UserId, ShiftTemplateId=@ShiftTemplateId, ShiftName=@ShiftName,
                    StartTime=@StartTime, EndTime=@EndTime, BreakMinutes=@BreakMinutes, GraceMinutes=@GraceMinutes,
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

            const string sql = @"INSERT dbo.HrmEmployeeSchedule(UserId, ShiftTemplateId, ShiftName, StartTime, EndTime, BreakMinutes, GraceMinutes, EffectiveFrom, EffectiveTo, StatusCode, CreatedByUserId)
                VALUES(@UserId, @ShiftTemplateId, @ShiftName, @StartTime, @EndTime, @BreakMinutes, @GraceMinutes, @EffectiveFrom, @EffectiveTo, 'ACTIVE', @ActorId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var connection = OpenConnection())
            {
                var id = connection.ExecuteScalar<int>(sql, new { request.UserId, request.ShiftTemplateId, request.ShiftName, request.StartTime, request.EndTime, request.BreakMinutes, request.GraceMinutes, request.EffectiveFrom, request.EffectiveTo, ActorId = actor.Id });
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
            const string sql = @"SELECT
                CAST(COALESCE((SELECT AnnualLeaveDays FROM dbo.HrmEmployeeProfile WHERE UserId=@UserId), 12) AS DECIMAL(10,1)) AnnualAllowance,
                CAST(COALESCE(SUM(CASE WHEN StatusCode='APPROVED' THEN DATEDIFF(DAY, StartDate, EndDate)+1 ELSE 0 END),0) AS DECIMAL(10,1)) UsedDays,
                COALESCE(SUM(CASE WHEN StatusCode IN ('PENDING_MANAGER','PENDING_HR') THEN 1 ELSE 0 END),0) PendingCount,
                COALESCE(SUM(CASE WHEN StatusCode='APPROVED' THEN 1 ELSE 0 END),0) ApprovedCount
                FROM dbo.HrmLeaveRequest WHERE UserId=@UserId AND YEAR(StartDate)=YEAR(GETDATE())";
            using (var connection = OpenConnection()) return connection.QuerySingle<LeaveStatsModel>(sql, new { UserId = actor.Id });
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
            var targetUserId = (actor != null && HrmRoles.CanManagePeople(actor.RoleCode) && request.EmployeeId.HasValue && request.EmployeeId.Value > 0)
                ? request.EmployeeId.Value
                : actor.Id;
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
                var changed = connection.Execute("DELETE FROM dbo.HrmLeaveRequest WHERE Id=@Id", new { Id = id }) > 0;
                if (changed) AddAudit(connection, actor.Id, "DELETE", "HrmLeaveRequest", id.ToString(), "Xóa yêu cầu", ipAddress);
                return changed;
            }
        }

        public void EnsureSampleLeaveRequests()
        {
            const string sql = @"
                SET QUOTED_IDENTIFIER ON;
                SET ANSI_NULLS ON;
                DELETE FROM dbo.HrmLeaveRequest WHERE Id > 5;
                IF EXISTS (SELECT 1 FROM dbo.HrmLeaveRequest WHERE Id=1)
                BEGIN
                    UPDATE dbo.HrmLeaveRequest SET UserId=4, StatusCode='PENDING_MANAGER', LeaveType=N'Nghỉ phép', Reason=N'Trưởng phòng xin nghỉ phép thường niên', ManagerNote=NULL, HrNote=NULL, ApprovedByManagerId=NULL, ApprovedByHrId=NULL WHERE Id=1;
                    UPDATE dbo.HrmLeaveRequest SET UserId=5, StatusCode='PENDING_MANAGER', LeaveType=N'Công tác/Ra ngoài', Reason=N'Gặp đối tác tại văn phòng chi nhánh', ApprovedByManagerId=NULL, ManagerNote=NULL, HrNote=NULL, ApprovedByHrId=NULL WHERE Id=2;
                    UPDATE dbo.HrmLeaveRequest SET UserId=5, StatusCode='PENDING_MANAGER', LeaveType=N'Đi muộn về sớm', Reason=N'Đi khám sức khỏe định kỳ buổi sáng tại bệnh viện', ManagerNote=NULL, HrNote=NULL, ApprovedByManagerId=NULL, ApprovedByHrId=NULL WHERE Id=3;
                    UPDATE dbo.HrmLeaveRequest SET UserId=5, StatusCode='PENDING_MANAGER', LeaveType=N'Làm thêm giờ', Reason=N'OT triển khai hệ thống server và bảo trì định kỳ cho công ty', ManagerNote=NULL, HrNote=NULL, ApprovedByManagerId=NULL, ApprovedByHrId=NULL WHERE Id=4;
                    UPDATE dbo.HrmLeaveRequest SET UserId=5, StatusCode='PENDING_MANAGER', LeaveType=N'Tạm ứng lương', Reason=N'Đề nghị tạm ứng chi tiêu gia đình đầu tháng', ApprovedByManagerId=NULL, ManagerNote=NULL, HrNote=NULL, ApprovedByHrId=NULL WHERE Id=5;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.HrmLeaveRequest(UserId, LeaveType, StartDate, EndDate, SessionCode, HandoverTo, Reason, StatusCode, CreatedAt)
                    VALUES
                    (4, N'Nghỉ phép', DATEADD(DAY, 4, GETDATE()), DATEADD(DAY, 5, GETDATE()), N'Cả ngày', N'Nguyễn Văn A', N'Trưởng phòng xin nghỉ phép thường niên', 'PENDING_MANAGER', DATEADD(HOUR, -2, GETDATE())),
                    (5, N'Công tác/Ra ngoài', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Cả ngày', N'Nguyễn Văn A', N'Gặp đối tác tại văn phòng chi nhánh', 'PENDING_MANAGER', DATEADD(HOUR, -4, GETDATE())),
                    (5, N'Đi muộn về sớm', DATEADD(DAY, 2, GETDATE()), DATEADD(DAY, 2, GETDATE()), N'Buổi sáng', N'Trần Thị B', N'Đi khám sức khỏe định kỳ buổi sáng tại bệnh viện', 'PENDING_MANAGER', DATEADD(HOUR, -6, GETDATE())),
                    (5, N'Làm thêm giờ', DATEADD(DAY, 1, GETDATE()), DATEADD(DAY, 1, GETDATE()), N'Tối', N'Nguyễn Văn A', N'OT triển khai hệ thống server và bảo trì định kỳ cho công ty', 'PENDING_MANAGER', DATEADD(HOUR, -8, GETDATE())),
                    (5, N'Tạm ứng lương', GETDATE(), GETDATE(), N'Cả ngày', NULL, N'Đề nghị tạm ứng chi tiêu gia đình đầu tháng', 'PENDING_MANAGER', DATEADD(HOUR, -12, GETDATE()));
                END";
            using (var connection = OpenConnection())
            {
                connection.Execute(sql);
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
                if (target == null) return false;

                // Không được tự phê duyệt đơn của chính mình
                if (target.UserId == actor.Id) return false;

                // Đơn của Trưởng phòng: chỉ Giám đốc hoặc Quản trị hệ thống mới được duyệt
                if (target.RoleCode == HrmRoles.Manager && actor.RoleCode == HrmRoles.Manager)
                {
                    return false;
                }

                var sql = @"UPDATE dbo.HrmLeaveRequest SET StatusCode=@Status, ManagerNote=@Note, ApprovedByManagerId=@ActorId, UpdatedAt=SYSDATETIME()
                    WHERE Id=@Id AND StatusCode IN ('PENDING_MANAGER','PENDING_HR')";
                var param = new { Status = request.Approve ? "APPROVED" : "REJECTED", request.Note, ActorId = actor.Id, request.Id };

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
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            string sql;
            object parameters;
            if (actor.RoleCode == HrmRoles.Manager)
            {
                sql = @"UPDATE r SET StatusCode='PENDING_HR', ManagerNote=@Note,
                        ApprovedByManagerId=@ActorId, UpdatedAt=SYSDATETIME()
                    OUTPUT INSERTED.Id
                    FROM dbo.HrmLeaveRequest r
                    INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                    WHERE r.StatusCode='PENDING_MANAGER' AND u.DepartmentId=@DepartmentId";
                parameters = new { Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(), ActorId = actor.Id, actor.DepartmentId };
            }
            else
            {
                sql = @"UPDATE dbo.HrmLeaveRequest SET StatusCode='APPROVED', HrNote=@Note,
                        ApprovedByHrId=@ActorId, UpdatedAt=SYSDATETIME()
                    OUTPUT INSERTED.Id
                    WHERE StatusCode IN ('PENDING_HR','PENDING_MANAGER')";
                parameters = new { Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(), ActorId = actor.Id };
            }
            var ids = connection.Query<int>(sql, parameters, transaction).ToList();
            if (ids.Count > 0)
                AddAudit(connection, actor.Id, "APPROVE_ALL", "HrmLeaveRequest", string.Join(",", ids.Take(5)), $"Phê duyệt hàng loạt {ids.Count} đơn nghỉ phép", ipAddress, transaction);
            foreach (var id in ids) NotifyLeave(connection, transaction, id, actor.RoleCode == HrmRoles.Manager ? "Trưởng phòng đã duyệt, chờ HR" : "Đã phê duyệt", note, actor.RoleCode == HrmRoles.Manager);
            transaction.Commit();
            return ids.Count;
        }

        public IList<CommunicationModel> GetCommunications(HrmUserAccountModel actor, string keyword, string category, string status = "", int take = 100)
        {
            const string sql = @"SELECT TOP (@Take) c.Id, c.AuthorUserId, u.DisplayName AuthorName, c.Category, c.ScopeCode,
                c.DepartmentId, c.Title, c.Body, c.AttachmentName, c.AttachmentContentType, c.IsPinned,
                COALESCE(c.StatusCode, CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END) StatusCode,
                c.ApprovedByUserId, approver.DisplayName ApprovedByName, c.ReviewNote,
                COALESCE(c.SubmittedAt,c.PublishedAt) SubmittedAt, c.PublishedAt,
                (SELECT COUNT(1) FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id) LikeCount,
                (SELECT COUNT(1) FROM dbo.HrmCommunicationComment m WHERE m.CommunicationId=c.Id AND m.IsDeleted=0) CommentCount,
                CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id AND r.UserId=@ActorId) THEN 1 ELSE 0 END AS BIT) LikedByCurrentUser,
                CAST(CASE WHEN c.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS BIT) IsMine
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
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
                const string commentSql = @"SELECT m.Id, m.CommunicationId, m.AuthorUserId, u.DisplayName AuthorName, m.Body, m.CreatedAt,
                    CAST(CASE WHEN m.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS BIT) IsMine
                    FROM dbo.HrmCommunicationComment m INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId
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
                SELECT c.Id, c.AuthorUserId, u.DisplayName AuthorName, c.Category, c.ScopeCode, c.DepartmentId,
                    c.Title, c.Body, c.AttachmentName, c.AttachmentContentType, c.IsPinned, c.StatusCode,
                    c.SubmittedAt, c.PublishedAt, CAST(1 AS BIT) IsMine
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId WHERE c.Id=@Id;";
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
                    SELECT m.Id,m.CommunicationId,m.AuthorUserId,u.DisplayName AuthorName,m.Body,m.CreatedAt,CAST(1 AS BIT) IsMine
                    FROM dbo.HrmCommunicationComment m INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId WHERE m.Id=@Id;
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

        private class AttendanceProjection : AttendanceRecordModel
        {
            public int GraceMinutes { get; set; }
        }

        public IList<AttendanceRecordModel> GetAttendance(HrmUserAccountModel actor, DateTime fromDate, DateTime toDate)
        {
            const string sql = @"WITH Events AS (
                    SELECT e.UserId, CAST(e.CheckTime AS DATE) WorkDate, MIN(e.CheckTime) CheckIn, MAX(e.CheckTime) CheckOut
                    FROM dbo.HrmAttendanceEvent e WHERE e.UserId IS NOT NULL AND e.CheckTime>=@FromDate AND e.CheckTime<DATEADD(DAY,1,@ToDate)
                    GROUP BY e.UserId, CAST(e.CheckTime AS DATE)
                )
                SELECT e.UserId, hm.PersonId, p.EmployeeCode, u.DisplayName, p.JobTitle, d.Name DepartmentName, e.WorkDate, s.ShiftName,
                    s.StartTime ScheduledStart, s.EndTime ScheduledEnd, s.GraceMinutes, e.CheckIn, e.CheckOut, 'HANET' Source
                FROM Events e INNER JOIN dbo.HrmUserAccount u ON u.Id=e.UserId
                LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
                LEFT JOIN dbo.HrmHanetPersonMap hm ON hm.UserId=u.Id AND hm.IsActive=1
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                OUTER APPLY (SELECT TOP 1 x.ShiftName, x.StartTime, x.EndTime, x.GraceMinutes
                    FROM dbo.HrmEmployeeSchedule x WHERE x.UserId=e.UserId AND x.StatusCode='ACTIVE'
                      AND x.EffectiveFrom<=e.WorkDate AND (x.EffectiveTo IS NULL OR x.EffectiveTo>=e.WorkDate)
                    ORDER BY x.EffectiveFrom DESC, x.Id DESC) s
                WHERE (@CanSeeAll=1 OR e.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY e.WorkDate DESC, u.DisplayName";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection())
            {
                var rows = connection.Query<AttendanceProjection>(sql, new { FromDate = fromDate.Date, ToDate = toDate.Date, CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, actor.DepartmentId }).ToList();
                foreach (var row in rows)
                {
                    if (!row.ScheduledStart.HasValue || !row.ScheduledEnd.HasValue)
                    {
                        row.StatusCode = "MISSING_SCHEDULE";
                        continue;
                    }
                    var scheduledStart = row.WorkDate.Date.Add(row.ScheduledStart.Value);
                    var scheduledEnd = row.WorkDate.Date.Add(row.ScheduledEnd.Value);
                    if (scheduledEnd <= scheduledStart) scheduledEnd = scheduledEnd.AddDays(1);
                    row.WorkedMinutes = row.CheckIn.HasValue && row.CheckOut.HasValue ? Math.Max(0, (int)(row.CheckOut.Value - row.CheckIn.Value).TotalMinutes) : 0;
                    row.LateMinutes = row.CheckIn.HasValue ? Math.Max(0, (int)(row.CheckIn.Value - scheduledStart.AddMinutes(row.GraceMinutes)).TotalMinutes) : 0;
                    row.EarlyMinutes = row.CheckOut.HasValue ? Math.Max(0, (int)(scheduledEnd - row.CheckOut.Value).TotalMinutes) : 0;
                    if (!row.CheckIn.HasValue || !row.CheckOut.HasValue || row.CheckIn == row.CheckOut) row.StatusCode = "MISSING_CHECK";
                    else if (row.LateMinutes > 0 && row.EarlyMinutes > 0) row.StatusCode = "LATE_EARLY";
                    else if (row.LateMinutes > 0) row.StatusCode = "LATE";
                    else if (row.EarlyMinutes > 0) row.StatusCode = "EARLY";
                    else row.StatusCode = "ON_TIME";
                }
                return rows.Cast<AttendanceRecordModel>().ToList();
            }
        }

        public DashboardModel GetDashboard(HrmUserAccountModel actor)
        {
            var today = DateTime.Today;
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
                    PresentToday = attendance.Select(x => x.UserId).Distinct().Count(),
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
