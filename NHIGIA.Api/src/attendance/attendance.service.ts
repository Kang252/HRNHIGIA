import {
  BadRequestException,
  ConflictException,
  Injectable,
} from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SchemaReadinessService } from "../database/schema-readiness.service";
import { SqlServerService } from "../database/sql-server.service";
import { AttendancePeriodDto } from "./attendance-period.dto";
import type { ListAttendanceDto } from "./list-attendance.dto";

const maxAttendanceRangeDays = 366;
const vietnamTimeZone = "Asia/Bangkok";

// These are the same leave types used by AttendanceLeavePolicy in the active
// C# service.  They are static SQL literals, not caller-supplied SQL.
const approvedLeaveTypesSql = `N'Phép năm',N'Nghỉ phép',N'Nghỉ ốm',N'Nghỉ không lương',N'Nghỉ kết hôn',N'Nghỉ thai sản',N'Nghỉ việc riêng',N'Nghỉ bù',N'Khác'`;

export interface AttendanceListData {
  scope: "SELF" | "DEPARTMENT" | "COMPANY";
  fromDate: string;
  toDate: string;
  records: AttendanceRecord[];
}

export interface AttendanceRecord {
  userId: number;
  employeeCode: string | null;
  displayName: string;
  jobTitle: string | null;
  departmentName: string | null;
  workDate: string;
  shiftName: string | null;
  scheduledStart: string | null;
  scheduledEnd: string | null;
  graceMinutes: number;
  breakMinutes: number;
  approvedLeave: boolean;
  leaveSession: string | null;
  leaveDescription: string | null;
  expectedStartAt: string | null;
  expectedEndAt: string | null;
  checkIn: string | null;
  checkOut: string | null;
  lastSeen: string | null;
  eventCount: number;
  isProvisional: boolean;
  hasExplicitPunches: boolean;
  isFlexible: boolean;
  requiredMinutes: number;
  workedMinutes: number;
  lateMinutes: number;
  earlyMinutes: number;
  statusCode: string;
  source: string;
}

export interface AttendancePeriodData {
  period: string;
  statusCode: string;
  isConfirmed: boolean;
  submittedAt: string | null;
  lockedAt: string | null;
  lockedByName: string | null;
  unlockReason: string | null;
  employeeCount: number;
  confirmedCount: number;
  pendingAdjustmentCount: number;
  pendingRemoteCount: number;
}

export interface AttendanceScope {
  canSeeAll: boolean;
  isManager: boolean;
}

interface RawAttendanceRow {
  UserId: number;
  EmployeeCode: string | null;
  DisplayName: string;
  JobTitle: string | null;
  DepartmentName: string | null;
  WorkDate: string;
  ShiftName: string | null;
  ScheduledStart: string | null;
  ScheduledEnd: string | null;
  GraceMinutes: number | null;
  BreakMinutes: number | null;
  HasExplicitPunches: boolean | number | null;
  IsFlexible: boolean | number | null;
  RequiredMinutes: number | null;
  CheckIn: string | null;
  CheckOut: string | null;
  LastSeen: string | null;
  EventCount: number | null;
  Source: string;
}

interface ScheduleRow {
  Id: number;
  UserId: number;
  ShiftName: string;
  StartTime: string;
  EndTime: string;
  BreakMinutes: number;
  GraceMinutes: number;
  WorkDaysMask: number;
  EffectiveFrom: string;
  EffectiveTo: string | null;
  StatusCode: string;
}

interface LeaveRow {
  Id: number;
  RequestCode: string;
  UserId: number;
  EmployeeCode: string | null;
  DisplayName: string;
  JobTitle: string | null;
  DepartmentName: string | null;
  LeaveType: string;
  StartDate: string;
  EndDate: string;
  SessionCode: string | null;
}

interface AttendancePeriodRow {
  Period: string;
  StatusCode: string | null;
  IsConfirmed: boolean | number | null;
  SubmittedAt: string | null;
  LockedAt: string | null;
  LockedByName: string | null;
  UnlockReason: string | null;
  EmployeeCount: number | null;
  ConfirmedCount: number | null;
  PendingAdjustmentCount: number | null;
  PendingRemoteCount: number | null;
}

export interface AttendanceRange {
  fromDate: string;
  toDate: string;
  from: Date;
  to: Date;
}

