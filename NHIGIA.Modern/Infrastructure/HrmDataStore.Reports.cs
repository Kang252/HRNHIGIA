using Dapper;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public partial class HrmDataStore
{
    public HrReportSummaryModel GetReportSummary(HrmUserAccountModel actor, DateTime fromDate, DateTime toDate)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        if (to < from || (to - from).TotalDays > 366)
            throw new InvalidOperationException("Khoảng báo cáo tối đa là 366 ngày.");

        var attendance = GetAttendance(actor, from, to).ToList();
        var canSeeAll = actor.RoleCode is HrmRoles.Admin or HrmRoles.Hr or HrmRoles.Director;
        var isManager = actor.RoleCode == HrmRoles.Manager;
        using var connection = OpenConnection();
        var args = new
        {
            From = from,
            ToExclusive = to.AddDays(1),
            CanSeeAll = canSeeAll,
            IsManager = isManager,
            ActorId = actor.Id,
            actor.DepartmentId
        };

        var summary = new HrReportSummaryModel
        {
            FromDate = from,
            ToDate = to,
            EmployeeCount = connection.ExecuteScalar<int>(@"SELECT COUNT(1)
                FROM dbo.HrmUserAccount u
                WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
                  AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))", args),
            RecordedWorkdays = attendance.Count(x => x.EventCount > 0),
            WorkedMinutes = attendance.Where(x => !x.IsProvisional).Sum(x => x.WorkedMinutes),
            OnLeaveDays = attendance
                .Where(x => x.ApprovedLeave)
                .Sum(x => string.Equals(x.LeaveSession, "Cả ngày", StringComparison.OrdinalIgnoreCase) ? 1m : 0.5m),
            LateOrEarlyCount = attendance.Count(x => x.LateMinutes > 0 || x.EarlyMinutes > 0),
            MissingAttendanceCount = attendance.Count(x => x.StatusCode is "MISSING_CHECK" or "MISSING_SCHEDULE"),
            PendingLeaveCount = connection.ExecuteScalar<int>(@"SELECT COUNT(1)
                FROM dbo.HrmLeaveRequest r INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
                WHERE r.StatusCode IN ('PENDING_MANAGER','PENDING_HR')
                  AND (@CanSeeAll=1 OR r.UserId=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))", args)
        };

        if (canSeeAll)
        {
            summary.RecruitmentCandidateCount = connection.ExecuteScalar<int>(@"SELECT COUNT(1)
                FROM dbo.HrmRecruitmentCandidate
                WHERE ReceivedAt>=@From AND ReceivedAt<@ToExclusive", args);
        }

        summary.CompletedResignationCount = connection.ExecuteScalar<int>(@"SELECT COUNT(1)
            FROM dbo.HrmWorkItem w
            LEFT JOIN dbo.HrmUserAccount u ON u.Id=COALESCE(w.EmployeeId,w.CreatedBy)
            WHERE w.Kind='resignation' AND w.Status='RESOLVED'
              AND COALESCE(w.UpdatedAt,w.CreatedAt)>=@From AND COALESCE(w.UpdatedAt,w.CreatedAt)<@ToExclusive
              AND (@CanSeeAll=1 OR COALESCE(w.EmployeeId,w.CreatedBy)=@ActorId
                   OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))", args);

        summary.EmploymentStatuses = connection.Query<EmploymentStatusStatModel>(@"SELECT
                CASE
                    WHEN u.IsActive=0 OR p.EmploymentStatus IN (N'Đã nghỉ việc',N'Đã thôi việc') THEN N'Đã thôi việc'
                    WHEN p.EmploymentStatus=N'Thử việc' THEN N'Thử việc'
                    WHEN p.EmploymentStatus=N'Thực tập sinh' OR p.JobTitle LIKE N'%thực tập%' THEN N'Thực tập sinh'
                    ELSE N'Nhân viên chính thức'
                END Status,
                COUNT(1) [Count]
            FROM dbo.HrmUserAccount u
            LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
            WHERE u.RoleCode<>'ADMIN'
              AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
            GROUP BY CASE
                WHEN u.IsActive=0 OR p.EmploymentStatus IN (N'Đã nghỉ việc',N'Đã thôi việc') THEN N'Đã thôi việc'
                WHEN p.EmploymentStatus=N'Thử việc' THEN N'Thử việc'
                WHEN p.EmploymentStatus=N'Thực tập sinh' OR p.JobTitle LIKE N'%thực tập%' THEN N'Thực tập sinh'
                ELSE N'Nhân viên chính thức'
            END
            ORDER BY [Count] DESC", args).ToList();

        return summary;
    }
}
