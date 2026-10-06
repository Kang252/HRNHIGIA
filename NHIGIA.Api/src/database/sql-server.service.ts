import {
  Injectable,
  Logger,
  OnApplicationShutdown,
  OnModuleInit,
  ServiceUnavailableException,
} from "@nestjs/common";
import * as sql from "mssql";
import {
  RuntimeConfigService,
  type SqlServerRuntimeConfig,
} from "../config/runtime-config.service";

export interface SqlServerHealth {
  configured: boolean;
  connected: boolean;
  latencyMs?: number;
  apiSessionSchema?: ApiSessionSchemaHealth;
}

export interface ApiSessionSchemaHealth {
  required: true;
  ready: boolean;
  missing: string[];
}

@Injectable()
export class SqlServerService implements OnModuleInit, OnApplicationShutdown {
  private readonly logger = new Logger(SqlServerService.name);
  private readonly options: SqlServerRuntimeConfig;
  private readonly requiresApiSessionSchema: boolean;
  private pool?: sql.ConnectionPool;
  private connecting?: Promise<sql.ConnectionPool>;

  public constructor(config: RuntimeConfigService) {
    this.options = config.sqlServer;
    this.requiresApiSessionSchema = config.authMode === "session";
  }

  public async onModuleInit(): Promise<void> {
    if (
      this.options.enabled &&
      (this.options.connectOnStartup || this.requiresApiSessionSchema)
    ) {
      const pool = await this.getPool();
      if (this.requiresApiSessionSchema) {
        await this.assertApiSessionSchema(pool);
      }
      this.logger.log(
        `Connected to configured SQL Server database '${this.options.database}'.`,
      );
    } else if (!this.options.enabled) {
      this.logger.warn(
        "SQL Server is disabled. Only foundation health checks are available.",
      );
    }
  }

  public async onApplicationShutdown(): Promise<void> {
    const pool = this.pool;
    this.pool = undefined;
    if (pool?.connected) {
      await pool.close();
      this.logger.log("SQL Server connection pool closed.");
    }
  }

  public async getPool(): Promise<sql.ConnectionPool> {
    if (!this.options.enabled) {
      throw new ServiceUnavailableException(
        "SQL Server access is not configured for NHIGIA.Api.",
      );
    }
    if (this.pool?.connected) return this.pool;
    if (this.connecting) return this.connecting;

    const pool = new sql.ConnectionPool({
      server: this.options.server,
      port: this.options.port,
      database: this.options.database,
      user: this.options.user,
      password: this.options.password,
      connectionTimeout: this.options.connectTimeoutMs,
      requestTimeout: this.options.requestTimeoutMs,
      pool: {
        min: this.options.poolMin,
        max: this.options.poolMax,
        idleTimeoutMillis: 30000,
      },
      options: {
        encrypt: this.options.encrypt,
        trustServerCertificate: this.options.trustServerCertificate,
        enableArithAbort: true,
      },
    });

    this.connecting = pool
      .connect()
      .then((connectedPool) => {
        this.pool = connectedPool;
        return connectedPool;
      })
      .catch(async (error: unknown) => {
        await pool.close().catch(() => undefined);
        this.logger.error(
          `Unable to connect to the configured SQL Server. errorType=${errorType(error)}`,
        );
        throw error;
      })
      .finally(() => {
        this.connecting = undefined;
      });

    return this.connecting;
  }

  public async health(): Promise<SqlServerHealth> {
    if (!this.options.enabled) return { configured: false, connected: false };

    const startedAt = performance.now();
    try {
      const pool = await this.getPool();
      await pool.request().query("SELECT 1 AS health_check;");
      const apiSessionSchema = this.requiresApiSessionSchema
        ? await this.getApiSessionSchemaHealth(pool)
        : undefined;
      return {
        configured: true,
        connected: true,
        latencyMs: Math.round((performance.now() - startedAt) * 10) / 10,
        ...(apiSessionSchema ? { apiSessionSchema } : {}),
      };
    } catch {
      return {
        configured: true,
        connected: false,
        latencyMs: Math.round((performance.now() - startedAt) * 10) / 10,
      };
    }
  }