@Injectable()
export class AttendanceService {
  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly schemaReadiness: SchemaReadinessService,
  ) {}

  public async list(
    identity: AuthenticatedIdentity,
    input: ListAttendanceDto,
  ): Promise<AttendanceListData> {
    await this.schemaReadiness.assertReady("attendance");
    const range = resolveAttendanceRange(input);
    const scope = attendanceScopeFor(identity);
    const pool = await this.sqlServer.getPool();
    const queryArgs = {
      from: range.from,
      to: range.to,
      targetUserId: input.userId ?? null,
      scope,
      identity,
    };
    const [attendance, schedules, leaves] = await Promise.all([
      queryRawAttendance(pool, queryArgs),
      querySchedules(pool, queryArgs),
      queryApprovedLeaves(pool, queryArgs),
    ]);

    return {
      scope: scope.canSeeAll
        ? "COMPANY"
        : scope.isManager
          ? "DEPARTMENT"
          : "SELF",
      fromDate: range.fromDate,
      toDate: range.toDate,
      records: projectAttendance(
        attendance,
        schedules,
        leaves,
        range,
        new Date(),
      ),
    };
  }

  public async getPeriod(
    identity: AuthenticatedIdentity,
    period: string,
  ): Promise<AttendancePeriodData> {
    validateAttendancePeriod(period);
    await this.schemaReadiness.assertReady("attendance");
    const pool = await this.sqlServer.getPool();
    const result = await pool
      .request()
      .input("Period", sql.Char(7), period)
      .input("UserId", sql.Int, identity.userId)
      .query<AttendancePeriodRow>(attendancePeriodSql);
    return toAttendancePeriodData(result.recordset[0], period);
  }

  public async confirmPeriod(
    identity: AuthenticatedIdentity,
    period: string,
    ipAddress: string | undefined,
  ): Promise<void> {
    validateAttendancePeriod(period);
    await this.schemaReadiness.assertReady("attendance");
    const pool = await this.sqlServer.getPool();
    const transaction = new sql.Transaction(pool);
    let committed = false;
    await transaction.begin(sql.ISOLATION_LEVEL.READ_COMMITTED);
    try {
      const state = await new sql.Request(transaction).input(
        "Period",
        sql.Char(7),
        period,
      ).query<{ StatusCode: string }>(`
          SELECT StatusCode
          FROM dbo.HrmAttendancePeriod WITH (UPDLOCK,HOLDLOCK)
          WHERE Period=@Period;`);
      if (state.recordset[0]?.StatusCode === "LOCKED") {
        throw new ConflictException("Kỳ chấm công đã khóa.");
      }

      await new sql.Request(transaction)
        .input("Period", sql.Char(7), period)
        .input("UserId", sql.Int, identity.userId).query(`
          MERGE dbo.HrmAttendancePeriodConfirmation WITH (HOLDLOCK) AS target
          USING (SELECT @Period AS Period,@UserId AS UserId) AS source
            ON target.Period=source.Period AND target.UserId=source.UserId
          WHEN MATCHED THEN
            UPDATE SET StatusCode='SUBMITTED',SubmittedAt=SYSDATETIME()
          WHEN NOT MATCHED THEN
            INSERT(Period,UserId) VALUES(@Period,@UserId);`);

      await new sql.Request(transaction)
        .input("UserId", sql.Int, identity.userId)
        .input("Period", sql.Char(7), period)
        .input("IpAddress", sql.NVarChar(80), cleanIp(ipAddress)).query(`
          INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
          VALUES(@UserId,'SUBMIT','AttendancePeriod',@Period,
            N'Xác nhận dữ liệu chấm công tháng qua NHIGIA.Api',@IpAddress);`);

      await transaction.commit();
      committed = true;
    } catch (error: unknown) {
      if (!committed) await transaction.rollback().catch(() => undefined);
      throw error;
    }
  }
}

async function queryRawAttendance(
  pool: sql.ConnectionPool,
  args: QueryArguments,
): Promise<RawAttendanceRow[]> {
  const request = scopeRequest(pool, args);
  const result = await request.query<RawAttendanceRow>(rawAttendanceSql);
  return result.recordset;
}

async function querySchedules(
  pool: sql.ConnectionPool,
  args: QueryArguments,
): Promise<ScheduleRow[]> {
  const request = scopeRequest(pool, args);
  const result = await request.query<ScheduleRow>(schedulesSql);
  return result.recordset;
}

async function queryApprovedLeaves(
  pool: sql.ConnectionPool,
  args: QueryArguments,
): Promise<LeaveRow[]> {
  const request = scopeRequest(pool, args);
  const result = await request.query<LeaveRow>(approvedLeavesSql);
  return result.recordset;
}

interface QueryArguments {
  from: Date;
  to: Date;
  targetUserId: number | null;
  scope: AttendanceScope;
  identity: AuthenticatedIdentity;
}

