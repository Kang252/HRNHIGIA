import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SchemaReadinessService } from "../database/schema-readiness.service";
import { SqlServerService } from "../database/sql-server.service";
import { CreateRemotePlanDto } from "./create-remote-plan.dto";
import { RemoteAttendanceRangeDto } from "./remote-attendance-range.dto";
import { ReviewRemoteItemDto } from "./review-remote-item.dto";

const maxRangeDays = 100;
const vietnamTimeZone = "Asia/Bangkok";
const localDatePattern = /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/;
const timePattern = /^([01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?$/;

export interface RemoteAttendanceState {
  userId: number;
  faceEnrollmentStatus: string | null;
  plans: RemotePlanItem[];
  punches: RemotePunchItem[];
  trips: RemoteTripItem[];
  /**
   * Camera evidence and the OpenCV advisory comparison deliberately stay in
   * NHIGIA.Modern.  This slice handles plans and human review only.
   */
  biometricCaptureAvailable: false;
}

export interface RemotePlanItem {
  id: number;
  userId: number;
  displayName: string;
  mode: "FIELD" | "HOME";
  placeName: string;
  fromDate: string;
  toDate: string;
  workDaysMask: number;
  windowStart: string;
  windowEnd: string;
  isFlexible: boolean;
  requiredMinutes: number;
  breakMinutes: number;
  latitude: number | null;
  longitude: number | null;
  radiusMeters: number;
  leaveRequestId: number | null;
  note: string | null;
  statusCode: string;
  reviewNote: string | null;
  reviewedAt: string | null;
  createdAt: string;
  canReview: boolean;
  canCancel: boolean;
}

export interface RemotePunchItem {
  id: number;
  userId: number;
  planId: number;
  displayName: string;
  kind: "IN" | "OUT" | "VISIT";
  capturedAt: string;
  checkTime: string;
  receivedAt: string;
  wasOffline: boolean;
  latitude: number | null;
  longitude: number | null;
  accuracyMeters: number | null;
  distanceMeters: number | null;
  placeName: string;
  note: string | null;
  statusCode: string;
  reviewReason: string | null;
  reviewNote: string | null;
  reviewedAt: string | null;
  canReview: boolean;
}

export interface RemoteTripItem {
  id: number;
  requestCode: string;
  startDate: string;
  endDate: string;
}

interface RemotePlanRow {
  Id: number;
  UserId: number;
  DisplayName: string;
  DepartmentId: number | null;
  RoleCode: string;
  Mode: "FIELD" | "HOME";
  PlaceName: string;
  FromDate: string;
  ToDate: string;
  WorkDaysMask: number;
  WindowStart: string;
  WindowEnd: string;
  IsFlexible: boolean | number;
  RequiredMinutes: number;
  BreakMinutes: number;
  Latitude: number | null;
  Longitude: number | null;
  RadiusMeters: number;
  LeaveRequestId: number | null;
  Note: string | null;
  StatusCode: string;
  ReviewNote: string | null;
  ReviewedAt: string | null;
  CreatedAt: string;
}

interface RemotePunchRow {
  Id: number;
  UserId: number;
  PlanId: number;
  DisplayName: string;
  DepartmentId: number | null;
  RoleCode: string;
  Kind: "IN" | "OUT" | "VISIT";
  CapturedAt: string;
  CheckTime: string;
  ReceivedAt: string;
  WasOffline: boolean | number;
  Latitude: number | null;
  Longitude: number | null;
  AccuracyMeters: number | null;
  DistanceMeters: number | null;
  PlaceName: string;
  Note: string | null;
  StatusCode: string;
  ReviewReason: string | null;
  ReviewNote: string | null;
  ReviewedAt: string | null;
}

interface RemoteTripRow {
  Id: number;
  RequestCode: string;
  StartDate: string;
  EndDate: string;
}

interface FaceEnrollmentRow {
  StatusCode: string;
}

interface PlanForReviewRow {
  Id: number;
  UserId: number;
  DepartmentId: number | null;
  RoleCode: string;
  StatusCode: string;
  FromDate: string;
  ToDate: string;
  WorkDaysMask: number;
}

interface PunchForReviewRow {
  Id: number;
  UserId: number;
  DepartmentId: number | null;
  RoleCode: string;
  Kind: "IN" | "OUT" | "VISIT";
  CheckTime: Date;
  Period: string;
  StatusCode: string;
  FaceEnrollmentId: number | null;
}

interface RemoteScope {
  canSeeAll: boolean;
  isManager: boolean;
}

interface ValidatedRemotePlan {
  mode: "FIELD" | "HOME";
  placeName: string;
  fromDate: Date;
  toDate: Date;
  fromDateText: string;
  toDateText: string;
  workDaysMask: number;
  windowStart: string;
  windowEnd: string;
  isFlexible: boolean;
  requiredMinutes: number;
  breakMinutes: number;
  latitude: number | null;
  longitude: number | null;
  radiusMeters: number;
  leaveRequestId: number | null;
  note: string | null;
}

const remoteScopeSql = `(u.Id=@ActorId OR @CanSeeAll=1 OR
  (@IsManager=1 AND u.DepartmentId=@DepartmentId AND u.RoleCode='EMPLOYEE'))`;

@Injectable()
export class RemoteAttendanceService {
  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly schemaReadiness: SchemaReadinessService,
  ) {}

  public async getState(
    identity: AuthenticatedIdentity,
    input: RemoteAttendanceRangeDto,
  ): Promise<RemoteAttendanceState> {
    await this.schemaReadiness.assertReady("remoteAttendance");
    const range = resolveRange(input);
    const pool = await this.sqlServer.getPool();
    const [plans, punches, trips, enrollment] = await Promise.all([
      this.listPlans(pool, identity, range),
      this.listPunches(pool, identity, range),
      this.listTrips(pool, identity, range),
      pool.request().input("ActorId", sql.Int, identity.userId)
        .query<FaceEnrollmentRow>(`
          SELECT TOP (1) StatusCode
          FROM dbo.HrmFaceEnrollment
          WHERE UserId=@ActorId
          ORDER BY Id DESC;`),
    ]);

    return {
      userId: identity.userId,
      faceEnrollmentStatus: enrollment.recordset[0]?.StatusCode ?? null,
      plans: plans.map((row) => toPlanItem(row, identity)),
      punches: punches.map((row) => toPunchItem(row, identity)),
      trips: trips.map((row) => ({
        id: row.Id,
        requestCode: row.RequestCode,
        startDate: row.StartDate,
        endDate: row.EndDate,
      })),
      biometricCaptureAvailable: false,
    };
  }

  public async createPlan(
    identity: AuthenticatedIdentity,
    input: CreateRemotePlanDto,
    ipAddress: string | undefined,
  ): Promise<{ id: number; statusCode: "PENDING" }> {
    await this.schemaReadiness.assertReady("remoteAttendance");
    const plan = validatePlan(input);
    if (plan.fromDateText < addVietnamDays(vietnamToday(), -7)) {
      throw new BadRequestException("Không thể đăng ký lùi quá 7 ngày.");
    }

    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.SERIALIZABLE,
      async (tx) => {
        if (plan.leaveRequestId !== null) {
          const linked = await new sql.Request(tx)
            .input("Id", sql.Int, plan.leaveRequestId)
            .input("UserId", sql.Int, identity.userId).query<{
            StartDate: string;
            EndDate: string;
          }>(`
            SELECT TOP (1) CONVERT(char(10),StartDate,23) AS StartDate,
              CONVERT(char(10),EndDate,23) AS EndDate
            FROM dbo.HrmLeaveRequest WITH (UPDLOCK,HOLDLOCK)
            WHERE Id=@Id AND UserId=@UserId AND StatusCode='APPROVED'
              AND LeaveType=N'Công tác/Ra ngoài';`);
          const trip = linked.recordset[0];
          if (
            plan.mode !== "FIELD" ||
            !trip ||
            trip.StartDate > plan.fromDateText ||
            trip.EndDate < plan.toDateText
          ) {
            throw new BadRequestException(
              "Đơn liên kết phải là công tác đã duyệt của bạn, bao phủ thời gian đăng ký.",
            );
          }
        }

        const inserted = await new sql.Request(tx)
          .input("UserId", sql.Int, identity.userId)
          .input("Mode", sql.VarChar(10), plan.mode)
          .input("PlaceName", sql.NVarChar(200), plan.placeName)
          .input("FromDate", sql.Date, plan.fromDate)
          .input("ToDate", sql.Date, plan.toDate)
          .input("WorkDaysMask", sql.Int, plan.workDaysMask)
          .input("WindowStart", sql.VarChar(8), plan.windowStart)
          .input("WindowEnd", sql.VarChar(8), plan.windowEnd)
          .input("IsFlexible", sql.Bit, plan.isFlexible)
          .input("RequiredMinutes", sql.Int, plan.requiredMinutes)
          .input("BreakMinutes", sql.Int, plan.breakMinutes)
          .input("Latitude", sql.Float, plan.latitude)
          .input("Longitude", sql.Float, plan.longitude)
          .input("RadiusMeters", sql.Int, plan.radiusMeters)
          .input("LeaveRequestId", sql.Int, plan.leaveRequestId)
          .input("Note", sql.NVarChar(2000), plan.note).query<{ Id: number }>(`
          INSERT dbo.HrmRemoteWorkPlan(
            UserId,Mode,PlaceName,FromDate,ToDate,WorkDaysMask,
            WindowStart,WindowEnd,IsFlexible,RequiredMinutes,BreakMinutes,
            Latitude,Longitude,RadiusMeters,LeaveRequestId,Note
          )
          OUTPUT INSERTED.Id AS Id
          VALUES(
            @UserId,@Mode,@PlaceName,@FromDate,@ToDate,@WorkDaysMask,
            @WindowStart,@WindowEnd,@IsFlexible,@RequiredMinutes,@BreakMinutes,
            @Latitude,@Longitude,@RadiusMeters,@LeaveRequestId,@Note
          );`);
        const insertedId = inserted.recordset[0]?.Id;
        if (
          typeof insertedId !== "number" ||
          !Number.isSafeInteger(insertedId) ||
          insertedId < 1
        ) {
          throw new Error("Không thể lưu đăng ký làm việc ngoài công ty.");
        }
        const id = insertedId;

        await this.audit(
          tx,
          identity.userId,
          "CREATE",
          "RemoteWorkPlan",
          id,
          "Đăng ký làm việc ngoài công ty",
          ipAddress,
        );
        await this.notifyReviewers(
          tx,
          identity,
          "Đăng ký làm việc ngoài công ty",
          `${identity.displayName ?? identity.username} gửi đăng ký cần duyệt.`,
        );
        return { id, statusCode: "PENDING" };
      },
    );
  }

  public async reviewPlan(
    identity: AuthenticatedIdentity,
    id: number,
    input: ReviewRemoteItemDto,
    ipAddress: string | undefined,
  ): Promise<{ id: number; statusCode: "APPROVED" | "REJECTED" }> {
    await this.schemaReadiness.assertReady("remoteAttendance");
    const note = reviewNote(input.note);
    const statusCode = input.approve ? "APPROVED" : "REJECTED";
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.SERIALIZABLE,
      async (tx) => {
        const plan = await new sql.Request(tx).input("Id", sql.BigInt, id)
          .query<PlanForReviewRow>(`
          SELECT TOP (1) p.Id,p.UserId,u.DepartmentId,u.RoleCode,p.StatusCode,
            CONVERT(char(10),p.FromDate,23) AS FromDate,
            CONVERT(char(10),p.ToDate,23) AS ToDate,p.WorkDaysMask
          FROM dbo.HrmRemoteWorkPlan p WITH (UPDLOCK,HOLDLOCK)
          INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId AND u.IsActive=1
          WHERE p.Id=@Id;`);
        const row = plan.recordset[0];
        if (
          !row ||
          row.StatusCode !== "PENDING" ||
          !canReview(identity, row.UserId, row.DepartmentId, row.RoleCode)
        ) {
          throw new NotFoundException(
            "Không tìm thấy đăng ký cần duyệt hoặc bạn không có quyền xử lý.",
          );
        }

        if (input.approve) {
          const overlapping = await new sql.Request(tx)
            .input("UserId", sql.Int, row.UserId)
            .input("FromDate", sql.Date, dateFromText(row.FromDate))
            .input("ToDate", sql.Date, dateFromText(row.ToDate))
            .input("WorkDaysMask", sql.Int, row.WorkDaysMask).query<{
            Count: number;
          }>(`
            SELECT COUNT(1) AS Count
            FROM dbo.HrmRemoteWorkPlan WITH (UPDLOCK,HOLDLOCK)
            WHERE UserId=@UserId AND StatusCode='APPROVED'
              AND FromDate<=@ToDate AND ToDate>=@FromDate
              AND (WorkDaysMask & @WorkDaysMask)<>0;`);
          if ((overlapping.recordset[0]?.Count ?? 0) > 0) {
            throw new BadRequestException(
              "Nhân viên đã có lịch ngoài công ty được duyệt trùng ngày. Hủy lịch trùng trước khi duyệt.",
            );
          }
        }

        await new sql.Request(tx)
          .input("Id", sql.BigInt, id)
          .input("StatusCode", sql.VarChar(20), statusCode)
          .input("ReviewNote", sql.NVarChar(1000), note)
          .input("ActorId", sql.Int, identity.userId).query(`
          UPDATE dbo.HrmRemoteWorkPlan
          SET StatusCode=@StatusCode,ReviewNote=@ReviewNote,
            ReviewedBy=@ActorId,ReviewedAt=SYSDATETIME()
          WHERE Id=@Id;`);
        await this.audit(
          tx,
          identity.userId,
          input.approve ? "APPROVE" : "REJECT",
          "RemoteWorkPlan",
          id,
          note,
          ipAddress,
        );
        await this.notifyOwner(
          tx,
          row.UserId,
          input.approve
            ? "Lịch ngoài công ty đã được duyệt"
            : "Lịch ngoài công ty bị từ chối",
          note,
        );
        return { id, statusCode };
      },
    );
  }

  public async cancelPlan(
    identity: AuthenticatedIdentity,
    id: number,
    ipAddress: string | undefined,
  ): Promise<{ id: number; statusCode: "CANCELLED" }> {
    await this.schemaReadiness.assertReady("remoteAttendance");
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        const plan = await new sql.Request(tx).input("Id", sql.BigInt, id)
          .query<PlanForReviewRow>(`
          SELECT TOP (1) p.Id,p.UserId,u.DepartmentId,u.RoleCode,p.StatusCode,
            CONVERT(char(10),p.FromDate,23) AS FromDate,
            CONVERT(char(10),p.ToDate,23) AS ToDate,p.WorkDaysMask
          FROM dbo.HrmRemoteWorkPlan p WITH (UPDLOCK)
          INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
          WHERE p.Id=@Id;`);
        const row = plan.recordset[0];
        if (
          !row ||
          (row.StatusCode !== "PENDING" && row.StatusCode !== "APPROVED") ||
          (row.UserId !== identity.userId &&
            !canReview(identity, row.UserId, row.DepartmentId, row.RoleCode))
        ) {
          throw new NotFoundException("Không tìm thấy đăng ký có thể hủy.");
        }

        await new sql.Request(tx).input("Id", sql.BigInt, id).query(`
        UPDATE dbo.HrmRemoteWorkPlan SET StatusCode='CANCELLED' WHERE Id=@Id;`);
        await this.audit(
          tx,
          identity.userId,
          "CANCEL",
          "RemoteWorkPlan",
          id,
          "Hủy hiệu lực lịch; giữ nguyên lượt chấm đã xác minh",
          ipAddress,
        );
        return { id, statusCode: "CANCELLED" };
      },
    );
  }

  /**
   * The reviewer decision remains explicitly human.  This route never runs
   * face matching and never turns a biometric signal into automatic approval.
   */
  public async reviewPunch(
    identity: AuthenticatedIdentity,
    id: number,
    input: ReviewRemoteItemDto,
    ipAddress: string | undefined,
  ): Promise<{ id: number; statusCode: "APPROVED" | "REJECTED" }> {
    await this.schemaReadiness.assertReady("remoteAttendance");
    const note = reviewNote(input.note);
    const statusCode = input.approve ? "APPROVED" : "REJECTED";
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.SERIALIZABLE,
      async (tx) => {
        const punch = await new sql.Request(tx).input("Id", sql.BigInt, id)
          .query<PunchForReviewRow>(`
          SELECT TOP (1) p.Id,p.UserId,u.DepartmentId,u.RoleCode,p.Kind,
            p.CheckTime,CONVERT(char(7),p.CheckTime,126) AS Period,
            p.StatusCode,p.FaceEnrollmentId
          FROM dbo.HrmRemotePunch p WITH (UPDLOCK,HOLDLOCK)
          INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId AND u.IsActive=1
          WHERE p.Id=@Id;`);
        const row = punch.recordset[0];
        if (
          !row ||
          row.StatusCode !== "PENDING" ||
          !canReview(identity, row.UserId, row.DepartmentId, row.RoleCode)
        ) {
          throw new NotFoundException(
            "Không tìm thấy lượt chấm cần duyệt hoặc bạn không có quyền xử lý.",
          );
        }

        if (input.approve && row.FaceEnrollmentId !== null) {
          const enrolled = await new sql.Request(tx)
            .input("Id", sql.BigInt, row.FaceEnrollmentId)
            .input("UserId", sql.Int, row.UserId).query<{ Count: number }>(`
            SELECT COUNT(1) AS Count
            FROM dbo.HrmFaceEnrollment WITH (UPDLOCK,HOLDLOCK)
            WHERE Id=@Id AND UserId=@UserId AND StatusCode='ACTIVE';`);
          if ((enrolled.recordset[0]?.Count ?? 0) !== 1) {
            throw new BadRequestException(
              "Mẫu khuôn mặt đã bị thu hồi. Nhân viên cần đăng ký và chấm lại trước khi duyệt.",
            );
          }
        }

        await ensurePeriodOpen(tx, row.Period);
        await new sql.Request(tx)
          .input("Id", sql.BigInt, id)
          .input("StatusCode", sql.VarChar(20), statusCode)
          .input("ReviewNote", sql.NVarChar(1000), note)
          .input("ActorId", sql.Int, identity.userId)
          .input(
            "FaceStatus",
            sql.NVarChar(30),
            input.approve ? "MANUAL_APPROVED" : "MANUAL_REJECTED",
          ).query(`
          UPDATE dbo.HrmRemotePunch
          SET StatusCode=@StatusCode,ReviewNote=@ReviewNote,
            ReviewedBy=@ActorId,ReviewedAt=SYSDATETIME(),
            IncludedAt=CASE WHEN @StatusCode='APPROVED' THEN SYSDATETIME() END,
            FaceVerificationStatus=CASE WHEN FaceEnrollmentId IS NOT NULL
              THEN @FaceStatus ELSE FaceVerificationStatus END
          WHERE Id=@Id;`);
        if (input.approve && row.Kind !== "VISIT") {
          await new sql.Request(tx)
            .input("UserId", sql.Int, row.UserId)
            .input("Period", sql.Char(7), row.Period).query(`
            DELETE FROM dbo.HrmAttendancePeriodConfirmation
            WHERE UserId=@UserId AND Period=@Period;`);
        }
        await this.audit(
          tx,
          identity.userId,
          input.approve ? "APPROVE" : "REJECT",
          "RemotePunch",
          id,
          note,
          ipAddress,
        );
        await this.notifyOwner(
          tx,
          row.UserId,
          input.approve
            ? "Đã xác minh lượt chấm ngoài công ty"
            : "Lượt chấm ngoài công ty bị từ chối",
          note,
        );
        return { id, statusCode };
      },
    );
  }

  private async listPlans(
    pool: sql.ConnectionPool,
    identity: AuthenticatedIdentity,
    range: RemoteRange,
  ): Promise<RemotePlanRow[]> {
    const result = await bindScope(pool.request(), identity)
      .input("FromDate", sql.Date, range.from)
      .input("ToDate", sql.Date, range.to).query<RemotePlanRow>(`
        SELECT p.Id,p.UserId,u.DisplayName,u.DepartmentId,u.RoleCode,p.Mode,
          p.PlaceName,CONVERT(char(10),p.FromDate,23) AS FromDate,
          CONVERT(char(10),p.ToDate,23) AS ToDate,p.WorkDaysMask,
          CONVERT(char(8),p.WindowStart,108) AS WindowStart,
          CONVERT(char(8),p.WindowEnd,108) AS WindowEnd,p.IsFlexible,
          p.RequiredMinutes,p.BreakMinutes,p.Latitude,p.Longitude,p.RadiusMeters,
          p.LeaveRequestId,p.Note,p.StatusCode,p.ReviewNote,
          CONVERT(char(19),p.ReviewedAt,126) AS ReviewedAt,
          CONVERT(char(19),p.CreatedAt,126) AS CreatedAt
        FROM dbo.HrmRemoteWorkPlan p
        INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
        WHERE p.ToDate>=@FromDate AND p.FromDate<=@ToDate
          AND ${remoteScopeSql}
        ORDER BY p.Id DESC;`);
    return result.recordset;
  }

  private async listPunches(
    pool: sql.ConnectionPool,
    identity: AuthenticatedIdentity,
    range: RemoteRange,
  ): Promise<RemotePunchRow[]> {
    const result = await bindScope(pool.request(), identity)
      .input("FromDate", sql.Date, range.from)
      .input("ToDate", sql.Date, range.to).query<RemotePunchRow>(`
        SELECT TOP (500) p.Id,p.UserId,p.PlanId,u.DisplayName,u.DepartmentId,
          u.RoleCode,p.Kind,
          CONVERT(varchar(34),p.CapturedAt,127) AS CapturedAt,
          CONVERT(char(19),p.CheckTime,126) AS CheckTime,
          CONVERT(char(19),p.ReceivedAt,126) AS ReceivedAt,p.WasOffline,
          p.Latitude,p.Longitude,p.AccuracyMeters,p.DistanceMeters,p.PlaceName,
          p.Note,p.StatusCode,p.ReviewReason,p.ReviewNote,
          CONVERT(char(19),p.ReviewedAt,126) AS ReviewedAt
        FROM dbo.HrmRemotePunch p
        INNER JOIN dbo.HrmUserAccount u ON u.Id=p.UserId
        WHERE p.CheckTime>=@FromDate AND p.CheckTime<DATEADD(day,1,@ToDate)
          AND ${remoteScopeSql}
        ORDER BY p.CheckTime DESC,p.Id DESC;`);
    return result.recordset;
  }

  private async listTrips(
    pool: sql.ConnectionPool,
    identity: AuthenticatedIdentity,
    range: RemoteRange,
  ): Promise<RemoteTripRow[]> {
    const result = await pool
      .request()
      .input("ActorId", sql.Int, identity.userId)
      .input("FromDate", sql.Date, range.from)
      .input("ToDate", sql.Date, range.to).query<RemoteTripRow>(`
        SELECT TOP (200) Id,RequestCode,CONVERT(char(10),StartDate,23) AS StartDate,
          CONVERT(char(10),EndDate,23) AS EndDate
        FROM dbo.HrmLeaveRequest
        WHERE UserId=@ActorId AND StatusCode='APPROVED'
          AND LeaveType=N'Công tác/Ra ngoài'
          AND StartDate<=@ToDate AND EndDate>=@FromDate
        ORDER BY EndDate DESC,Id DESC;`);
    return result.recordset;
  }

  private async audit(
    tx: sql.Transaction,
    userId: number,
    actionCode: string,
    entityType: "RemoteWorkPlan" | "RemotePunch",
    entityId: number,
    detail: string,
    ipAddress: string | undefined,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("UserId", sql.Int, userId)
      .input("ActionCode", sql.NVarChar(80), actionCode)
      .input("EntityType", sql.NVarChar(80), entityType)
      .input("EntityId", sql.NVarChar(80), String(entityId))
      .input("Detail", sql.NVarChar(sql.MAX), detail)
      .input("IpAddress", sql.NVarChar(80), cleanIp(ipAddress)).query(`
        INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
        VALUES(@UserId,@ActionCode,@EntityType,@EntityId,@Detail,@IpAddress);`);
  }

  private async notifyOwner(
    tx: sql.Transaction,
    userId: number,
    title: string,
    message: string,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("UserId", sql.Int, userId)
      .input("Title", sql.NVarChar(200), title)
      .input("Message", sql.NVarChar(1000), message).query(`
        INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
        VALUES(@UserId,@Title,@Message,'/RemoteAttendance');`);
  }

  private async notifyReviewers(
    tx: sql.Transaction,
    identity: AuthenticatedIdentity,
    title: string,
    message: string,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("OwnerId", sql.Int, identity.userId)
      .input("Title", sql.NVarChar(200), title)
      .input("Message", sql.NVarChar(1000), message).query(`
        INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
        SELECT r.Id,@Title,@Message,'/RemoteAttendance'
        FROM dbo.HrmUserAccount r
        INNER JOIN dbo.HrmUserAccount owner ON owner.Id=@OwnerId
        WHERE r.IsActive=1 AND r.Id<>owner.Id
          AND (r.RoleCode IN ('ADMIN','HR','DIRECTOR')
            OR (r.RoleCode='MANAGER' AND owner.RoleCode='EMPLOYEE'
              AND r.DepartmentId=owner.DepartmentId));`);
  }

  private async transaction<T>(
    pool: sql.ConnectionPool,
    isolationLevel: number,
    action: (transaction: sql.Transaction) => Promise<T>,
  ): Promise<T> {
    const transaction = new sql.Transaction(pool);
    let committed = false;
    await transaction.begin(isolationLevel);
    try {
      const result = await action(transaction);
      await transaction.commit();
      committed = true;
      return result;
    } catch (error: unknown) {
      if (!committed) await transaction.rollback().catch(() => undefined);
      throw error;
    }
  }
}

