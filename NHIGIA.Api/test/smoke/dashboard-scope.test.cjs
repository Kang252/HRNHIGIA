const assert = require("node:assert/strict");
const test = require("node:test");

const { sqlScope } = require("../../dist/dashboard/dashboard.service.js");

function identity(roleCode) {
  return {
    userId: 42,
    username: "smoke-test",
    displayName: "Smoke Test",
    roleCode,
    departmentId: 7,
  };
}

test("dashboard scope keeps an employee limited to their own records", () => {
  assert.deepEqual(sqlScope(identity("EMPLOYEE")), {
    canSeeAll: false,
    isManager: false,
  });
});

test("dashboard scope grants department visibility only to a manager", () => {
  assert.deepEqual(sqlScope(identity("MANAGER")), {
    canSeeAll: false,
    isManager: true,
  });
});

for (const roleCode of ["HR", "DIRECTOR", "ADMIN"]) {
  test(`dashboard scope grants company visibility to ${roleCode}`, () => {
    assert.deepEqual(sqlScope(identity(roleCode)), {
      canSeeAll: true,
      isManager: true,
    });
  });
}

test("dashboard scope defaults unknown roles to self-only access", () => {
  assert.deepEqual(sqlScope(identity("UNRECOGNIZED")), {
    canSeeAll: false,
    isManager: false,
  });
});