function scopeRequest(
  pool: sql.ConnectionPool,
  args: QueryArguments,
): sql.Request {
  return pool
    .request()
    .input("FromDate", sql.Date, args.from)
    .input("ToDate", sql.Date, args.to)
    .input("TargetUserId", sql.Int, args.targetUserId)
    .input("CanSeeAll", sql.Bit, args.scope.canSeeAll)
    .input("IsManager", sql.Bit, args.scope.isManager)
    .input("ActorId", sql.Int, args.identity.userId)
    .input("DepartmentId", sql.Int, args.identity.departmentId ?? null);
}

// This CTE preserves the established event rules: remote punches must already
// be approved, locked periods only include evidence received before locking,
// and approved adjustments override the displayed in/out times.
const rawAttendanceSql = `
WITH RawPunches AS (
  SELECT e.UserId,e.CheckTime,e.CheckTime AS InCandidate,e.CheckTime AS OutCandidate,
    CAST(0 AS bit) AS IsRemote
  FROM dbo.HrmAttendanceEvent e
  LEFT JOIN dbo.HrmAttendancePeriod ap ON ap.Period=CONVERT(char(7),e.CheckTime,126)
  WHERE e.UserId IS NOT NULL AND e.CheckTime>=@FromDate AND e.CheckTime<DATEADD(day,1,@ToDate)
    AND (ap.StatusCode IS NULL OR ap.StatusCode<>'LOCKED' OR e.ReceivedAt<=ap.LockedAt)
  UNION ALL
  SELECT r.UserId,r.CheckTime,
    CASE WHEN r.Kind='IN' THEN r.CheckTime END AS InCandidate,
    CASE WHEN r.Kind='OUT' THEN r.CheckTime END AS OutCandidate,
    CAST(1 AS bit) AS IsRemote
  FROM dbo.HrmRemotePunch r
  LEFT JOIN dbo.HrmAttendancePeriod ap ON ap.Period=CONVERT(char(7),r.CheckTime,126)
  WHERE r.StatusCode='APPROVED' AND r.Kind IN ('IN','OUT')
    AND r.CheckTime>=@FromDate AND r.CheckTime<DATEADD(day,1,@ToDate)
    AND (ap.StatusCode IS NULL OR ap.StatusCode<>'LOCKED' OR r.IncludedAt<=ap.LockedAt)
), EventRaw AS (
  SELECT e.UserId,CAST(e.CheckTime AS date) AS WorkDate,MIN(e.InCandidate) AS CheckIn,
    MAX(e.OutCandidate) AS OutTime,MAX(e.CheckTime) AS LastSeen,COUNT(1) AS EventCount,
    MAX(e.IsRemote) AS HasRemote,MIN(e.IsRemote) AS OnlyRemote
  FROM RawPunches e
  GROUP BY e.UserId,CAST(e.CheckTime AS date)
), Dates AS (
  SELECT UserId,WorkDate FROM EventRaw
  UNION
  SELECT UserId,WorkDate FROM dbo.HrmAttendanceAdjustment
  WHERE StatusCode='APPROVED' AND WorkDate BETWEEN @FromDate AND @ToDate
), Events AS (
  SELECT d.UserId,d.WorkDate,e.CheckIn,e.OutTime,e.LastSeen,COALESCE(e.EventCount,0) AS EventCount,
    e.HasRemote,e.OnlyRemote
  FROM Dates d
  LEFT JOIN EventRaw e ON e.UserId=d.UserId AND e.WorkDate=d.WorkDate
)
SELECT e.UserId,p.EmployeeCode,u.DisplayName,p.JobTitle,d.Name AS DepartmentName,
  CONVERT(char(10),e.WorkDate,23) AS WorkDate,
  CASE WHEN rp.Id IS NULL THEN s.ShiftName
       WHEN rp.Mode='HOME' THEN N'Làm tại nhà' ELSE N'Đi thị trường' END AS ShiftName,
  CONVERT(char(8),COALESCE(rp.WindowStart,s.StartTime),108) AS ScheduledStart,
  CONVERT(char(8),COALESCE(rp.WindowEnd,s.EndTime),108) AS ScheduledEnd,
  s.GraceMinutes,COALESCE(rp.BreakMinutes,s.BreakMinutes) AS BreakMinutes,
  CONVERT(bit,COALESCE(e.HasRemote,0)) AS HasExplicitPunches,
  COALESCE(rp.IsFlexible,0) AS IsFlexible,COALESCE(rp.RequiredMinutes,0) AS RequiredMinutes,
  CONVERT(char(19),COALESCE(a.RequestedCheckIn,e.CheckIn),126) AS CheckIn,
  CONVERT(char(19),COALESCE(a.RequestedCheckOut,
    CASE WHEN e.OutTime>e.CheckIn OR e.CheckIn IS NULL THEN e.OutTime END),126) AS CheckOut,
  CONVERT(char(19),e.LastSeen,126) AS LastSeen,e.EventCount,
  CASE WHEN a.Id IS NOT NULL THEN N'Điều chỉnh đã duyệt'
       WHEN e.OnlyRemote=1 THEN 'GPS + Photo'
       WHEN e.HasRemote=1 THEN 'HANET + GPS' ELSE 'HANET' END AS Source
FROM Events e
INNER JOIN dbo.HrmUserAccount u ON u.Id=e.UserId
LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
OUTER APPLY (
  SELECT TOP 1 wp.*
  FROM dbo.HrmRemoteWorkPlan wp
  JOIN dbo.HrmRemotePunch r ON r.PlanId=wp.Id AND r.UserId=e.UserId
  WHERE r.StatusCode='APPROVED' AND r.Kind IN ('IN','OUT')
    AND r.CheckTime>=e.WorkDate AND r.CheckTime<DATEADD(day,1,e.WorkDate)
    AND EXISTS(
      SELECT 1 FROM RawPunches raw
      WHERE raw.UserId=r.UserId AND raw.CheckTime=r.CheckTime AND raw.IsRemote=1
    )
  ORDER BY r.CheckTime DESC,r.Id DESC
) rp
OUTER APPLY (
  SELECT TOP 1 x.Id,x.RequestedCheckIn,x.RequestedCheckOut
  FROM dbo.HrmAttendanceAdjustment x
  WHERE x.UserId=e.UserId AND x.WorkDate=e.WorkDate AND x.StatusCode='APPROVED'
  ORDER BY x.ReviewedAt DESC,x.Id DESC
) a
OUTER APPLY (
  SELECT TOP 1 x.ShiftName,x.StartTime,x.EndTime,x.GraceMinutes,x.BreakMinutes
  FROM dbo.HrmEmployeeSchedule x
  WHERE x.UserId=e.UserId AND x.StatusCode='ACTIVE'
    AND x.EffectiveFrom<=e.WorkDate AND (x.EffectiveTo IS NULL OR x.EffectiveTo>=e.WorkDate)
    AND (x.WorkDaysMask & CASE ((DATEDIFF(day,CONVERT(date,'19000107'),e.WorkDate)%7+7)%7)
      WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 4 WHEN 3 THEN 8
      WHEN 4 THEN 16 WHEN 5 THEN 32 WHEN 6 THEN 64 END)<>0
  ORDER BY x.EffectiveFrom DESC,x.Id DESC
) s
WHERE (@TargetUserId IS NULL OR e.UserId=@TargetUserId)
  AND (@CanSeeAll=1 OR e.UserId=@ActorId
       OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))
ORDER BY e.WorkDate DESC,u.DisplayName,e.UserId;`;