interface RemoteRange {
  from: Date;
  to: Date;
}

export function remoteScopeFor(identity: AuthenticatedIdentity): RemoteScope {
  return {
    canSeeAll:
      identity.roleCode === "ADMIN" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR",
    isManager: identity.roleCode === "MANAGER",
  };
}

export function canReview(
  actor: AuthenticatedIdentity,
  ownerId: number,
  departmentId: number | null,
  ownerRole: string,
): boolean {
  const scope = remoteScopeFor(actor);
  return (
    actor.userId !== ownerId &&
    (scope.canSeeAll ||
      (scope.isManager &&
        ownerRole === "EMPLOYEE" &&
        typeof actor.departmentId === "number" &&
        actor.departmentId === departmentId))
  );
}

export function resolveRange(input: RemoteAttendanceRangeDto): RemoteRange {
  const from = dateFromText(input.fromDate);
  const to = dateFromText(input.toDate);
  if (
    to < from ||
    (to.getTime() - from.getTime()) / 86_400_000 > maxRangeDays
  ) {
    throw new BadRequestException("Khoảng xem tối đa 100 ngày.");
  }
  return { from, to };
}

function bindScope(
  request: sql.Request,
  identity: AuthenticatedIdentity,
): sql.Request {
  const scope = remoteScopeFor(identity);
  return request
    .input("ActorId", sql.Int, identity.userId)
    .input("CanSeeAll", sql.Bit, scope.canSeeAll)
    .input("IsManager", sql.Bit, scope.isManager)
    .input("DepartmentId", sql.Int, identity.departmentId ?? null);
}

