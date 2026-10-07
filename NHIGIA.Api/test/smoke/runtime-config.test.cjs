const assert = require("node:assert/strict");
const test = require("node:test");

const {
  RuntimeConfigService,
} = require("../../dist/config/runtime-config.service.js");

function runtimeConfig(values) {
  return new RuntimeConfigService({
    get(key) {
      return values[key];
    },
  });
}

test("passes an explicit TLS server name to SQL runtime configuration", () => {
  const service = runtimeConfig({
    NODE_ENV: "production",
    DB_CONNECTION_STRING:
      "Server=tenant.sql.example;Database=NHIGIA;User Id=api;Password=not-a-secret;Encrypt=True;TrustServerCertificate=False",
    DB_TLS_SERVER_NAME: "shared.sql.example",
    DB_CONNECT_ON_STARTUP: true,
    DB_CONNECT_TIMEOUT_MS: 5000,
    DB_REQUEST_TIMEOUT_MS: 15000,
    DB_POOL_MIN: 0,
    DB_POOL_MAX: 10,
  });

  assert.equal(service.sqlServer.server, "tenant.sql.example");
  assert.equal(service.sqlServer.tlsServerName, "shared.sql.example");
  assert.equal(service.sqlServer.trustServerCertificate, false);
});

test("does not require a TLS server-name override for normal SQL hosts", () => {
  const service = runtimeConfig({
    NODE_ENV: "production",
    DB_CONNECTION_STRING:
      "Server=sql.example;Database=NHIGIA;User Id=api;Password=not-a-secret;Encrypt=True;TrustServerCertificate=False",
    DB_CONNECT_ON_STARTUP: true,
    DB_CONNECT_TIMEOUT_MS: 5000,
    DB_REQUEST_TIMEOUT_MS: 15000,
    DB_POOL_MIN: 0,
    DB_POOL_MAX: 10,
  });

  assert.equal(service.sqlServer.tlsServerName, undefined);
});
