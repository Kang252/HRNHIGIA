const assert = require("node:assert/strict");
const test = require("node:test");

const {
  apiSessionSchemaFromRow,
} = require("../../dist/database/sql-server.service.js");

const readyRow = {
  TablePresent: true,
  IdPresent: true,
  TokenHashPresent: true,
  UserIdPresent: true,
  CreatedAtPresent: true,
  LastSeenAtPresent: true,
  ExpiresAtPresent: true,
  RevokedAtPresent: true,
  IpAddressPresent: true,
  UserAgentHashPresent: true,
};

test("accepts the schema created by the additive API-session migration", () => {
  assert.deepEqual(apiSessionSchemaFromRow(readyRow), {
    required: true,
    ready: true,
    missing: [],
  });
});

test("reports missing session migration columns so health/startup cannot go green", () => {
  const result = apiSessionSchemaFromRow({
    ...readyRow,
    TokenHashPresent: false,
    UserAgentHashPresent: false,
  });
  assert.deepEqual(result, {
    required: true,
    ready: false,
    missing: ["TokenHash", "UserAgentHash"],
  });
});
