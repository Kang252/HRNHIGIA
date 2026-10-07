const assert = require("node:assert/strict");
const test = require("node:test");

const {
  canReview,
  remoteScopeFor,
  resolveRange,
  validatePlan,
} = require("../../dist/remote-attendance/remote-attendance.service.js");

function identity(roleCode, departmentId = 7, userId = 42) {
  return {
    userId,
    username: "smoke-test",
    displayName: "Smoke Test",
    roleCode,
    departmentId,
  };
}

test("remote-attendance keeps the legacy self, manager, and company scopes", () => {
  assert.deepEqual(remoteScopeFor(identity("EMPLOYEE")), {
    canSeeAll: false,
    isManager: false,
  });
  assert.deepEqual(remoteScopeFor(identity("MANAGER")), {
    canSeeAll: false,
    isManager: true,
  });
  for (const roleCode of ["HR", "DIRECTOR", "ADMIN"]) {
    assert.deepEqual(remoteScopeFor(identity(roleCode)), {
      canSeeAll: true,
      isManager: false,
    });
  }

  const manager = identity("MANAGER");
  assert.equal(canReview(manager, 9, 7, "EMPLOYEE"), true);
  assert.equal(canReview(manager, 9, 8, "EMPLOYEE"), false);
  assert.equal(canReview(manager, 9, 7, "MANAGER"), false);
  assert.equal(canReview(manager, 42, 7, "EMPLOYEE"), false);
});

test("remote-attendance range keeps date-only boundaries and the 100-day limit", () => {
  assert.deepEqual(
    resolveRange({ fromDate: "2026-10-01", toDate: "2026-10-31" }),
    {
      from: new Date("2026-10-01T00:00:00.000Z"),
      to: new Date("2026-10-31T00:00:00.000Z"),
    },
  );
  assert.throws(
    () => resolveRange({ fromDate: "2026-02-30", toDate: "2026-03-01" }),
    /Ngày không hợp lệ/,
  );
  assert.throws(
    () => resolveRange({ fromDate: "2026-01-01", toDate: "2026-04-12" }),
    /Khoảng xem tối đa 100 ngày/,
  );
});

test("remote work plan defaults and home geofence validation remain explicit", () => {
  const field = validatePlan({
    mode: "FIELD",
    placeName: "Điểm khách hàng A",
    fromDate: "2026-10-12",
    toDate: "2026-10-12",
    workDaysMask: 2,
  });
  assert.equal(field.windowStart, "08:00:00");
  assert.equal(field.windowEnd, "17:30:00");
  assert.equal(field.requiredMinutes, 480);
  assert.equal(field.breakMinutes, 60);
  assert.equal(field.radiusMeters, 200);

  assert.throws(
    () =>
      validatePlan({
        mode: "HOME",
        placeName: "Nhà riêng",
        fromDate: "2026-10-12",
        toDate: "2026-10-12",
        workDaysMask: 2,
      }),
    /Lịch làm tại nhà cần tọa độ đăng ký/,
  );
  assert.throws(
    () =>
      validatePlan({
        mode: "FIELD",
        placeName: "Điểm khách hàng A",
        fromDate: "2026-10-12",
        toDate: "2026-10-12",
        workDaysMask: 2,
        windowStart: "17:30",
        windowEnd: "08:00",
      }),
    /Khung giờ phải bắt đầu và kết thúc trong cùng ngày/,
  );
});
