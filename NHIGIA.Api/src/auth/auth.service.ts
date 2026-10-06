import {
  Injectable,
  Logger,
  ServiceUnavailableException,
  UnauthorizedException,
} from "@nestjs/common";
import { createHash, pbkdf2, randomBytes, timingSafeEqual } from "node:crypto";
import { promisify } from "node:util";
import * as sql from "mssql";
import type { Request } from "express";
import { RuntimeConfigService } from "../config/runtime-config.service";
import { SqlServerService } from "../database/sql-server.service";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import type { LoginDto } from "./login.dto";
import { LoginRateLimitService } from "./login-rate-limit.service";

const derivePbkdf2 = promisify(pbkdf2);
const currentHrmIterations = 100000;
const currentHrmKeyLength = 32;
const timingSafeFallbackSalt = Buffer.alloc(16).toString("base64");
const timingSafeFallbackHash =
  Buffer.alloc(currentHrmKeyLength).toString("base64");

interface LoginAccountRow {
  Id: number;
  Username: string;
  PasswordHash: string;
  PasswordSalt: string;
  DisplayName: string | null;
  RoleCode: AuthenticatedIdentity["roleCode"];
  DepartmentId: number | null;
  DepartmentName: string | null;
  SupervisorUserId: number | null;
  IsActive: boolean;
}

interface SessionIdentityRow {
  SessionId: number;
  Id: number;
  Username: string;
  DisplayName: string | null;
  RoleCode: AuthenticatedIdentity["roleCode"];
  DepartmentId: number | null;
  DepartmentName: string | null;
  SupervisorUserId: number | null;
}

export interface LoginResult {
  identity: AuthenticatedIdentity;
  token: string;
  maxAgeMs: number;
}

