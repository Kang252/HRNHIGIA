import { Injectable } from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SchemaReadinessService } from "../database/schema-readiness.service";
import { SqlServerService } from "../database/sql-server.service";
import {
  type ListProceduresDto,
  type ProcedureKind,
  type ProcedureSource,
  type ProcedureWorkKind,
  procedureWorkKinds,
} from "./list-procedures.dto";

export interface ProcedureListItem {
  id: number;
  source: ProcedureSource;
  code: string;
  kind: ProcedureKind;
  requestType: string;
  title: string;
  summary: string;
  ownerUserId: number;
  ownerName: string;
  departmentName: string | null;
  startAt: Date | null;
  endAt: Date | null;
  dueDate: Date | null;
  status: string;
  hasAttachment: boolean;
  createdAt: Date;
  updatedAt: Date | null;
  isMine: boolean;
}

export interface LeaveProcedureDetail extends ProcedureListItem {
  source: "leave";
  kind: "leave";
  sessionCode: string;
  handoverTo: string | null;
  reason: string;
  attachmentName: string | null;
  managerNote: string | null;
  hrNote: string | null;
}

export interface WorkProcedureDetail extends ProcedureListItem {
  source: "work";
  kind: ProcedureWorkKind;
  category: string | null;
  description: string | null;
  location: string | null;
  destination: string | null;
  priority: string;
  lastActionNote: string | null;
}

export type ProcedureDetail = LeaveProcedureDetail | WorkProcedureDetail;

interface LeaveListRow {
  Id: number;
  RequestCode: string;
  UserId: number;
  OwnerName: string;
  DepartmentName: string | null;
  LeaveType: string;
  StartDate: Date;
  EndDate: Date;
  StatusCode: string;
  Summary: string;
  HasAttachment: boolean | number;
  CreatedAt: Date;
  UpdatedAt: Date | null;
}

interface LeaveDetailRow extends LeaveListRow {
  SessionCode: string;
  HandoverTo: string | null;
  Reason: string;
  AttachmentName: string | null;
  ManagerNote: string | null;
  HrNote: string | null;
}

interface WorkListRow {
  Id: number;
  Kind: ProcedureWorkKind;
  Title: string;
  OwnerUserId: number;
  OwnerName: string;
  DepartmentName: string | null;
  DueDate: Date | null;
  StartAt: Date | null;
  EndAt: Date | null;
  Status: string | null;
  Summary: string;
  CreatedAt: Date;
  UpdatedAt: Date | null;
}

interface WorkDetailRow extends WorkListRow {
  Category: string | null;
  Description: string | null;
  Location: string | null;
  Destination: string | null;
  Priority: string | null;
  LastActionNote: string | null;
}

interface ProcedureScope {
  canSeeAll: boolean;
  isManager: boolean;
}

