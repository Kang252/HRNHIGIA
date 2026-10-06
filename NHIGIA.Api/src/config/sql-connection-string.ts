export interface ParsedSqlServerConnectionString {
  server: string;
  port: number;
  database: string;
  user: string;
  password: string;
  encrypt: boolean;
  trustServerCertificate: boolean;
  connectTimeoutMs?: number;
  requestTimeoutMs?: number;
  poolMin?: number;
  poolMax?: number;
}

/**
 * Reads the ADO.NET connection-string subset used by the existing ASP.NET
 * service. It intentionally ignores unrelated provider options and never
 * includes values in thrown errors, so configuration failures cannot leak a
 * password into Render logs.
 */
export function parseSqlServerConnectionString(
  connectionString: string,
): ParsedSqlServerConnectionString {
  const values = parsePairs(connectionString);
  const serverAndPort = parseServer(
    readRequired(
      values,
      ["server", "data source", "address", "addr", "network address"],
      "Server",
    ),
  );
  const database = readRequired(
    values,
    ["database", "initial catalog"],
    "Database",
  );
  const user = readRequired(
    values,
    ["user id", "userid", "uid", "user"],
    "User Id",
  );
  const password = readRequired(values, ["password", "pwd"], "Password");

  if (
    readBoolean(values, ["integrated security", "trusted_connection"], false)
  ) {
    throw new Error(
      "SQL connection string cannot use Integrated Security in NHIGIA.Api. Use a SQL login.",
    );
  }

  const poolMin = readInteger(values, ["min pool size"], 0, 0, 100);
  const poolMax = readInteger(values, ["max pool size"], undefined, 1, 100);
  if (poolMin !== undefined && poolMax !== undefined && poolMin > poolMax) {
    throw new Error(
      "SQL connection string Min Pool Size cannot exceed Max Pool Size.",
    );
  }

  return {
    server: serverAndPort.server,
    port: serverAndPort.port,
    database,
    user,
    password,
    // The Node driver defaults to false, while Azure SQL and the C# service
    // are expected to encrypt transport. Keep the safer default here.
    encrypt: readBoolean(values, ["encrypt"], true),
    trustServerCertificate: readBoolean(
      values,
      ["trustservercertificate", "trust server certificate"],
      false,
    ),
    connectTimeoutMs: secondsToMilliseconds(
      readInteger(
        values,
        ["connect timeout", "connection timeout"],
        undefined,
        1,
        120,
      ),
    ),
    requestTimeoutMs: secondsToMilliseconds(
      readInteger(values, ["command timeout"], undefined, 1, 300),
    ),
    poolMin,
    poolMax,
  };
}

function parsePairs(connectionString: string): Map<string, string> {
  if (!connectionString.trim())
    throw new Error("SQL connection string is empty.");

  const result = new Map<string, string>();
  let part = "";
  let quote: '"' | "'" | undefined;
  const addPart = (rawPart: string): void => {
    const separator = rawPart.indexOf("=");
    if (separator < 1) {
      if (rawPart.trim())
        throw new Error(
          "SQL connection string contains an invalid key/value pair.",
        );
      return;
    }
    const key = rawPart.slice(0, separator).trim().toLowerCase();
    let value = rawPart.slice(separator + 1).trim();
    if (!key) throw new Error("SQL connection string contains an empty key.");
    if (
      (value.startsWith('"') && value.endsWith('"')) ||
      (value.startsWith("'") && value.endsWith("'"))
    ) {
      value = value.slice(1, -1);
    }
    result.set(key, value);
  };

  for (let index = 0; index < connectionString.length; index += 1) {
    const character = connectionString[index];
    if (quote) {
      if (character === quote) {
        if (connectionString[index + 1] === quote) {
          part += character;
          index += 1;
        } else {
          quote = undefined;
          part += character;
        }
      } else {
        part += character;
      }
      continue;
    }

    if (character === '"' || character === "'") {
      quote = character;
      part += character;
    } else if (character === ";") {
      addPart(part);
      part = "";
    } else {
      part += character;
    }
  }
  if (quote)
    throw new Error(
      "SQL connection string contains an unterminated quoted value.",
    );
  addPart(part);
  return result;
}

function readRequired(
  values: Map<string, string>,
  aliases: string[],
  name: string,
): string {
  const value = read(values, aliases);
  if (!value) throw new Error(`SQL connection string must include ${name}.`);
  return value;
}

function read(
  values: Map<string, string>,
  aliases: string[],
): string | undefined {
  for (const alias of aliases) {
    const value = values.get(alias);
    if (value !== undefined) return value;
  }
  return undefined;
}

function readBoolean(
  values: Map<string, string>,
  aliases: string[],
  fallback: boolean,
): boolean {
  const value = read(values, aliases);
  if (value === undefined || value === "") return fallback;
  switch (value.trim().toLowerCase()) {
    case "true":
    case "yes":
    case "1":
      return true;
    case "false":
    case "no":
    case "0":
      return false;
    default:
      throw new Error(
        "SQL connection string contains an invalid boolean option.",
      );
  }
}

function readInteger(
  values: Map<string, string>,
  aliases: string[],
  fallback: number | undefined,
  minimum: number,
  maximum: number,
): number | undefined {
  const value = read(values, aliases);
  if (value === undefined || value === "") return fallback;
  if (!/^\d+$/.test(value) || !Number.isSafeInteger(Number(value))) {
    throw new Error(
      "SQL connection string contains an invalid numeric option.",
    );
  }
  const numberValue = Number(value);
  if (numberValue < minimum || numberValue > maximum) {
    throw new Error(
      "SQL connection string numeric option is outside the supported range.",
    );
  }
  return numberValue;
}

function parseServer(rawServer: string): { server: string; port: number } {
  const withoutProtocol = rawServer.trim().replace(/^tcp:/i, "");
  if (!withoutProtocol || withoutProtocol.includes("\\")) {
    throw new Error(
      "SQL connection string Server must use a host and TCP port, not a named instance.",
    );
  }

  const match = /^(?<host>[^,]+?)(?:,(?<port>\d+))?$/.exec(withoutProtocol);
  const host = match?.groups?.host?.trim();
  const portText = match?.groups?.port;
  if (!host)
    throw new Error("SQL connection string contains an invalid Server value.");
  const port = portText ? Number(portText) : 1433;
  if (!Number.isInteger(port) || port < 1 || port > 65535) {
    throw new Error(
      "SQL connection string Server contains an invalid TCP port.",
    );
  }
  return { server: host, port };
}

function secondsToMilliseconds(value: number | undefined): number | undefined {
  return value === undefined ? undefined : value * 1000;
}
