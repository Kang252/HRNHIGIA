import { Injectable } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import type { ValidatedEnvironment } from "./env.validation";
import { parseSqlServerConnectionString } from "./sql-connection-string";

export interface SqlServerRuntimeConfig {
  enabled: boolean;
  connectOnStartup: boolean;
  server: string;
  port: number;
  database: string;
  user: string;
  password: string;
  encrypt: boolean;
  trustServerCertificate: boolean;
  connectTimeoutMs: number;
  requestTimeoutMs: number;
  poolMin: number;
  poolMax: number;
}

@Injectable()
export class RuntimeConfigService {
  public constructor(
    private readonly config: ConfigService<ValidatedEnvironment, true>,
  ) {}

  public get nodeEnv(): ValidatedEnvironment["NODE_ENV"] {
    return this.config.get("NODE_ENV", { infer: true });
  }

  public get port(): number {
    return this.config.get("PORT", { infer: true });
  }

  public get host(): string {
    return this.config.get("HOST", { infer: true });
  }

  public get trustProxyHops(): number {
    return this.config.get("TRUST_PROXY_HOPS", { infer: true });
  }

  public get apiPrefix(): string {
    return this.config.get("API_PREFIX", { infer: true });
  }

  public get logLevel(): ValidatedEnvironment["LOG_LEVEL"] {
    return this.config.get("LOG_LEVEL", { infer: true });
  }

  public get clientAppDir(): string | undefined {
    return this.config.get("CLIENT_APP_DIR", { infer: true }) || undefined;
  }

  public get authMode(): ValidatedEnvironment["AUTH_MODE"] {
    return this.config.get("AUTH_MODE", { infer: true });
  }

  public get sessionCookieName(): string {
    return this.config.get("SESSION_COOKIE_NAME", { infer: true });
  }

  public get sessionTtlHours(): number {
    return this.config.get("SESSION_TTL_HOURS", { infer: true });
  }

  public get sessionRememberMeTtlHours(): number {
    return this.config.get("SESSION_REMEMBER_ME_TTL_HOURS", { infer: true });
  }

  public get loginRateLimitWindowSeconds(): number {
    return this.config.get("LOGIN_RATE_LIMIT_WINDOW_SECONDS", { infer: true });
  }

  public get loginRateLimitMaxAttempts(): number {
    return this.config.get("LOGIN_RATE_LIMIT_MAX_ATTEMPTS", { infer: true });
  }

  public get loginRateLimitMaxTrackedKeys(): number {
    return this.config.get("LOGIN_RATE_LIMIT_MAX_TRACKED_KEYS", {
      infer: true,
    });
  }

  public get authProxySharedSecret(): string | undefined {
    return (
      this.config.get("AUTH_PROXY_SHARED_SECRET", { infer: true }) || undefined
    );
  }

  public get authProxyMaxSkewSeconds(): number {
    return this.config.get("AUTH_PROXY_MAX_SKEW_SECONDS", { infer: true });
  }

  public get sqlServer(): SqlServerRuntimeConfig {
    const connectionString = this.connectionString;
    if (connectionString) {
      const parsed = parseSqlServerConnectionString(connectionString);
      if (this.nodeEnv === "production" && parsed.trustServerCertificate) {
        throw new Error(
          "SQL TrustServerCertificate must be false in production.",
        );
      }
      return {
        enabled: true,
        connectOnStartup: this.config.get("DB_CONNECT_ON_STARTUP", {
          infer: true,
        }),
        server: parsed.server,
        port: parsed.port,
        database: parsed.database,
        user: parsed.user,
        password: parsed.password,
        encrypt: parsed.encrypt,
        trustServerCertificate: parsed.trustServerCertificate,
        connectTimeoutMs:
          parsed.connectTimeoutMs ??
          this.config.get("DB_CONNECT_TIMEOUT_MS", { infer: true }),
        requestTimeoutMs:
          parsed.requestTimeoutMs ??
          this.config.get("DB_REQUEST_TIMEOUT_MS", { infer: true }),
        poolMin:
          parsed.poolMin ?? this.config.get("DB_POOL_MIN", { infer: true }),
        poolMax:
          parsed.poolMax ?? this.config.get("DB_POOL_MAX", { infer: true }),
      };
    }

    return {
      enabled: this.config.get("DB_ENABLED", { infer: true }),
      connectOnStartup: this.config.get("DB_CONNECT_ON_STARTUP", {
        infer: true,
      }),
      server: this.config.get("DB_SERVER", { infer: true }) ?? "",
      port: this.config.get("DB_PORT", { infer: true }),
      database: this.config.get("DB_DATABASE", { infer: true }) ?? "",
      user: this.config.get("DB_USER", { infer: true }) ?? "",
      password: this.config.get("DB_PASSWORD", { infer: true }) ?? "",
      encrypt: this.config.get("DB_ENCRYPT", { infer: true }),
      trustServerCertificate: this.config.get("DB_TRUST_SERVER_CERTIFICATE", {
        infer: true,
      }),
      connectTimeoutMs: this.config.get("DB_CONNECT_TIMEOUT_MS", {
        infer: true,
      }),
      requestTimeoutMs: this.config.get("DB_REQUEST_TIMEOUT_MS", {
        infer: true,
      }),
      poolMin: this.config.get("DB_POOL_MIN", { infer: true }),
      poolMax: this.config.get("DB_POOL_MAX", { infer: true }),
    };
  }

  private get connectionString(): string | undefined {
    // DB_CONNECTION_STRING is explicit for this API; the .NET-compatible key
    // lets a Render service reuse the existing C# secret without duplicating it.
    const apiValue = this.config
      .get("DB_CONNECTION_STRING", { infer: true })
      ?.trim();
    const dotnetValue = this.config
      .get("ConnectionStrings__MainConnectionString", { infer: true })
      ?.trim();
    return apiValue || dotnetValue || undefined;
  }
}