@Injectable()
export class ProceduresService {
  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly schemaReadiness: SchemaReadinessService,
  ) {}

  /**
   * Lists only records that the signed-in employee could open through the
   * current C# procedures and work pages.  Work kinds are allow-listed to
   * keep financial, asset, KPI, recruitment and training data in their own
   * migration slices.
   */
  public async list(
    identity: AuthenticatedIdentity,
    filter: ListProceduresDto,
  ): Promise<ProcedureListItem[]> {
    await this.schemaReadiness.assertReady("procedures");
    const sources = sourcesForFilter(filter);
    if (sources.length === 0) return [];

    const pool = await this.sqlServer.getPool();
    const take = filter.take ?? 20;
    const lists = await Promise.all(
      sources.map(async (source) =>
        source === "leave"
          ? this.listLeaves(pool, identity, filter, take)
          : this.listWorkItems(pool, identity, filter, take),
      ),
    );

    return lists.flat().sort(compareByCreatedAtDescending).slice(0, take);
  }

  public async findVisibleById(
    identity: AuthenticatedIdentity,
    source: ProcedureSource,
    id: number,
  ): Promise<ProcedureDetail | undefined> {
    await this.schemaReadiness.assertReady("procedures");
    const pool = await this.sqlServer.getPool();
    if (source === "leave") {
      const result = await bindScope(pool.request(), identity).input(
        "Id",
        sql.Int,
        id,
      ).query<LeaveDetailRow>(`${leaveDetailSelectSql}
          ${leaveFromSql}
          WHERE r.Id=@Id AND ${leaveScopeSql};`);
      const row = result.recordset[0];
      return row ? toLeaveProcedureDetail(row, identity.userId) : undefined;
    }

    const result = await bindScope(pool.request(), identity).input(
      "Id",
      sql.Int,
      id,
    ).query<WorkDetailRow>(`${workDetailSelectSql}
        ${workFromSql}
        WHERE w.Id=@Id AND w.Kind IN (${workKindsSql}) AND ${workScopeSql};`);
    const row = result.recordset[0];
    return row ? toWorkProcedureDetail(row, identity.userId) : undefined;
  }

  private async listLeaves(
    pool: sql.ConnectionPool,
    identity: AuthenticatedIdentity,
    filter: ListProceduresDto,
    take: number,
  ): Promise<ProcedureListItem[]> {
    const result = await bindScope(pool.request(), identity)
      .input("Take", sql.Int, take)
      .input("Status", sql.NVarChar(50), filter.status?.trim() ?? "")
      .input("Keyword", sql.NVarChar(100), filter.keyword?.trim() ?? "")
      .query<LeaveListRow>(`${leaveListSelectSql}
        ${leaveFromSql}
        WHERE ${leaveScopeSql}
          AND (@Status='' OR r.StatusCode=@Status)
          AND (@Keyword='' OR r.RequestCode LIKE '%' + @Keyword + '%'
               OR r.LeaveType LIKE '%' + @Keyword + '%'
               OR r.Reason LIKE '%' + @Keyword + '%'
               OR u.DisplayName LIKE '%' + @Keyword + '%')
        ORDER BY r.CreatedAt DESC,r.Id DESC;`);
    return result.recordset.map((row) =>
      toLeaveProcedureListItem(row, identity.userId),
    );
  }

  private async listWorkItems(
    pool: sql.ConnectionPool,
    identity: AuthenticatedIdentity,
    filter: ListProceduresDto,
    take: number,
  ): Promise<ProcedureListItem[]> {
    const kind = filter.kind === "leave" ? "" : (filter.kind ?? "");
    const result = await bindScope(pool.request(), identity)
      .input("Take", sql.Int, take)
      .input("Kind", sql.NVarChar(50), kind)
      .input("Status", sql.NVarChar(50), filter.status?.trim() ?? "")
      .input("Keyword", sql.NVarChar(100), filter.keyword?.trim() ?? "")
      .query<WorkListRow>(`${workListSelectSql}
        ${workFromSql}
        WHERE w.Kind IN (${workKindsSql}) AND ${workScopeSql}
          AND (@Kind='' OR w.Kind=@Kind)
          AND (@Status='' OR COALESCE(w.Status,'OPEN')=@Status)
          AND (@Keyword='' OR w.Title LIKE '%' + @Keyword + '%'
               OR w.Reference LIKE '%' + @Keyword + '%'
               OR w.Category LIKE '%' + @Keyword + '%'
               OR w.Description LIKE '%' + @Keyword + '%'
               OR employee.DisplayName LIKE '%' + @Keyword + '%'
               OR creator.DisplayName LIKE '%' + @Keyword + '%')
        ORDER BY w.CreatedAt DESC,w.Id DESC;`);
    return result.recordset.map((row) =>
      toWorkProcedureListItem(row, identity.userId),
    );
  }
}

const leaveListSelectSql = `SELECT TOP (@Take)
  r.Id,r.RequestCode,r.UserId,u.DisplayName AS OwnerName,d.Name AS DepartmentName,
  r.LeaveType,r.StartDate,r.EndDate,r.StatusCode,
  LEFT(COALESCE(r.Reason,N''),280) AS Summary,
  CAST(CASE WHEN r.AttachmentContent IS NULL THEN 0 ELSE 1 END AS bit) AS HasAttachment,
  r.CreatedAt,r.UpdatedAt`;