export function validatePlan(input: CreateRemotePlanDto): ValidatedRemotePlan {
  if (input.mode !== "FIELD" && input.mode !== "HOME") {
    throw new BadRequestException("Loại lịch không hợp lệ.");
  }
  const fromDate = dateFromText(input.fromDate);
  const toDate = dateFromText(input.toDate);
  if (
    toDate < fromDate ||
    (toDate.getTime() - fromDate.getTime()) / 86_400_000 > 92
  ) {
    throw new BadRequestException("Chọn khoảng đăng ký tối đa 93 ngày.");
  }
  const windowStart = normaliseTime(input.windowStart ?? "08:00");
  const windowEnd = normaliseTime(input.windowEnd ?? "17:30");
  const windowMinutes =
    timeSeconds(windowEnd) / 60 - timeSeconds(windowStart) / 60;
  if (windowMinutes <= 0) {
    throw new BadRequestException(
      "Khung giờ phải bắt đầu và kết thúc trong cùng ngày.",
    );
  }
  const breakMinutes = input.breakMinutes ?? 60;
  const requiredMinutes = input.requiredMinutes ?? 480;
  if (
    !Number.isInteger(input.workDaysMask) ||
    input.workDaysMask < 1 ||
    input.workDaysMask > 127 ||
    !Number.isInteger(breakMinutes) ||
    !Number.isInteger(requiredMinutes) ||
    breakMinutes < 0 ||
    breakMinutes >= windowMinutes ||
    requiredMinutes < 1 ||
    requiredMinutes > windowMinutes - breakMinutes
  ) {
    throw new BadRequestException(
      "Số phút làm việc/nghỉ không phù hợp khung giờ.",
    );
  }
  const latitude = input.latitude ?? null;
  const longitude = input.longitude ?? null;
  if (
    (latitude === null) !== (longitude === null) ||
    (latitude !== null &&
      (!Number.isFinite(latitude) ||
        !Number.isFinite(longitude as number) ||
        Math.abs(latitude) > 90 ||
        Math.abs(longitude as number) > 180))
  ) {
    throw new BadRequestException("Tọa độ không hợp lệ.");
  }
  if (input.mode === "HOME" && latitude === null) {
    throw new BadRequestException("Lịch làm tại nhà cần tọa độ đăng ký.");
  }
  const placeName = input.placeName.trim();
  const note = input.note?.trim() || null;
  const radiusMeters = input.radiusMeters ?? 200;
  if (
    !placeName ||
    placeName.length > 200 ||
    (note !== null && note.length > 2000) ||
    !Number.isInteger(radiusMeters) ||
    radiusMeters < 50 ||
    radiusMeters > 2000 ||
    (input.leaveRequestId !== undefined &&
      (!Number.isSafeInteger(input.leaveRequestId) || input.leaveRequestId < 1))
  ) {
    throw new BadRequestException(
      "Nhập địa điểm/tuyến tối đa 200 ký tự và ghi chú tối đa 2.000 ký tự.",
    );
  }
  if (input.isFlexible !== undefined && typeof input.isFlexible !== "boolean") {
    throw new BadRequestException("Ca linh hoạt không hợp lệ.");
  }
  return {
    mode: input.mode,
    placeName,
    fromDate,
    toDate,
    fromDateText: input.fromDate,
    toDateText: input.toDate,
    workDaysMask: input.workDaysMask,
    windowStart,
    windowEnd,
    isFlexible: input.isFlexible ?? false,
    requiredMinutes,
    breakMinutes,
    latitude,
    longitude,
    radiusMeters,
    leaveRequestId: input.leaveRequestId ?? null,
    note,
  };
}