const schedulesSql = `
SELECT s.Id,s.UserId,s.ShiftName,CONVERT(char(8),s.StartTime,108) AS StartTime,
  CONVERT(char(8),s.EndTime,108) AS EndTime,s.BreakMinutes,s.GraceMinutes,s.WorkDaysMask,
  CONVERT(char(10),s.EffectiveFrom,23) AS EffectiveFrom,
  CONVERT(char(10),s.EffectiveTo,23) AS EffectiveTo,s.StatusCode
FROM dbo.HrmEmployeeSchedule s
INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId
WHERE (@TargetUserId IS NULL OR s.UserId=@TargetUserId)
  AND (@CanSeeAll=1 OR u.Id=@ActorId
       OR (@IsManager=1 AND u.DepartmentId=@DepartmentId));`;

const approvedLeavesSql = `
SELECT r.Id,r.RequestCode,r.UserId,p.EmployeeCode,u.DisplayName,p.JobTitle,
  d.Name AS DepartmentName,r.LeaveType,CONVERT(char(10),r.StartDate,23) AS StartDate,
  CONVERT(char(10),r.EndDate,23) AS EndDate,r.SessionCode
FROM dbo.HrmLeaveRequest r
INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
WHERE r.StatusCode='APPROVED' AND r.StartDate<=@ToDate AND r.EndDate>=@FromDate
  AND r.LeaveType IN (${approvedLeaveTypesSql})
  AND (@TargetUserId IS NULL OR r.UserId=@TargetUserId)
  AND (@CanSeeAll=1 OR r.UserId=@ActorId
       OR (@IsManager=1 AND u.DepartmentId=@DepartmentId));`;

