using Dapper;
using NHIGIA.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHIGIA.Web.Infrastructure
{
    public class HrmDataStore
    {
        public static readonly HrmDataStore Instance = new HrmDataStore();
        private static readonly object LocalDbLock = new object();
        private static bool _localDbStarted;
        private static string _localDbPipeName;
        private string ConnectionString
        {
            get
            {
                var environmentValue = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING");
                if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue;
                return ConfigurationManager.ConnectionStrings["MainConnectionString"].ConnectionString;
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(GetEffectiveConnectionString());
            try
            {
                connection.Open();
                return connection;
            }
            catch (SqlException) when (IsLocalDbConnection())
            {
                connection.Dispose();
                connection = new SqlConnection(GetEffectiveConnectionString(true));
                connection.Open();
                return connection;
            }
        }

        private string GetEffectiveConnectionString(bool forceLocalDbRestart = false)
        {
            if (!IsLocalDbConnection()) return ConnectionString;
            EnsureLocalDbStarted(forceLocalDbRestart);
            if (string.IsNullOrWhiteSpace(_localDbPipeName)) return ConnectionString;

            var builder = new SqlConnectionStringBuilder(ConnectionString) { DataSource = _localDbPipeName };
            return builder.ConnectionString;
        }

        private bool IsLocalDbConnection()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            return Regex.IsMatch(builder.DataSource ?? string.Empty, @"^\(localdb\)\\[A-Za-z0-9_-]+$", RegexOptions.IgnoreCase);
        }

        private void EnsureLocalDbStarted(bool force = false)
        {
            if (_localDbStarted && !force) return;
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            var match = Regex.Match(builder.DataSource ?? string.Empty, @"^\(localdb\)\\(?<name>[A-Za-z0-9_-]+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                _localDbStarted = true;
                return;
            }

            lock (LocalDbLock)
            {
                if (_localDbStarted && !force) return;
                var instanceName = match.Groups["name"].Value;
                RunLocalDb("start \"" + instanceName + "\"");
                var instanceInfo = RunLocalDb("info \"" + instanceName + "\"");
                var pipeMatch = Regex.Match(instanceInfo, @"Instance pipe name:\s*(?<pipe>np:[^\r\n]+)", RegexOptions.IgnoreCase);
                if (!pipeMatch.Success)
                {
                    throw new InvalidOperationException("SQL LocalDB đã khởi động nhưng không cung cấp named pipe cho instance " + instanceName + ".");
                }
                _localDbPipeName = pipeMatch.Groups["pipe"].Value.Trim();
                _localDbStarted = true;
            }
        }

        private static string RunLocalDb(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sqllocaldb.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit(15000);
                if (!process.HasExited)
                {
                    process.Kill();
                    throw new InvalidOperationException("SQL LocalDB không phản hồi trong 15 giây.");
                }
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("Không thể điều khiển SQL LocalDB: " + error);
                }
                return output;
            }
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
            const string sql = @"SELECT u.Id UserId, u.Username, u.DisplayName, u.RoleCode, d.Name DepartmentName,
                supervisor.DisplayName SupervisorName, u.IsActive,
                p.EmployeeCode, p.AvatarUrl, p.Gender, p.DateOfBirth, p.PlaceOfBirth, p.Nationality,
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
            const string sql = @"SELECT u.Id, u.Username, u.DisplayName, u.RoleCode, u.DepartmentId, d.Name DepartmentName,
                u.SupervisorUserId, u.IsActive
                FROM dbo.HrmUserAccount u LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
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
            const string sql = @"SELECT r.Id, r.RequestCode, r.UserId, u.Username, u.DisplayName, d.Name DepartmentName,
                r.LeaveType, r.StartDate, r.EndDate, r.SessionCode, r.HandoverTo, r.Reason, r.AttachmentName,
                r.StatusCode, r.ManagerNote, r.HrNote, r.CreatedAt
                FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
                WHERE (@CanSeeAll=1 OR r.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
                ORDER BY r.CreatedAt DESC";
            var canSeeAll = actor.RoleCode == HrmRoles.Admin || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director;
            using (var connection = OpenConnection()) return connection.Query<LeaveRequestModel>(sql, new { CanSeeAll = canSeeAll, IsManager = actor.RoleCode == HrmRoles.Manager, ActorId = actor.Id, DepartmentId = actor.DepartmentId }).ToList();
        }

        public LeaveStatsModel GetLeaveStats(HrmUserAccountModel actor)
        {
            const string sql = @"SELECT
                CAST(12 AS DECIMAL(10,1)) AnnualAllowance,
                CAST(COALESCE(SUM(CASE WHEN StatusCode='APPROVED' THEN DATEDIFF(DAY, StartDate, EndDate)+1 ELSE 0 END),0) AS DECIMAL(10,1)) UsedDays,
                COALESCE(SUM(CASE WHEN StatusCode IN ('PENDING_MANAGER','PENDING_HR') THEN 1 ELSE 0 END),0) PendingCount,
                COALESCE(SUM(CASE WHEN StatusCode='APPROVED' THEN 1 ELSE 0 END),0) ApprovedCount
                FROM dbo.HrmLeaveRequest WHERE UserId=@UserId AND YEAR(StartDate)=YEAR(GETDATE())";
            using (var connection = OpenConnection()) return connection.QuerySingle<LeaveStatsModel>(sql, new { UserId = actor.Id });
        }

        public LeaveRequestModel CreateLeave(CreateLeaveRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            const string sql = @"INSERT dbo.HrmLeaveRequest(UserId, LeaveType, StartDate, EndDate, SessionCode, HandoverTo, Reason, AttachmentName)
                VALUES(@UserId, @LeaveType, @StartDate, @EndDate, @SessionCode, @HandoverTo, @Reason, @AttachmentName);
                DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
                SELECT r.Id, r.RequestCode, r.UserId, u.Username, u.DisplayName, d.Name DepartmentName,
                    r.LeaveType, r.StartDate, r.EndDate, r.SessionCode, r.HandoverTo, r.Reason, r.AttachmentName, r.StatusCode, r.CreatedAt
                FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId WHERE r.Id=@Id;";
            using (var connection = OpenConnection())
            {
                var result = connection.QuerySingle<LeaveRequestModel>(sql, new { UserId = actor.Id, request.LeaveType, request.StartDate, request.EndDate, request.SessionCode, request.HandoverTo, request.Reason, request.AttachmentName });
                AddAudit(connection, actor.Id, "CREATE", "HrmLeaveRequest", result.Id.ToString(), "Gửi đơn nghỉ phép " + result.RequestCode, ipAddress);
                return result;
            }
        }

        public bool CancelLeave(int id, HrmUserAccountModel actor, string ipAddress)
        {
            using (var connection = OpenConnection())
            {
                var changed = connection.Execute("UPDATE dbo.HrmLeaveRequest SET StatusCode='CANCELLED', UpdatedAt=SYSDATETIME() WHERE Id=@Id AND UserId=@UserId AND StatusCode IN ('PENDING_MANAGER','PENDING_HR')", new { Id = id, UserId = actor.Id }) > 0;
                if (changed) AddAudit(connection, actor.Id, "CANCEL", "HrmLeaveRequest", id.ToString(), "Hủy đơn nghỉ phép", ipAddress);
                return changed;
            }
        }

        public bool ApproveLeave(ApprovalRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            string sql;
            object param;
            if (actor.RoleCode == HrmRoles.Manager)
            {
                sql = @"UPDATE r SET StatusCode=@Status, ManagerNote=@Note, ApprovedByManagerId=@ActorId, UpdatedAt=SYSDATETIME()
                    FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                    WHERE r.Id=@Id AND r.StatusCode='PENDING_MANAGER' AND u.DepartmentId=@DepartmentId";
                param = new { Status = request.Approve ? "PENDING_HR" : "REJECTED", request.Note, ActorId = actor.Id, request.Id, actor.DepartmentId };
            }
            else
            {
                sql = @"UPDATE dbo.HrmLeaveRequest SET StatusCode=@Status, HrNote=@Note, ApprovedByHrId=@ActorId, UpdatedAt=SYSDATETIME()
                    WHERE Id=@Id AND StatusCode IN ('PENDING_HR','PENDING_MANAGER')";
                param = new { Status = request.Approve ? "APPROVED" : "REJECTED", request.Note, ActorId = actor.Id, request.Id };
            }
            using (var connection = OpenConnection())
            {
                var changed = connection.Execute(sql, param) > 0;
                if (changed) AddAudit(connection, actor.Id, request.Approve ? "APPROVE" : "REJECT", "HrmLeaveRequest", request.Id.ToString(), request.Note, ipAddress);
                return changed;
            }
        }

        public IList<CommunicationModel> GetCommunications(HrmUserAccountModel actor, string keyword, string category, int take = 100)
        {
            const string sql = @"SELECT TOP (@Take) c.Id, c.AuthorUserId, u.DisplayName AuthorName, c.Category, c.ScopeCode,
                c.DepartmentId, c.Title, c.Body, c.AttachmentName, c.IsPinned, c.PublishedAt
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
                WHERE c.IsPublished=1
                  AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                    OR (c.ScopeCode='MANAGER' AND @IsManager=1) OR c.AuthorUserId=@ActorId)
                  AND (@Keyword='' OR c.Title LIKE '%' + @Keyword + '%' OR c.Body LIKE '%' + @Keyword + '%')
                  AND (@Category='' OR c.Category=@Category)
                ORDER BY c.IsPinned DESC, c.PublishedAt DESC";
            var isManager = actor.RoleCode == HrmRoles.Manager || actor.RoleCode == HrmRoles.Hr || actor.RoleCode == HrmRoles.Director || actor.RoleCode == HrmRoles.Admin;
            using (var connection = OpenConnection()) return connection.Query<CommunicationModel>(sql, new { Take = take, actor.DepartmentId, IsManager = isManager, ActorId = actor.Id, Keyword = keyword ?? string.Empty, Category = category ?? string.Empty }).ToList();
        }

        public CommunicationModel CreateCommunication(CreateCommunicationRequest request, HrmUserAccountModel actor, string ipAddress)
        {
            var scope = (request.ScopeCode ?? "DEPARTMENT").ToUpperInvariant();
            var departmentId = scope == "DEPARTMENT" ? actor.DepartmentId : null;
            const string sql = @"INSERT dbo.HrmCommunication(AuthorUserId, Category, ScopeCode, DepartmentId, Title, Body, AttachmentName, IsPinned)
                VALUES(@AuthorUserId, @Category, @ScopeCode, @DepartmentId, @Title, @Body, @AttachmentName, @IsPinned);
                DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
                SELECT c.Id, c.AuthorUserId, u.DisplayName AuthorName, c.Category, c.ScopeCode, c.DepartmentId,
                    c.Title, c.Body, c.AttachmentName, c.IsPinned, c.PublishedAt
                FROM dbo.HrmCommunication c INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId WHERE c.Id=@Id;";
            using (var connection = OpenConnection())
            {
                var result = connection.QuerySingle<CommunicationModel>(sql, new { AuthorUserId = actor.Id, request.Category, ScopeCode = scope, DepartmentId = departmentId, request.Title, request.Body, request.AttachmentName, request.IsPinned });
                AddAudit(connection, actor.Id, "PUBLISH", "HrmCommunication", result.Id.ToString(), result.Title, ipAddress);
                return result;
            }
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
                SELECT e.UserId, u.DisplayName, d.Name DepartmentName, e.WorkDate, s.ShiftName,
                    s.StartTime ScheduledStart, s.EndTime ScheduledEnd, s.GraceMinutes, e.CheckIn, e.CheckOut, 'HANET' Source
                FROM Events e INNER JOIN dbo.HrmUserAccount u ON u.Id=e.UserId
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
            var communications = GetCommunications(actor, string.Empty, string.Empty, 3);
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
                    settings.ClientSecret = HrmSecretProtector.Unprotect(settings.ClientSecret);
                    settings.AccessToken = HrmSecretProtector.Unprotect(settings.AccessToken);
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
            const string sql = @"UPDATE dbo.HrmHanetSettings SET ApiBaseUrl=@ApiBaseUrl, OAuthTokenUrl=@OAuthTokenUrl,
                ClientId=@ClientId, ProtectedClientSecret=COALESCE(@ProtectedClientSecret, ProtectedClientSecret),
                ProtectedAccessToken=COALESCE(@ProtectedAccessToken, ProtectedAccessToken), PlaceId=@PlaceId,
                WebhookSecret=@WebhookSecret, IsEnabled=@IsEnabled, UpdatedByUserId=@UserId, UpdatedAt=SYSDATETIME() WHERE Id=1";
            var protectedSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret) && settings.ClientSecret != "********" ? HrmSecretProtector.Protect(settings.ClientSecret) : null;
            var protectedToken = !string.IsNullOrWhiteSpace(settings.AccessToken) && settings.AccessToken != "********" ? HrmSecretProtector.Protect(settings.AccessToken) : null;
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
            const string sql = @"MERGE dbo.HrmHanetPersonMap AS target USING (SELECT @UserId UserId) AS source ON target.UserId=source.UserId
                WHEN MATCHED THEN UPDATE SET AliasId=@AliasId, PersonId=@PersonId, PlaceId=@PlaceId, IsActive=1, UpdatedAt=SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT(UserId, AliasId, PersonId, PlaceId) VALUES(@UserId, @AliasId, @PersonId, @PlaceId);";
            using (var connection = OpenConnection())
            {
                connection.Execute(sql, request);
                AddAudit(connection, actor.Id, "UPDATE", "HrmHanetPersonMap", request.UserId.ToString(), "Ánh xạ nhân viên HANET", ipAddress);
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

        private static void AddAudit(IDbConnection connection, int? userId, string action, string entityType, string entityId, string detail, string ipAddress)
        {
            connection.Execute("INSERT dbo.HrmAuditLog(UserId, ActionCode, EntityType, EntityId, Detail, IpAddress) VALUES(@UserId,@Action,@EntityType,@EntityId,@Detail,@IpAddress)", new { UserId = userId, Action = action, EntityType = entityType, EntityId = entityId, Detail = detail, IpAddress = ipAddress });
        }
    }
}