function toPlanItem(
  row: RemotePlanRow,
  identity: AuthenticatedIdentity,
): RemotePlanItem {
  const canReviewPlan =
    row.StatusCode === "PENDING" &&
    canReview(identity, row.UserId, row.DepartmentId, row.RoleCode);
  const canCancel =
    (row.StatusCode === "PENDING" || row.StatusCode === "APPROVED") &&
    (row.UserId === identity.userId ||
      canReview(identity, row.UserId, row.DepartmentId, row.RoleCode));
  return {
    id: row.Id,
    userId: row.UserId,
    displayName: row.DisplayName,
    mode: row.Mode,
    placeName: row.PlaceName,
    fromDate: row.FromDate,
    toDate: row.ToDate,
    workDaysMask: row.WorkDaysMask,
    windowStart: trimSeconds(row.WindowStart),
    windowEnd: trimSeconds(row.WindowEnd),
    isFlexible: bool(row.IsFlexible),
    requiredMinutes: row.RequiredMinutes,
    breakMinutes: row.BreakMinutes,
    latitude: row.Latitude,
    longitude: row.Longitude,
    radiusMeters: row.RadiusMeters,
    leaveRequestId: row.LeaveRequestId,
    note: row.Note,
    statusCode: row.StatusCode,
    reviewNote: row.ReviewNote,
    reviewedAt: row.ReviewedAt,
    createdAt: row.CreatedAt,
    canReview: canReviewPlan,
    canCancel,
  };
}

