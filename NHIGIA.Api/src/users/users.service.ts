import { Injectable } from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SqlServerService } from "../database/sql-server.service";
import type { ListUsersDto } from "./list-users.dto";

/**
 * Minimal, work-facing identity safe to show in the employee directory.
 * Deliberately excludes credentials and HR profile fields such as identity,
 * contact, salary, insurance and bank data.
 */
export interface EmployeeDirectoryItem {
  id: number;
  username: string;
  employeeCode: string | null;
  displayName: string;
  roleCode: string;
  department: string | null;
  jobTitle: string | null;
  avatarUrl: string | null;
}

interface EmployeeDirectoryRow {
  Id: number;
  Username: string;
  EmployeeCode: string | null;
  DisplayName: string;
  RoleCode: string;
  DepartmentName: string | null;
  JobTitle: string | null;
  AvatarUrl: string | null;
}

interface AvatarRow {
  Content: Buffer;
  ContentType: string | null;
}

export interface EmployeeAvatar {
  content: Buffer;
  contentType: string;
}

interface DirectoryScope {
  canSeeAll: boolean;
  isManager: boolean;
}

@Injectable()
export class UsersService {
  public constructor(private readonly sqlServer: SqlServerService) {}

  public async list(
    identity: AuthenticatedIdentity,
    filter: ListUsersDto,
  ): Promise<EmployeeDirectoryItem[]> {
    const request = await this.requestForVisibleUsers(identity);
    const result = await request
      .input("Take", sql.Int, filter.take ?? 20)
      .input("Keyword", sql.NVarChar(100), filter.keyword?.trim() ?? "")
      .query<EmployeeDirectoryRow>(`${visibleUsersSql}
        AND (@Keyword='' OR u.Username LIKE '%' + @Keyword + '%'
             OR u.DisplayName LIKE '%' + @Keyword + '%'
             OR p.EmployeeCode LIKE '%' + @Keyword + '%')
        ORDER BY d.Name, u.DisplayName, u.Id;`);
    return result.recordset.map(toDirectoryItem);
  }

  public async findVisibleById(
    identity: AuthenticatedIdentity,
    userId: number,
  ): Promise<EmployeeDirectoryItem | undefined> {
    const request = await this.requestForVisibleUsers(identity);
    const result = await request
      .input("UserId", sql.Int, userId)
      .query<EmployeeDirectoryRow>(profileVisibleUserSql);
    const row = result.recordset[0];
    return row ? toDirectoryItem(row) : undefined;
  }

  public async findVisibleAvatar(
    identity: AuthenticatedIdentity,
    userId: number,
  ): Promise<EmployeeAvatar | undefined> {
    const request = await this.requestForVisibleUsers(identity);
    const result = await request
      .input("UserId", sql.Int, userId)
      .query<AvatarRow>(avatarVisibleUserSql);
    const row = result.recordset[0];
    if (!row?.Content || row.Content.length === 0) return undefined;
    return {
      content: row.Content,
      contentType: safeImageContentType(row.ContentType),
    };
  }

  private async requestForVisibleUsers(
    identity: AuthenticatedIdentity,
  ): Promise<sql.Request> {
    const scope = scopeFor(identity);
    const pool = await this.sqlServer.getPool();
    return pool
      .request()
      .input("CanSeeAll", sql.Bit, scope.canSeeAll)
      .input("IsManager", sql.Bit, scope.isManager)
      .input("ActorId", sql.Int, identity.userId)
      .input("DepartmentId", sql.Int, identity.departmentId ?? null);
  }
}

const directoryProjectionSql = `
    u.Id,u.Username,p.EmployeeCode,u.DisplayName,u.RoleCode,
    d.Name AS DepartmentName,p.JobTitle,
    CASE WHEN p.AvatarContent IS NOT NULL
      THEN CONCAT('/api/v1/users/',p.UserId,'/avatar?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
      ELSE p.AvatarUrl END AS AvatarUrl`;

// This mirrors HrmDataStore.GetVisibleUsers: only active non-admin records
// are returned; HR/director/admin have company scope, managers department
// scope, and every other employee has self scope.  The API intentionally
// projects only the directory columns listed in EmployeeDirectoryItem.
const visibleUsersSql = `SELECT TOP (@Take)${directoryProjectionSql}
  FROM dbo.HrmUserAccount u
  LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
  LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
  WHERE u.IsActive=1 AND u.RoleCode<>'ADMIN'
    AND (@CanSeeAll=1 OR u.Id=@ActorId OR (@IsManager=1 AND u.DepartmentId=@DepartmentId))`;

// HrmDataStore.GetVisibleUsers powers the list, while HomeController.MyProfile
// permits an elevated HR/director/admin account to open any company profile.
// The detail route preserves that scope without returning the sensitive fields
// which the legacy profile page uses.  Manager visibility remains the active,
// non-admin department scope from GetVisibleUsers; every account can view self.
const profileVisibleUserSql = `SELECT TOP (1)${directoryProjectionSql}
  FROM dbo.HrmUserAccount u
  LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
  LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
  WHERE u.Id=@UserId
    AND (u.Id=@ActorId OR @CanSeeAll=1
         OR (@IsManager=1 AND u.IsActive=1 AND u.RoleCode<>'ADMIN'
             AND u.DepartmentId=@DepartmentId));`;

const avatarVisibleUserSql = `SELECT TOP (1)
    p.AvatarContent AS Content,p.AvatarContentType AS ContentType
  FROM dbo.HrmUserAccount u
  INNER JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
  WHERE u.Id=@UserId AND p.AvatarContent IS NOT NULL
    AND (u.Id=@ActorId OR @CanSeeAll=1
         OR (@IsManager=1 AND u.IsActive=1 AND u.RoleCode<>'ADMIN'
             AND u.DepartmentId=@DepartmentId));`;

function scopeFor(identity: AuthenticatedIdentity): DirectoryScope {
  return {
    canSeeAll:
      identity.roleCode === "ADMIN" ||
      identity.roleCode === "HR" ||
      identity.roleCode === "DIRECTOR",
    // Keep this strict, as in GetVisibleUsers. HR/director/admin already
    // have company scope through CanSeeAll and are not treated as managers.
    isManager: identity.roleCode === "MANAGER",
  };
}

function toDirectoryItem(row: EmployeeDirectoryRow): EmployeeDirectoryItem {
  return {
    id: row.Id,
    username: row.Username,
    employeeCode: row.EmployeeCode,
    displayName: row.DisplayName,
    roleCode: row.RoleCode,
    department: row.DepartmentName,
    jobTitle: row.JobTitle,
    avatarUrl: row.AvatarUrl,
  };
}

function safeImageContentType(contentType: string | null): string {
  const normalized = contentType?.trim().toLowerCase();
  return normalized === "image/jpeg" ||
    normalized === "image/png" ||
    normalized === "image/gif" ||
    normalized === "image/webp"
    ? normalized
    : "image/jpeg";
}