@Injectable()
export class AuthService {
  private readonly logger = new Logger(AuthService.name);

  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly config: RuntimeConfigService,
    private readonly loginRateLimit: LoginRateLimitService,
  ) {}

  public async login(input: LoginDto, request: Request): Promise<LoginResult> {
    this.ensureSessionMode();
    const username = input.username.trim();
    const password = input.password;
    if (!username || !password)
      throw new UnauthorizedException(
        "Tên đăng nhập hoặc mật khẩu không đúng.",
      );
    const ipAddress = cleanIp(request.ip);
    this.loginRateLimit.consume(ipAddress);

    let account: LoginAccountRow | undefined;
    try {
      const pool = await this.sqlServer.getPool();
      const result = await pool
        .request()
        .input("Username", sql.NVarChar(100), username)
        .query<LoginAccountRow>(`SELECT u.Id,u.Username,u.PasswordHash,u.PasswordSalt,u.DisplayName,u.RoleCode,
          u.DepartmentId,d.Name AS DepartmentName,u.SupervisorUserId,u.IsActive
          FROM dbo.HrmUserAccount u
          LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
          WHERE u.Username=@Username`);
      account = result.recordset[0];
    } catch (error: unknown) {
      this.loginRateLimit.reset(ipAddress);
      this.logDatabaseFailure("login lookup", error);
      throw new ServiceUnavailableException(
        "Dịch vụ đăng nhập đang chưa sẵn sàng.",
      );
    }

    // Always perform the same KDF work so account existence and activation do
    // not create an observable fast-failure path for credential probing.
    const passwordMatches = await verifyExistingPassword(
      password,
      account?.PasswordSalt ?? timingSafeFallbackSalt,
      account?.PasswordHash ?? timingSafeFallbackHash,
    );
    if (!account || !account.IsActive || !passwordMatches) {
      throw new UnauthorizedException(
        "Tên đăng nhập hoặc mật khẩu không đúng.",
      );
    }
    this.loginRateLimit.reset(ipAddress);

    const identity = identityFrom(account);
    const token = randomBytes(32).toString("base64url");
    const tokenHash = hashToken(token);
    const maxAgeMs =
      (input.rememberMe
        ? this.config.sessionRememberMeTtlHours
        : this.config.sessionTtlHours) *
      60 *
      60 *
      1000;
    const expiresAt = new Date(Date.now() + maxAgeMs);
    const userAgentHash = hashUserAgent(request.header("user-agent"));

    try {
      const pool = await this.sqlServer.getPool();
      const transaction = new sql.Transaction(pool);
      await transaction.begin(sql.ISOLATION_LEVEL.READ_COMMITTED);
      try {
        await new sql.Request(transaction)
          .input("TokenHash", sql.Char(64), tokenHash)
          .input("UserId", sql.Int, identity.userId)
          .input("ExpiresAt", sql.DateTime2, expiresAt)
          .input("IpAddress", sql.NVarChar(80), ipAddress)
          .input("UserAgentHash", sql.Char(64), userAgentHash)
          .query(`DELETE FROM dbo.HrmApiSession
                    WHERE ExpiresAt<=SYSUTCDATETIME()
                       OR (RevokedAt IS NOT NULL AND RevokedAt<DATEADD(day,-30,SYSUTCDATETIME()));
                  INSERT dbo.HrmApiSession(TokenHash,UserId,ExpiresAt,IpAddress,UserAgentHash)
                    VALUES(@TokenHash,@UserId,@ExpiresAt,@IpAddress,@UserAgentHash);
                  UPDATE dbo.HrmUserAccount SET LastLoginAt=SYSDATETIME() WHERE Id=@UserId;
                  INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
                    VALUES(@UserId,'LOGIN','HrmUserAccount',CONVERT(nvarchar(20),@UserId),N'Đăng nhập qua NHIGIA.Api',@IpAddress);`);
        await transaction.commit();
      } catch (error: unknown) {
        await transaction.rollback().catch(() => undefined);
        throw error;
      }
    } catch (error: unknown) {
      this.logDatabaseFailure("session creation", error);
      throw new ServiceUnavailableException(
        "Dịch vụ đăng nhập đang chưa sẵn sàng.",
      );
    }

    return { identity, token, maxAgeMs };
  }

  public async findIdentity(
    token: string | undefined,
    request: Request,
  ): Promise<AuthenticatedIdentity | undefined> {
    this.ensureSessionMode();
    if (!isTokenShapeValid(token)) return undefined;
    const tokenHash = hashToken(token);
    try {
      const pool = await this.sqlServer.getPool();
      const result = await pool
        .request()
        .input("TokenHash", sql.Char(64), tokenHash)
        .query<SessionIdentityRow>(`SELECT TOP 1 s.Id AS SessionId,u.Id,u.Username,u.DisplayName,u.RoleCode,
          u.DepartmentId,d.Name AS DepartmentName,u.SupervisorUserId
          FROM dbo.HrmApiSession s
          INNER JOIN dbo.HrmUserAccount u ON u.Id=s.UserId AND u.IsActive=1
          LEFT JOIN dbo.HrmDepartment d ON d.Id=u.DepartmentId
          WHERE s.TokenHash=@TokenHash AND s.RevokedAt IS NULL AND s.ExpiresAt>SYSUTCDATETIME();`);
      const session = result.recordset[0];
      if (!session) return undefined;

      await pool
        .request()
        .input("SessionId", sql.BigInt, session.SessionId)
        .input("IpAddress", sql.NVarChar(80), cleanIp(request.ip))
        .query(`UPDATE dbo.HrmApiSession SET LastSeenAt=SYSUTCDATETIME(),IpAddress=COALESCE(@IpAddress,IpAddress)
                WHERE Id=@SessionId AND RevokedAt IS NULL;`);
      return identityFrom(session);
    } catch (error: unknown) {
      this.logDatabaseFailure("session validation", error);
      throw new ServiceUnavailableException(
        "Dịch vụ xác thực đang chưa sẵn sàng.",
      );
    }
  }

  public async logout(token: string | undefined): Promise<void> {
    if (!isTokenShapeValid(token) || !this.config.sqlServer.enabled) return;
    try {
      const pool = await this.sqlServer.getPool();
      await pool.request().input("TokenHash", sql.Char(64), hashToken(token))
        .query(`UPDATE dbo.HrmApiSession
                SET RevokedAt=COALESCE(RevokedAt,SYSUTCDATETIME())
                WHERE TokenHash=@TokenHash;`);
    } catch (error: unknown) {
      this.logDatabaseFailure("logout", error);
      throw new ServiceUnavailableException(
        "Dịch vụ đăng xuất đang chưa sẵn sàng.",
      );
    }
  }

  public sessionCookie(
    token: string,
    maxAgeMs: number,
  ): {
    name: string;
    value: string;
    maxAge: number;
    httpOnly: true;
    secure: boolean;
    sameSite: "lax";
    path: "/";
  } {
    return {
      name: this.config.sessionCookieName,
      value: token,
      maxAge: maxAgeMs,
      httpOnly: true,
      secure: this.config.nodeEnv === "production",
      sameSite: "lax",
      path: "/",
    };
  }

  public clearSessionCookie(): {
    name: string;
    httpOnly: true;
    secure: boolean;
    sameSite: "lax";
    path: "/";
  } {
    return {
      name: this.config.sessionCookieName,
      httpOnly: true,
      secure: this.config.nodeEnv === "production",
      sameSite: "lax",
      path: "/",
    };
  }

  public tokenFromRequest(request: Request): string | undefined {
    const cookie = request.headers.cookie;
    if (!cookie) return undefined;
    const target = `${this.config.sessionCookieName}=`;
    for (const part of cookie.split(";")) {
      const trimmed = part.trim();
      if (!trimmed.startsWith(target)) continue;
      const value = trimmed.slice(target.length);
      return value || undefined;
    }
    return undefined;
  }

  private ensureSessionMode(): void {
    if (this.config.authMode !== "session") {
      throw new ServiceUnavailableException(
        "Đăng nhập trực tiếp chưa được bật cho NHIGIA.Api.",
      );
    }
  }

  private logDatabaseFailure(operation: string, error: unknown): void {
    this.logger.error(
      `Database failure during ${operation}. errorType=${errorType(error)}`,
    );
  }
}