const attendancePeriodSql = `
SELECT @Period AS Period,COALESCE(p.StatusCode,'OPEN') AS StatusCode,
  CONVERT(bit,CASE WHEN c.UserId IS NULL THEN 0 ELSE 1 END) AS IsConfirmed,
  CONVERT(char(19),c.SubmittedAt,126) AS SubmittedAt,
  CONVERT(char(19),p.LockedAt,126) AS LockedAt,u.DisplayName AS LockedByName,p.UnlockReason,
  (SELECT COUNT(1) FROM dbo.HrmUserAccount x
    WHERE x.IsActive=1 AND x.RoleCode<>'ADMIN') AS EmployeeCount,
  (SELECT COUNT(1) FROM dbo.HrmAttendancePeriodConfirmation x
    INNER JOIN dbo.HrmUserAccount a ON a.Id=x.UserId
    WHERE x.Period=@Period AND x.StatusCode='SUBMITTED'
      AND a.IsActive=1 AND a.RoleCode<>'ADMIN') AS ConfirmedCount,
  (SELECT COUNT(1) FROM dbo.HrmAttendanceAdjustment x
    WHERE CONVERT(char(7),x.WorkDate,126)=@Period AND x.StatusCode='PENDING') AS PendingAdjustmentCount,
  (SELECT COUNT(1) FROM dbo.HrmRemotePunch x
    WHERE CONVERT(char(7),x.CheckTime,126)=@Period AND x.StatusCode='PENDING'
      AND x.Kind IN ('IN','OUT')) AS PendingRemoteCount
FROM (SELECT 1 AS n) seed
LEFT JOIN dbo.HrmAttendancePeriod p ON p.Period=@Period
LEFT JOIN dbo.HrmAttendancePeriodConfirmation c
  ON c.Period=@Period AND c.UserId=@UserId
LEFT JOIN dbo.HrmUserAccount u ON u.Id=p.LockedByUserId;`;

export function projectAttendance(
  rawRows: RawAttendanceRow[],
  scheduleRows: ScheduleRow[],
  leaveRows: LeaveRow[],
  range: AttendanceRange,
  now: Date,
): AttendanceRecord[] {
  const records = new Map<string, AttendanceRecord>();
  for (const rawRow of rawRows) {
    const record = recordFromRaw(rawRow);
    const key = attendanceKey(record.userId, record.workDate);
    const existing = records.get(key);
    if (!existing || existing.eventCount < record.eventCount)
      records.set(key, record);
  }

  const schedulesByUser = groupBy(scheduleRows, (schedule) => schedule.UserId);
  const leavesByUser = groupBy(leaveRows, (leave) => leave.UserId);
  for (const [userId, leaves] of leavesByUser) {
    for (const workDate of datesInRange(range.fromDate, range.toDate)) {
      const key = attendanceKey(userId, workDate);
      if (records.has(key)) continue;
      const activeLeaves = activeLeavesForDate(leaves, workDate);
      if (activeLeaves.length === 0) continue;
      const schedule = scheduleForDate(
        schedulesByUser.get(userId) ?? [],
        workDate,
      );
      // Multi-day leave does not fabricate an attendance row on a scheduled
      // day that does not exist.
      if (!schedule) continue;
      const owner = activeLeaves[0];
      if (!owner) continue;
      const record: AttendanceRecord = {
        userId,
        employeeCode: owner.EmployeeCode,
        displayName: owner.DisplayName,
        jobTitle: owner.JobTitle,
        departmentName: owner.DepartmentName,
        workDate,
        shiftName: schedule.ShiftName,
        scheduledStart: timeWithoutSeconds(schedule.StartTime),
        scheduledEnd: timeWithoutSeconds(schedule.EndTime),
        graceMinutes: schedule.GraceMinutes,
        breakMinutes: schedule.BreakMinutes,
        approvedLeave: false,
        leaveSession: null,
        leaveDescription: null,
        expectedStartAt: null,
        expectedEndAt: null,
        checkIn: null,
        checkOut: null,
        lastSeen: null,
        eventCount: 0,
        isProvisional: false,
        hasExplicitPunches: false,
        isFlexible: false,
        requiredMinutes: 0,
        workedMinutes: 0,
        lateMinutes: 0,
        earlyMinutes: 0,
        statusCode: "MISSING_SCHEDULE",
        source: "Nghỉ đã duyệt",
      };
      calculateAttendance(record, activeLeaves, now);
      if (record.approvedLeave) records.set(key, record);
    }
  }

  for (const record of records.values()) {
    calculateAttendance(
      record,
      activeLeavesForDate(
        leavesByUser.get(record.userId) ?? [],
        record.workDate,
      ),
      now,
    );
  }

  return [...records.values()].sort((left, right) => {
    const dateSort = right.workDate.localeCompare(left.workDate);
    if (dateSort !== 0) return dateSort;
    const nameSort = left.displayName.localeCompare(right.displayName, "vi");
    return nameSort !== 0 ? nameSort : left.userId - right.userId;
  });
}

