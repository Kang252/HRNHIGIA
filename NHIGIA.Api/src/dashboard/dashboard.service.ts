import { Injectable } from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SqlServerService } from "../database/sql-server.service";

export interface DashboardData {
  scope: "SELF" | "DEPARTMENT" | "COMPANY";
  metrics: {
    totalEmployees: number;
    pendingApprovals: number;
    unmappedHanetUsers: number;
  };
}

@Injectable()
export class DashboardService {
  public constructor(private readonly sqlServer: SqlServerService) {}

  public async getDashboard(
    identity: AuthenticatedIdentity,
  ): Promise<DashboardData> {
    const pool = await this.sqlServer.getPool();
    const scope = sqlScope(identity);
    const request = pool
      .request()
      .input("CanSeeAll", sql.Bit, scope.canSeeAll)
      .input("IsManager", sql.Bit, scope.isManager)
      .input("ActorId", sql.Int, identity.userId)
      .input("DepartmentId", sql.Int, identity.departmentId ?? null)
      .input("RoleCode", sql.VarChar(20), identity.roleCode);

    const result = await request.query<{
      TotalEmployees: number;
      PendingApprovals: number;
      UnmappedHanetUsers: number;
    }>(`
      SELECT
        (SELECT COUNT(1) FROM dbo.HrmUserAccount u
          WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
            AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))) AS TotalEmployees,
        (SELECT COUNT(1) FROM dbo.HrmLeaveRequest r
          INNER JOIN dbo.HrmUserAccount u ON u.Id=r.UserId
          WHERE ((@RoleCode='MANAGER' AND r.StatusCode='PENDING_MANAGER' AND u.DepartmentId=@DepartmentId)
              OR (@RoleCode IN ('HR','ADMIN','DIRECTOR') AND r.StatusCode IN ('PENDING_MANAGER','PENDING_HR'))
              OR (@RoleCode='EMPLOYEE' AND r.UserId=@ActorId AND r.StatusCode IN ('PENDING_MANAGER','PENDING_HR')))) AS PendingApprovals,
        (SELECT COUNT(1) FROM dbo.HrmUserAccount u
          LEFT JOIN dbo.HrmHanetPersonMap m ON m.UserId=u.Id AND m.IsActive=1
          WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN' AND m.Id IS NULL
            AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))) AS UnmappedHanetUsers;`);
    const row = result.recordset[0] ?? {
      TotalEmployees: 0,
      PendingApprovals: 0,
      UnmappedHanetUsers: 0,
    };
    return {
      scope: scope.canSeeAll
        ? "COMPANY"
        : scope.isManager
          ? "DEPARTMENT"
          : "SELF",
      metrics: {
        totalEmployees: row.TotalEmployees,
        pendingApprovals: row.PendingApprovals,
        unmappedHanetUsers: row.UnmappedHanetUsers,
      },
    };
  }
}

export interface SqlRowScope {
  canSeeAll: boolean;
  isManager: boolean;
}

export function sqlScope(identity: AuthenticatedIdentity): SqlRowScope {
  return {
    canSeeAll:
      identity.roleCode === "ADMIN" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR",
    isManager:
      identity.roleCode === "MANAGER" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR" ||
      identity.roleCode === "ADMIN",
  };
}