  private async assertApiSessionSchema(
    pool: sql.ConnectionPool,
  ): Promise<void> {
    const schema = await this.getApiSessionSchemaHealth(pool);
    if (schema.ready) return;
    throw new Error(
      `NHIGIA.Api session migration is incomplete. Missing: ${schema.missing.join(", ")}. Apply NHIGIA.Api/sql/001_api_sessions.sql before enabling AUTH_MODE=session.`,
    );
  }

  private async getApiSessionSchemaHealth(
    pool: sql.ConnectionPool,
  ): Promise<ApiSessionSchemaHealth> {
    const result = await pool.request().query<ApiSessionSchemaRow>(`
      SELECT
        CAST(CASE WHEN OBJECT_ID(N'dbo.HrmApiSession', N'U') IS NULL THEN 0 ELSE 1 END AS bit) AS TablePresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'Id') IS NULL THEN 0 ELSE 1 END AS bit) AS IdPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'TokenHash') IS NULL THEN 0 ELSE 1 END AS bit) AS TokenHashPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'UserId') IS NULL THEN 0 ELSE 1 END AS bit) AS UserIdPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit) AS CreatedAtPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'LastSeenAt') IS NULL THEN 0 ELSE 1 END AS bit) AS LastSeenAtPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'ExpiresAt') IS NULL THEN 0 ELSE 1 END AS bit) AS ExpiresAtPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'RevokedAt') IS NULL THEN 0 ELSE 1 END AS bit) AS RevokedAtPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'IpAddress') IS NULL THEN 0 ELSE 1 END AS bit) AS IpAddressPresent,
        CAST(CASE WHEN COL_LENGTH(N'dbo.HrmApiSession', N'UserAgentHash') IS NULL THEN 0 ELSE 1 END AS bit) AS UserAgentHashPresent;`);
    return apiSessionSchemaFromRow(result.recordset[0]);
  }
}

interface ApiSessionSchemaRow {
  TablePresent?: boolean | number | null;
  IdPresent?: boolean | number | null;
  TokenHashPresent?: boolean | number | null;
  UserIdPresent?: boolean | number | null;
  CreatedAtPresent?: boolean | number | null;
  LastSeenAtPresent?: boolean | number | null;
  ExpiresAtPresent?: boolean | number | null;
  RevokedAtPresent?: boolean | number | null;
  IpAddressPresent?: boolean | number | null;
  UserAgentHashPresent?: boolean | number | null;
}

const requiredApiSessionSchemaFields: ReadonlyArray<
  readonly [string, keyof ApiSessionSchemaRow]
> = [
  ["dbo.HrmApiSession", "TablePresent"],
  ["Id", "IdPresent"],
  ["TokenHash", "TokenHashPresent"],
  ["UserId", "UserIdPresent"],
  ["CreatedAt", "CreatedAtPresent"],
  ["LastSeenAt", "LastSeenAtPresent"],
  ["ExpiresAt", "ExpiresAtPresent"],
  ["RevokedAt", "RevokedAtPresent"],
  ["IpAddress", "IpAddressPresent"],
  ["UserAgentHash", "UserAgentHashPresent"],
];

export function apiSessionSchemaFromRow(
  row: ApiSessionSchemaRow | undefined,
): ApiSessionSchemaHealth {
  const missing = requiredApiSessionSchemaFields
    .filter(([, property]) => {
      const value = row?.[property];
      return value !== true && value !== 1;
    })
    .map(([name]) => name);
  return {
    required: true,
    ready: missing.length === 0,
    missing,
  };
}

function errorType(error: unknown): string {
  return error instanceof Error ? error.name : "UnknownError";
}