const leaveDetailSelectSql = `SELECT TOP (1)
  r.Id,r.RequestCode,r.UserId,u.DisplayName AS OwnerName,d.Name AS DepartmentName,
  r.LeaveType,r.StartDate,r.EndDate,r.StatusCode,
  LEFT(COALESCE(r.Reason,N''),280) AS Summary,
  CAST(CASE WHEN r.AttachmentContent IS NULL THEN 0 ELSE 1 END AS bit) AS HasAttachment,
  r.CreatedAt,r.UpdatedAt,r.SessionCode,r.HandoverTo,r.Reason,r.AttachmentName,
  r.ManagerNote,r.HrNote`;

const leaveFromSql = `FROM dbo.HrmLeaveRequest r
  INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
  LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId`;

const leaveScopeSql = `(
  @CanSeeAll=1 OR r.UserId=@ActorId
  OR (@IsManager=1 AND u.DepartmentId=@DepartmentId)
)`;

const workListSelectSql = `SELECT TOP (@Take)
  w.Id,w.Kind,w.Title,
  COALESCE(w.EmployeeId,w.CreatedBy) AS OwnerUserId,
  COALESCE(employee.DisplayName,creator.DisplayName) AS OwnerName,
  d.Name AS DepartmentName,w.DueDate,w.StartAt,w.EndAt,
  COALESCE(w.Status,'OPEN') AS Status,
  LEFT(COALESCE(w.Description,N''),280) AS Summary,w.CreatedAt,w.UpdatedAt`;

const workDetailSelectSql = `SELECT TOP (1)
  w.Id,w.Kind,w.Title,
  COALESCE(w.EmployeeId,w.CreatedBy) AS OwnerUserId,
  COALESCE(employee.DisplayName,creator.DisplayName) AS OwnerName,
  d.Name AS DepartmentName,w.DueDate,w.StartAt,w.EndAt,
  COALESCE(w.Status,'OPEN') AS Status,
  LEFT(COALESCE(w.Description,N''),280) AS Summary,w.CreatedAt,w.UpdatedAt,
  w.Category,w.Description,w.Location,w.Destination,w.Priority,w.LastActionNote`;

const workFromSql = `FROM dbo.HrmWorkItem w
  LEFT JOIN dbo.HrmUserAccount employee ON employee.Id=w.EmployeeId
  INNER JOIN dbo.HrmUserAccount creator ON creator.Id=w.CreatedBy
  LEFT JOIN dbo.HrmDepartment d ON d.Id=COALESCE(w.DepartmentId,employee.DepartmentId)`;

const workScopeSql = `(
  @CanSeeAll=1 OR w.EmployeeId=@ActorId OR w.CreatedBy=@ActorId
  OR EXISTS(SELECT 1 FROM dbo.HrmWorkItemParticipant participant
            WHERE participant.WorkItemId=w.Id AND participant.UserId=@ActorId)
  OR (@IsManager=1 AND w.Kind<>'transfer'
      AND (employee.DepartmentId=@DepartmentId OR w.DepartmentId=@DepartmentId))
)`;

const workKindsSql = procedureWorkKinds.map((kind) => `N'${kind}'`).join(",");

export function procedureScope(
  identity: AuthenticatedIdentity,
): ProcedureScope {
  return {
    canSeeAll:
      identity.roleCode === "ADMIN" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR",
    // C# WorkItemStore lets only the Manager role acquire department scope.
    // Company-wide roles already pass through canSeeAll above.
    isManager: identity.roleCode === "MANAGER",
  };
}

function bindScope(
  request: sql.Request,
  identity: AuthenticatedIdentity,
): sql.Request {
  const scope = procedureScope(identity);
  return request
    .input("CanSeeAll", sql.Bit, scope.canSeeAll)
    .input("IsManager", sql.Bit, scope.isManager)
    .input("ActorId", sql.Int, identity.userId)
    .input("DepartmentId", sql.Int, identity.departmentId ?? null);
}