function recordFromRaw(row: RawAttendanceRow): AttendanceRecord {
  return {
    userId: row.UserId,
    employeeCode: row.EmployeeCode,
    displayName: row.DisplayName,
    jobTitle: row.JobTitle,
    departmentName: row.DepartmentName,
    workDate: row.WorkDate,
    shiftName: row.ShiftName,
    scheduledStart: timeWithoutSeconds(row.ScheduledStart),
    scheduledEnd: timeWithoutSeconds(row.ScheduledEnd),
    graceMinutes: row.GraceMinutes ?? 0,
    breakMinutes: row.BreakMinutes ?? 0,
    approvedLeave: false,
    leaveSession: null,
    leaveDescription: null,
    expectedStartAt: null,
    expectedEndAt: null,
    checkIn: row.CheckIn,
    checkOut: row.CheckOut,
    lastSeen: row.LastSeen,
    eventCount: row.EventCount ?? 0,
    isProvisional: false,
    hasExplicitPunches: asBoolean(row.HasExplicitPunches),
    isFlexible: asBoolean(row.IsFlexible),
    requiredMinutes: row.RequiredMinutes ?? 0,
    workedMinutes: 0,
    lateMinutes: 0,
    earlyMinutes: 0,
    statusCode: "MISSING_SCHEDULE",
    source: row.Source,
  };
}

/** Mirrors the active C# AttendanceLeavePolicy calculation. */
function calculateAttendance(
  record: AttendanceRecord,
  leaves: LeaveRow[],
  now: Date,
): void {
  record.lateMinutes = 0;
  record.earlyMinutes = 0;
  record.workedMinutes = 0;
  record.isProvisional = false;
  record.approvedLeave = false;
  record.leaveSession = null;
  record.leaveDescription = null;
  record.expectedStartAt = null;
  record.expectedEndAt = null;

  const leaveMask = leaves.reduce(
    (mask, leave) => mask | leaveSessionMask(leave.SessionCode),
    0,
  );
  const scheduledStart = record.scheduledStart;
  const scheduledEnd = record.scheduledEnd;
  if (!scheduledStart || !scheduledEnd) {
    describeLeave(record, leaves, leaveMask);
    record.statusCode = leaveMask === 3 ? "ON_LEAVE" : "MISSING_SCHEDULE";
    return;
  }

  const start = vietnamDateTime(record.workDate, scheduledStart);
  let end = vietnamDateTime(record.workDate, scheduledEnd);
  if (end.getTime() <= start.getTime()) end = addDaysToInstant(end, 1);
  let expectedStart = start;
  let expectedEnd = end;
  const overnight = vietnamDatePart(end) !== vietnamDatePart(start);
  const noon = vietnamDateTime(record.workDate, "12:00");
  const split = overnight
    ? new Date(
        start.getTime() +
          Math.floor(
            (end.getTime() - start.getTime() - record.breakMinutes * 60000) / 2,
          ),
      )
    : noon;
  const afternoonStart = overnight
    ? new Date(split.getTime() + record.breakMinutes * 60000)
    : vietnamDateTime(record.workDate, "13:30");

  if ((leaveMask & 1) !== 0 && start.getTime() < split.getTime())
    expectedStart =
      end.getTime() < afternoonStart.getTime() ? end : afternoonStart;
  if ((leaveMask & 2) !== 0 && end.getTime() > split.getTime())
    expectedEnd = start.getTime() > split.getTime() ? start : split;
  if (leaveMask === 3) expectedStart = end;
  if (
    expectedStart.getTime() !== start.getTime() ||
    expectedEnd.getTime() !== end.getTime()
  ) {
    describeLeave(record, leaves, leaveMask);
  }

  if (expectedStart.getTime() >= expectedEnd.getTime()) {
    record.statusCode = "ON_LEAVE";
    return;
  }

  record.expectedStartAt = vietnamLocalIso(expectedStart);
  record.expectedEndAt = vietnamLocalIso(expectedEnd);
  const checkIn = parseVietnamLocalDateTime(record.checkIn);
  let checkOut = parseVietnamLocalDateTime(record.checkOut);
  const lastSeen = parseVietnamLocalDateTime(record.lastSeen);

  if (record.isFlexible) {
    const actualStart =
      checkIn && checkIn.getTime() > expectedStart.getTime()
        ? checkIn
        : expectedStart;
    const actualEnd =
      checkOut && checkOut.getTime() < expectedEnd.getTime()
        ? checkOut
        : expectedEnd;
    record.workedMinutes =
      checkIn && checkOut
        ? Math.max(
            0,
            Math.floor((actualEnd.getTime() - actualStart.getTime()) / 60000) -
              (record.approvedLeave ? 0 : record.breakMinutes),
          )
        : 0;
    const target = record.approvedLeave
      ? Math.ceil(record.requiredMinutes / 2)
      : record.requiredMinutes;
    record.isProvisional = now.getTime() < expectedEnd.getTime();
    record.statusCode = record.isProvisional
      ? "IN_PROGRESS"
      : !checkIn || !checkOut
        ? "MISSING_CHECK"
        : record.workedMinutes >= target
          ? "ON_TIME"
          : "INSUFFICIENT_HOURS";
    return;
  }

  record.lateMinutes = checkIn
    ? Math.max(
        0,
        Math.floor(
          (checkIn.getTime() -
            (expectedStart.getTime() + record.graceMinutes * 60000)) /
            60000,
        ),
      )
    : 0;
  if (now.getTime() < expectedEnd.getTime()) {
    record.isProvisional = true;
    record.checkOut = null;
    record.statusCode = "IN_PROGRESS";
    return;
  }

  if (
    !record.hasExplicitPunches &&
    record.eventCount > 1 &&
    lastSeen &&
    (!checkIn || lastSeen.getTime() !== checkIn.getTime())
  ) {
    checkOut = lastSeen;
    record.checkOut = vietnamLocalIso(lastSeen);
  }
  if (checkIn && checkOut) {
    const actualStart =
      record.approvedLeave && checkIn.getTime() < expectedStart.getTime()
        ? expectedStart
        : checkIn;
    const actualEnd =
      record.approvedLeave && checkOut.getTime() > expectedEnd.getTime()
        ? expectedEnd
        : checkOut;
    record.workedMinutes = Math.max(
      0,
      Math.floor((actualEnd.getTime() - actualStart.getTime()) / 60000),
    );
  }
  record.earlyMinutes = checkOut
    ? Math.max(
        0,
        Math.floor((expectedEnd.getTime() - checkOut.getTime()) / 60000),
      )
    : 0;
  record.statusCode =
    !checkIn || !checkOut
      ? "MISSING_CHECK"
      : record.lateMinutes > 0 && record.earlyMinutes > 0
        ? "LATE_EARLY"
        : record.lateMinutes > 0
          ? "LATE"
          : record.earlyMinutes > 0
            ? "EARLY"
            : "ON_TIME";
}

