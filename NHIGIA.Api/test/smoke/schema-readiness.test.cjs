const assert = require("node:assert/strict");
const test = require("node:test");

const {
  featureSchemaHealthFromRows,
  featureSchemaRequirements,
  schemaProbeSql,
  schemaUnavailableMessage,
} = require("../../dist/database/schema-readiness.service.js");

test("reports every absent notification column as a migration requirement", () => {
  const required = featureSchemaRequirements.notifications;
  const rows = required
    .filter((name) => name !== "dbo.HrmNotification.LinkUrl")
    .map((Requirement) => ({ Requirement, IsPresent: true }));

  const result = featureSchemaHealthFromRows(
    "notifications",
    rows,
    new Date("2026-10-06T00:00:00.000Z"),
  );

  assert.equal(result.ready, false);
  assert.equal(result.unavailable, false);
  assert.deepEqual(result.missing, ["dbo.HrmNotification.LinkUrl"]);
  assert.match(
    schemaUnavailableMessage("notifications", result),
    /Schema cơ sở dữ liệu cho Thông báo chưa sẵn sàng/,
  );
});

test("requires all static communications requirements before marking ready", () => {
  const rows = featureSchemaRequirements.communications.map((Requirement) => ({
    Requirement,
    IsPresent: 1,
  }));
  const result = featureSchemaHealthFromRows("communications", rows);

  assert.equal(result.ready, true);
  assert.deepEqual(result.missing, []);
});

test("keeps every static schema probe aligned with its declared requirements", () => {
  for (const [feature, requirements] of Object.entries(
    featureSchemaRequirements,
  )) {
    const probeRequirements = new Set(
      [...schemaProbeSql[feature].matchAll(/N'(dbo\.[^']+)'/g)].map(
        (match) => match[1],
      ),
    );
    assert.deepEqual(
      [...probeRequirements].sort(),
      [...requirements].sort(),
      feature,
    );
  }
});
