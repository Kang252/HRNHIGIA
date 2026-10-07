import * as Joi from "joi";

export type AuthMode = "disabled" | "session" | "proxy-hmac";

export interface ValidatedEnvironment {
  NODE_ENV: "development" | "test" | "production";
  PORT: number;
  HOST: string;
  TRUST_PROXY_HOPS: number;
  API_PREFIX: string;
  LOG_LEVEL: "error" | "warn" | "log" | "debug" | "verbose";
  CLIENT_APP_DIR?: string;
  DB_ENABLED: boolean;
  DB_CONNECT_ON_STARTUP: boolean;
  DB_CONNECTION_STRING?: string;
  ConnectionStrings__MainConnectionString?: string;
  DB_SERVER?: string;
  DB_PORT: number;
  DB_DATABASE?: string;
  DB_USER?: string;
  DB_PASSWORD?: string;
  DB_ENCRYPT: boolean;
  DB_TRUST_SERVER_CERTIFICATE: boolean;
  DB_TLS_SERVER_NAME?: string;
  DB_CONNECT_TIMEOUT_MS: number;
  DB_REQUEST_TIMEOUT_MS: number;
  DB_POOL_MIN: number;
  DB_POOL_MAX: number;
  AUTH_MODE: AuthMode;
  SESSION_COOKIE_NAME: string;
  SESSION_TTL_HOURS: number;
  SESSION_REMEMBER_ME_TTL_HOURS: number;
  LOGIN_RATE_LIMIT_WINDOW_SECONDS: number;
  LOGIN_RATE_LIMIT_MAX_ATTEMPTS: number;
  LOGIN_RATE_LIMIT_MAX_TRACKED_KEYS: number;
  AUTH_PROXY_SHARED_SECRET?: string;
  AUTH_PROXY_MAX_SKEW_SECONDS: number;
}

const booleanValue = Joi.boolean()
  .truthy("true")
  .truthy("1")
  .falsy("false")
  .falsy("0");