function toPunchItem(
  row: RemotePunchRow,
  identity: AuthenticatedIdentity,
): RemotePunchItem {
  return {
    id: row.Id,
    userId: row.UserId,
    planId: row.PlanId,
    displayName: row.DisplayName,
    kind: row.Kind,
    capturedAt: row.CapturedAt,
    checkTime: row.CheckTime,
    receivedAt: row.ReceivedAt,
    wasOffline: bool(row.WasOffline),
    latitude: row.Latitude,
    longitude: row.Longitude,
    accuracyMeters: row.AccuracyMeters,
    distanceMeters: row.DistanceMeters,
    placeName: row.PlaceName,
    note: row.Note,
    statusCode: row.StatusCode,
    reviewReason: row.ReviewReason,
    reviewNote: row.ReviewNote,
    reviewedAt: row.ReviewedAt,
    canReview:
      row.StatusCode === "PENDING" &&
      canReview(identity, row.UserId, row.DepartmentId, row.RoleCode),
  };
}

function reviewNote(value: string): string {
  const note = value.trim();
  if (!note || note.length > 1000) {
    throw new BadRequestException("Nhập ghi chú xử lý từ 1 đến 1.000 ký tự.");
  }
  return note;
}

function normaliseTime(value: string): string {
  if (!timePattern.test(value)) {
    throw new BadRequestException("Khung giờ không hợp lệ.");
  }
  return value.length === 5 ? `${value}:00` : value;
}