function identityFrom(
  row:
    | Omit<LoginAccountRow, "PasswordHash" | "PasswordSalt" | "IsActive">
    | SessionIdentityRow,
): AuthenticatedIdentity {
  return {
    userId: row.Id,
    username: row.Username,
    roleCode: row.RoleCode,
    ...(row.DepartmentId ? { departmentId: row.DepartmentId } : {}),
    ...(row.DisplayName ? { displayName: row.DisplayName } : {}),
    ...(row.DepartmentName ? { departmentName: row.DepartmentName } : {}),
    ...(row.SupervisorUserId ? { supervisorUserId: row.SupervisorUserId } : {}),
  };
}

async function verifyExistingPassword(
  password: string,
  saltBase64: string,
  expectedHashBase64: string,
): Promise<boolean> {
  try {
    const salt = Buffer.from(saltBase64, "base64");
    const expected = Buffer.from(expectedHashBase64, "base64");
    if (salt.length === 0 || expected.length !== currentHrmKeyLength)
      return false;
    const actual = await derivePbkdf2(
      password,
      salt,
      currentHrmIterations,
      currentHrmKeyLength,
      "sha1",
    );
    return (
      actual.length === expected.length && timingSafeEqual(actual, expected)
    );
  } catch {
    return false;
  }
}

function hashToken(token: string): string {
  return createHash("sha256").update(token, "utf8").digest("hex");
}

function hashUserAgent(userAgent: string | undefined): string | null {
  if (!userAgent) return null;
  return createHash("sha256")
    .update(userAgent.slice(0, 1024), "utf8")
    .digest("hex");
}

function cleanIp(ip: string | undefined): string | null {
  if (!ip) return null;
  return ip.slice(0, 80);
}

function isTokenShapeValid(token: string | undefined): token is string {
  return typeof token === "string" && /^[A-Za-z0-9_-]{43}$/.test(token);
}

function errorType(error: unknown): string {
  return error instanceof Error ? error.name : "UnknownError";
}