const schema = Joi.object<ValidatedEnvironment>({
  NODE_ENV: Joi.string()
    .valid("development", "test", "production")
    .default("development"),
  PORT: Joi.number().port().default(10000),
  HOST: Joi.string()
    .max(255)
    .pattern(/^[A-Za-z0-9.:-]+$/)
    .default("0.0.0.0"),
  TRUST_PROXY_HOPS: Joi.number().integer().min(0).max(5).default(1),
  API_PREFIX: Joi.string()
    .pattern(/^api\/v[1-9][0-9]*$/)
    .default("api/v1"),
  LOG_LEVEL: Joi.string()
    .valid("error", "warn", "log", "debug", "verbose")
    .default("log"),
  CLIENT_APP_DIR: Joi.string().max(1024).optional().allow(""),
  DB_ENABLED: booleanValue.default(false),
  DB_CONNECT_ON_STARTUP: booleanValue.default(false),
  // Either DB_CONNECTION_STRING/ConnectionStrings__MainConnectionString or
  // the individual DB_* settings can supply the database configuration.
  DB_CONNECTION_STRING: Joi.string().max(4096).optional().allow(""),
  ConnectionStrings__MainConnectionString: Joi.string()
    .max(4096)
    .optional()
    .allow(""),
  DB_SERVER: Joi.string().max(255).optional(),
  DB_PORT: Joi.number().port().default(1433),
  DB_DATABASE: Joi.string().max(128).optional(),
  DB_USER: Joi.string().max(128).optional(),
  DB_PASSWORD: Joi.string().max(1024).optional(),
  DB_ENCRYPT: booleanValue.default(true),
  DB_TRUST_SERVER_CERTIFICATE: booleanValue.default(false),
  // Some managed SQL providers route a tenant hostname to a shared SQL
  // instance whose certificate is issued to a different, documented DNS name.
  // This keeps certificate verification enabled while supplying that name as
  // TLS SNI/hostname validation input to the driver.
  DB_TLS_SERVER_NAME: Joi.string().hostname().max(253).optional().allow(""),
  DB_CONNECT_TIMEOUT_MS: Joi.number()
    .integer()
    .min(1000)
    .max(120000)
    .default(5000),
  DB_REQUEST_TIMEOUT_MS: Joi.number()
    .integer()
    .min(1000)
    .max(300000)
    .default(15000),
  DB_POOL_MIN: Joi.number().integer().min(0).max(100).default(0),
  DB_POOL_MAX: Joi.number().integer().min(1).max(100).default(10),
  AUTH_MODE: Joi.string()
    .valid("disabled", "session", "proxy-hmac")
    .default("disabled"),
  SESSION_COOKIE_NAME: Joi.string()
    .pattern(/^[A-Za-z0-9._-]{1,100}$/)
    .default("NHIGIA.Api.Session"),
  SESSION_TTL_HOURS: Joi.number()
    .integer()
    .min(1)
    .max(24 * 7)
    .default(8),
  SESSION_REMEMBER_ME_TTL_HOURS: Joi.number()
    .integer()
    .min(1)
    .max(24 * 30)
    .default(168),
  LOGIN_RATE_LIMIT_WINDOW_SECONDS: Joi.number()
    .integer()
    .min(60)
    .max(3600)
    .default(900),
  LOGIN_RATE_LIMIT_MAX_ATTEMPTS: Joi.number()
    .integer()
    .min(1)
    .max(100)
    .default(10),
  LOGIN_RATE_LIMIT_MAX_TRACKED_KEYS: Joi.number()
    .integer()
    .min(100)
    .max(100000)
    .default(5000),
  AUTH_PROXY_SHARED_SECRET: Joi.string()
    .min(32)
    .max(1024)
    .when("AUTH_MODE", {
      is: "proxy-hmac",
      then: Joi.required(),
      otherwise: Joi.optional().allow(""),
    }),
  AUTH_PROXY_MAX_SKEW_SECONDS: Joi.number()
    .integer()
    .min(10)
    .max(300)
    .default(60),
})
  .custom((value: ValidatedEnvironment, helpers) => {
    if (value.DB_POOL_MIN > value.DB_POOL_MAX) {
      return helpers.error("any.invalid", {
        message: "DB_POOL_MIN cannot exceed DB_POOL_MAX.",
      });
    }
    if (value.SESSION_REMEMBER_ME_TTL_HOURS < value.SESSION_TTL_HOURS) {
      return helpers.error("any.invalid", {
        message:
          "SESSION_REMEMBER_ME_TTL_HOURS cannot be shorter than SESSION_TTL_HOURS.",
      });
    }
    const hasConnectionString = Boolean(
      value.DB_CONNECTION_STRING ||
      value.ConnectionStrings__MainConnectionString,
    );
    const usesDatabase = value.DB_ENABLED || hasConnectionString;
    if (
      usesDatabase &&
      !hasConnectionString &&
      (!value.DB_SERVER ||
        !value.DB_DATABASE ||
        !value.DB_USER ||
        !value.DB_PASSWORD)
    ) {
      return helpers.error("any.invalid", {
        message:
          "Database access requires DB_CONNECTION_STRING/ConnectionStrings__MainConnectionString or DB_SERVER, DB_DATABASE, DB_USER and DB_PASSWORD.",
      });
    }
    if (value.AUTH_MODE === "session" && !usesDatabase) {
      return helpers.error("any.invalid", {
        message: "AUTH_MODE=session requires SQL Server configuration.",
      });
    }
    if (
      value.NODE_ENV === "production" &&
      value.DB_ENABLED &&
      !hasConnectionString &&
      value.DB_TRUST_SERVER_CERTIFICATE
    ) {
      return helpers.error("any.invalid", {
        message: "DB_TRUST_SERVER_CERTIFICATE must be false in production.",
      });
    }
    if (value.NODE_ENV === "production" && value.AUTH_MODE === "disabled") {
      return helpers.error("any.invalid", {
        message: "AUTH_MODE=disabled is not permitted in production.",
      });
    }
    return value;
  })
  .unknown(true);

export function validateEnvironment(
  input: Record<string, unknown>,
): ValidatedEnvironment {
  const { error, value } = schema.validate(input, {
    abortEarly: false,
    convert: true,
    errors: { label: "key" },
  });

  if (error) {
    throw new Error(
      `Invalid NHIGIA.Api configuration: ${error.details.map((detail) => detail.message).join("; ")}`,
    );
  }
  return value;
}