function timeSeconds(value: string): number {
  const [hours = 0, minutes = 0, seconds = 0] = value.split(":").map(Number);
  return hours * 3600 + minutes * 60 + seconds;
}

function trimSeconds(value: string): string {
  return value.length >= 5 ? value.slice(0, 5) : value;
}

function dateFromText(value: string): Date {
  if (!localDatePattern.test(value)) {
    throw new BadRequestException("Ngày không hợp lệ.");
  }
  const parsed = new Date(`${value}T00:00:00.000Z`);
  if (Number.isNaN(parsed.getTime()) || dateText(parsed) !== value) {
    throw new BadRequestException("Ngày không hợp lệ.");
  }
  return parsed;
}

function dateText(value: Date): string {
  return value.toISOString().slice(0, 10);
}

function vietnamToday(now = new Date()): string {
  const values = new Intl.DateTimeFormat("en-CA", {
    timeZone: vietnamTimeZone,
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  })
    .formatToParts(now)
    .reduce<Record<string, string>>((result, part) => {
      if (part.type !== "literal") result[part.type] = part.value;
      return result;
    }, {});
  return `${values.year}-${values.month}-${values.day}`;
}

function addVietnamDays(date: string, days: number): string {
  const value = dateFromText(date);
  value.setUTCDate(value.getUTCDate() + days);
  return dateText(value);
}

async function ensurePeriodOpen(
  tx: sql.Transaction,
  period: string,
): Promise<void> {
  const result = await new sql.Request(tx).input("Period", sql.Char(7), period)
    .query<{ StatusCode: string }>(`
      SELECT StatusCode
      FROM dbo.HrmAttendancePeriod WITH (UPDLOCK,HOLDLOCK)
      WHERE Period=@Period;`);
  if (result.recordset[0]?.StatusCode === "LOCKED") {
    throw new BadRequestException(
      "Kỳ công đã khóa. HR cần mở khóa trước khi tiếp nhận/xác minh lượt chấm.",
    );
  }
}

function cleanIp(value: string | undefined): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed.slice(0, 80) : null;
}

function bool(value: boolean | number): boolean {
  return value === true || value === 1;
}