function describeLeave(
  record: AttendanceRecord,
  leaves: LeaveRow[],
  leaveMask: number,
): void {
  if (leaveMask === 0) return;
  record.approvedLeave = true;
  record.leaveSession =
    leaveMask === 3 ? "Cả ngày" : leaveMask === 1 ? "Buổi sáng" : "Buổi chiều";
  record.leaveDescription = [
    ...new Set(
      leaves.map((leave) => `${leave.LeaveType} (${leave.RequestCode})`),
    ),
  ].join("; ");
}

function toAttendancePeriodData(
  row: AttendancePeriodRow | undefined,
  period: string,
): AttendancePeriodData {
  return {
    period: row?.Period ?? period,
    statusCode: row?.StatusCode ?? "OPEN",
    isConfirmed: asBoolean(row?.IsConfirmed),
    submittedAt: row?.SubmittedAt ?? null,
    lockedAt: row?.LockedAt ?? null,
    lockedByName: row?.LockedByName ?? null,
    unlockReason: row?.UnlockReason ?? null,
    employeeCount: row?.EmployeeCount ?? 0,
    confirmedCount: row?.ConfirmedCount ?? 0,
    pendingAdjustmentCount: row?.PendingAdjustmentCount ?? 0,
    pendingRemoteCount: row?.PendingRemoteCount ?? 0,
  };
}

export function resolveAttendanceRange(
  input: ListAttendanceDto,
): AttendanceRange {
  const defaultToDate = vietnamDatePart(new Date());
  const toDate = input.toDate ?? defaultToDate;
  const fromDate = input.fromDate ?? addDaysToDate(toDate, -30);
  const from = dateOnlyAsUtc(fromDate);
  const to = dateOnlyAsUtc(toDate);
  if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime())) {
    throw new BadRequestException("Khoảng ngày chấm công không hợp lệ.");
  }
  const rangeDays = Math.round((to.getTime() - from.getTime()) / 86400000);
  if (rangeDays < 0 || rangeDays > maxAttendanceRangeDays) {
    throw new BadRequestException("Khoảng lọc tối đa là 366 ngày.");
  }
  return { fromDate, toDate, from, to };
}

function validateAttendancePeriod(period: string): asserts period is string {
  const typed: AttendancePeriodDto = { period };
  if (!/^\d{4}-(0[1-9]|1[0-2])$/.test(typed.period)) {
    throw new BadRequestException("Kỳ chấm công không hợp lệ.");
  }
}

export function attendanceScopeFor(
  identity: AuthenticatedIdentity,
): AttendanceScope {
  return {
    canSeeAll:
      identity.roleCode === "ADMIN" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR",
    isManager: identity.roleCode === "MANAGER",
  };
}

