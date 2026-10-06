const assert = require("node:assert/strict");
const test = require("node:test");

const {
  parseSqlServerConnectionString,
} = require("../../dist/config/sql-connection-string.js");

test("parses the supported SQL Server connection-string subset", () => {
  const parsed = parseSqlServerConnectionString(
    "Server=tcp:sql.nhigia.test,1444;Initial Catalog=NHIGIA;User Id=hrm_api;Password='test;value';Encrypt=true;Trust Server Certificate=false;Connect Timeout=12;Command Timeout=35;Min Pool Size=2;Max Pool Size=12;",
  );

  assert.deepEqual(parsed, {
    server: "sql.nhigia.test",
    port: 1444,
    database: "NHIGIA",
    user: "hrm_api",
    password: "test;value",
    encrypt: true,
    trustServerCertificate: false,
    connectTimeoutMs: 12_000,
    requestTimeoutMs: 35_000,
    poolMin: 2,
    poolMax: 12,
  });
});

test("rejects named SQL Server instances rather than silently using a dynamic port", () => {
  assert.throws(
    () =>
      parseSqlServerConnectionString(
        "Server=sql.nhigia.test\\SQLEXPRESS;Database=NHIGIA;User Id=hrm_api;Password=not-a-secret;",
      ),
    /host and TCP port, not a named instance/i,
  );
});

test("rejects Integrated Security because the API requires a SQL login", () => {
  assert.throws(
    () =>
      parseSqlServerConnectionString(
        "Server=sql.nhigia.test,1433;Database=NHIGIA;User Id=hrm_api;Password=not-a-secret;Integrated Security=true;",
      ),
    /cannot use Integrated Security/i,
  );
});