function sourcesForFilter(filter: ListProceduresDto): ProcedureSource[] {
  if (filter.source === "leave")
    return filter.kind && filter.kind !== "leave" ? [] : ["leave"];
  if (filter.source === "work") return filter.kind === "leave" ? [] : ["work"];
  if (filter.kind === "leave") return ["leave"];
  if (filter.kind) return ["work"];
  return ["leave", "work"];
}

export function toLeaveProcedureListItem(
  row: LeaveListRow,
  actorId: number,
): ProcedureListItem {
  return {
    id: row.Id,
    source: "leave",
    code: row.RequestCode,
    kind: "leave",
    requestType: row.LeaveType,
    title: row.LeaveType,
    summary: row.Summary,
    ownerUserId: row.UserId,
    ownerName: row.OwnerName,
    departmentName: row.DepartmentName,
    startAt: row.StartDate,
    endAt: row.EndDate,
    dueDate: null,
    status: row.StatusCode,
    hasAttachment: isSqlBitSet(row.HasAttachment),
    createdAt: row.CreatedAt,
    updatedAt: row.UpdatedAt,
    isMine: row.UserId === actorId,
  };
}

export function toLeaveProcedureDetail(
  row: LeaveDetailRow,
  actorId: number,
): LeaveProcedureDetail {
  return {
    ...toLeaveProcedureListItem(row, actorId),
    source: "leave",
    kind: "leave",
    sessionCode: row.SessionCode,
    handoverTo: row.HandoverTo,
    reason: row.Reason,
    attachmentName: row.AttachmentName,
    managerNote: row.ManagerNote,
    hrNote: row.HrNote,
  };
}

export function toWorkProcedureListItem(
  row: WorkListRow,
  actorId: number,
): ProcedureListItem {
  return {
    id: row.Id,
    source: "work",
    code: workRecordCode(row.Kind, row.Id),
    kind: row.Kind,
    requestType: workKindLabel(row.Kind),
    title: row.Title,
    summary: row.Summary,
    ownerUserId: row.OwnerUserId,
    ownerName: row.OwnerName,
    departmentName: row.DepartmentName,
    startAt: row.StartAt ?? row.DueDate,
    endAt: row.EndAt ?? row.DueDate,
    dueDate: row.DueDate,
    status: row.Status ?? "OPEN",
    hasAttachment: false,
    createdAt: row.CreatedAt,
    updatedAt: row.UpdatedAt,
    isMine: row.OwnerUserId === actorId,
  };
}

export function toWorkProcedureDetail(
  row: WorkDetailRow,
  actorId: number,
): WorkProcedureDetail {
  return {
    ...toWorkProcedureListItem(row, actorId),
    source: "work",
    kind: row.Kind,
    category: row.Category,
    description: row.Description,
    location: row.Location,
    destination: row.Destination,
    priority: row.Priority ?? "NORMAL",
    lastActionNote: row.LastActionNote,
  };
}

function workRecordCode(kind: ProcedureWorkKind, id: number): string {
  const prefix =
    {
      overtime: "TC",
      resignation: "NV",
      helpdesk: "HD",
      vehicle: "XE",
      meeting: "PH",
      "business-trip": "CT",
      transfer: "DC",
    }[kind] ?? "HS";
  return `${prefix}${id.toString().padStart(8, "0")}`;
}

function workKindLabel(kind: ProcedureWorkKind): string {
  return (
    {
      overtime: "Tăng ca",
      resignation: "Nghỉ việc và thôi việc",
      helpdesk: "Yêu cầu hỗ trợ",
      vehicle: "Đặt xe",
      meeting: "Đặt phòng họp",
      "business-trip": "Công tác",
      transfer: "Điều chuyển nhân sự",
    }[kind] ?? kind
  );
}

function isSqlBitSet(value: boolean | number): boolean {
  return value === true || value === 1;
}

function compareByCreatedAtDescending(
  left: ProcedureListItem,
  right: ProcedureListItem,
): number {
  const timeDifference = right.createdAt.getTime() - left.createdAt.getTime();
  if (timeDifference !== 0) return timeDifference;
  if (right.id !== left.id) return right.id - left.id;
  return right.source.localeCompare(left.source);
}