function groupBy<T, TKey>(
  values: readonly T[],
  keyOf: (value: T) => TKey,
): Map<TKey, T[]> {
  const groups = new Map<TKey, T[]>();
  for (const value of values) {
    const key = keyOf(value);
    const group = groups.get(key);
    if (group) group.push(value);
    else groups.set(key, [value]);
  }
  return groups;
}

function activeLeavesForDate(
  leaves: readonly LeaveRow[],
  date: string,
): LeaveRow[] {
  return leaves.filter(
    (leave) => leave.StartDate <= date && leave.EndDate >= date,
  );
}

function scheduleForDate(
  schedules: readonly ScheduleRow[],
  date: string,
): ScheduleRow | undefined {
  const dayMask = 1 << dateOnlyAsUtc(date).getUTCDay();
  return schedules
    .filter(
      (schedule) =>
        schedule.StatusCode === "ACTIVE" &&
        schedule.EffectiveFrom <= date &&
        (!schedule.EffectiveTo || schedule.EffectiveTo >= date) &&
        (schedule.WorkDaysMask & dayMask) !== 0,
    )
    .sort((left, right) => {
      const effectiveSort = right.EffectiveFrom.localeCompare(
        left.EffectiveFrom,
      );
      return effectiveSort !== 0 ? effectiveSort : right.Id - left.Id;
    })[0];
}

function leaveSessionMask(sessionCode: string | null): number {
  switch (sessionCode?.trim().toLocaleUpperCase("vi-VN")) {
    case "CẢ NGÀY":
    case "FULL_DAY":
    case "FULL":
      return 3;
    case "BUỔI SÁNG":
    case "MORNING":
    case "AM":
      return 1;
    case "BUỔI CHIỀU":
    case "AFTERNOON":
    case "PM":
      return 2;
    default:
      return 0;
  }
}

function attendanceKey(userId: number, workDate: string): string {
  return `${userId}:${workDate}`;
}

function asBoolean(value: boolean | number | null | undefined): boolean {
  return value === true || value === 1;
}

function cleanIp(ipAddress: string | undefined): string | null {
  return ipAddress?.slice(0, 80) || null;
}

function dateOnlyAsUtc(value: string): Date {
  const date = new Date(`${value}T00:00:00.000Z`);
  const matches = /^\d{4}-(\d{2})-(\d{2})$/.exec(value);
  if (!matches) return new Date(Number.NaN);
  const month = Number(matches[1]);
  const day = Number(matches[2]);
  return date.getUTCMonth() + 1 === month && date.getUTCDate() === day
    ? date
    : new Date(Number.NaN);
}

function addDaysToDate(value: string, days: number): string {
  const date = dateOnlyAsUtc(value);
  date.setUTCDate(date.getUTCDate() + days);
  return date.toISOString().slice(0, 10);
}

function* datesInRange(fromDate: string, toDate: string): Generator<string> {
  let current = dateOnlyAsUtc(fromDate);
  const end = dateOnlyAsUtc(toDate);
  while (current.getTime() <= end.getTime()) {
    yield current.toISOString().slice(0, 10);
    current = new Date(current.getTime());
    current.setUTCDate(current.getUTCDate() + 1);
  }
}

function timeWithoutSeconds(value: string | null): string | null {
  return value ? value.slice(0, 5) : null;
}

function parseVietnamLocalDateTime(value: string | null): Date | null {
  if (!value || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/.test(value))
    return null;
  const date = new Date(`${value}+07:00`);
  return Number.isNaN(date.getTime()) ? null : date;
}

function vietnamDateTime(workDate: string, time: string): Date {
  const date = parseVietnamLocalDateTime(
    `${workDate}T${timeWithoutSeconds(time) ?? "00:00"}:00`,
  );
  if (!date) throw new Error("Invalid internal attendance date/time.");
  return date;
}

function addDaysToInstant(value: Date, days: number): Date {
  return new Date(value.getTime() + days * 86400000);
}

function vietnamDatePart(value: Date): string {
  return formatInVietnam(value, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

function vietnamLocalIso(value: Date): string {
  return formatInVietnam(value, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hourCycle: "h23",
  });
}

function formatInVietnam(
  value: Date,
  options: Intl.DateTimeFormatOptions,
): string {
  const formatter = new Intl.DateTimeFormat("en-CA", {
    timeZone: vietnamTimeZone,
    ...options,
  });
  const parts = Object.fromEntries(
    formatter
      .formatToParts(value)
      .filter((part) => part.type !== "literal")
      .map((part) => [part.type, part.value]),
  );
  const date = `${parts.year}-${parts.month}-${parts.day}`;
  return parts.hour
    ? `${date}T${parts.hour}:${parts.minute}:${parts.second}`
    : date;
}
