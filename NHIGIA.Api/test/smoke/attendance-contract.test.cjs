const assert = require("node:assert/strict");
const test = require("node:test");

const {
  attendanceScopeFor,
  projectAttendance,
  resolveAttendanceRange,
} = require("../../dist/attendance/attendance.service.js");

function identity(roleCode) {
  return {
    userId: 42,
    username: "smoke-test",
    displayName: "Smoke Test",
    roleCode,
    departmentId: 7,
  };
}

function range(fromDate, toDate) {
  return resolveAttendanceRange({ fromDate, toDate });
}

test("attendance visibility scopes self, department, and company correctly", () => {
  assert.deepEqual(attendanceScopeFor(identity("EMPLOYEE")), {
    canSeeAll: false,
    isManager: false,
  });
  assert.deepEqual(attendanceScopeFor(identity("MANAGER")), {
    canSeeAll: false,
    isManager: true,
  });
  for (const roleCode of ["HR", "DIRECTOR", "ADMIN"]) {
    assert.deepEqual(attendanceScopeFor(identity(roleCode)), {
      canSeeAll: true,
      isManager: false,
    });
  }
});

test("attendance date range validates calendar dates and the 366-day limit", () => {
  assert.deepEqual(range("2026-10-01", "2026-10-31"), {
    fromDate: "2026-10-01",
    toDate: "2026-10-31",
    from: new Date("2026-10-01T00:00:00.000Z"),
    to: new Date("2026-10-31T00:00:00.000Z"),
  });
  assert.throws(
    () => range("2026-02-30", "2026-03-01"),
    /Khoảng ngày chấm công không hợp lệ/,
  );
  assert.throws(
    () => range("2026-01-01", "2027-01-03"),
    /Khoảng lọc tối đa là 366 ngày/,
  );
});

test("attendance restores HANET checkout after a shift and retains late/early status", () => {
  const records = projectAttendance(
    [
      {
        UserId: 42,
        EmployeeCode: "HN-42",
        DisplayName: "Smoke Test",
        JobTitle: "Nhân viên",
        DepartmentName: "Công nghệ thông tin",
        WorkDate: "2026-10-01",
        ShiftName: "Ca hành chính",
        ScheduledStart: "08:00:00",
        ScheduledEnd: "17:30:00",
        GraceMinutes: 0,
        BreakMinutes: 60,
        HasExplicitPunches: 0,
        IsFlexible: 0,
        RequiredMinutes: 0,
        CheckIn: "2026-10-01T08:05:00",
        CheckOut: null,
        LastSeen: "2026-10-01T17:00:00",
        EventCount: 2,
        Source: "HANET",
      },
    ],
    [],
    [],
    range("2026-10-01", "2026-10-01"),
    new Date("2026-10-01T18:00:00+07:00"),
  );

  assert.deepEqual(records[0], {
    userId: 42,
    employeeCode: "HN-42",
    displayName: "Smoke Test",
    jobTitle: "Nhân viên",
    departmentName: "Công nghệ thông tin",
    workDate: "2026-10-01",
    shiftName: "Ca hành chính",
    scheduledStart: "08:00",
    scheduledEnd: "17:30",
    graceMinutes: 0,
    breakMinutes: 60,
    approvedLeave: false,
    leaveSession: null,
    leaveDescription: null,
    expectedStartAt: "2026-10-01T08:00:00",
    expectedEndAt: "2026-10-01T17:30:00",
    checkIn: "2026-10-01T08:05:00",
    checkOut: "2026-10-01T17:00:00",
    lastSeen: "2026-10-01T17:00:00",
    eventCount: 2,
    isProvisional: false,
    hasExplicitPunches: false,
    isFlexible: false,
    requiredMinutes: 0,
    workedMinutes: 535,
    lateMinutes: 5,
    earlyMinutes: 30,
    statusCode: "LATE_EARLY",
    source: "HANET",
  });
});

test("a full-day approved leave adds a scheduled attendance record without punches", () => {
  const records = projectAttendance(
    [],
    [
      {
        Id: 1,
        UserId: 42,
        ShiftName: "Ca hành chính",
        StartTime: "08:00:00",
        EndTime: "17:30:00",
        BreakMinutes: 60,
        GraceMinutes: 0,
        WorkDaysMask: 2,
        EffectiveFrom: "2026-01-01",
        EffectiveTo: null,
        StatusCode: "ACTIVE",
      },
    ],
    [
      {
        Id: 8,
        RequestCode: "NP00000008",
        UserId: 42,
        EmployeeCode: "HN-42",
        DisplayName: "Smoke Test",
        JobTitle: "Nhân viên",
        DepartmentName: "Công nghệ thông tin",
        LeaveType: "Phép năm",
        StartDate: "2026-10-05",
        EndDate: "2026-10-05",
        SessionCode: "FULL_DAY",
      },
    ],
    range("2026-10-05", "2026-10-05"),
    new Date("2026-10-05T18:00:00+07:00"),
  );

  assert.equal(records.length, 1);
  assert.equal(records[0].approvedLeave, true);
  assert.equal(records[0].leaveSession, "Cả ngày");
  assert.equal(records[0].leaveDescription, "Phép năm (NP00000008)");
  assert.equal(records[0].statusCode, "ON_LEAVE");
  assert.equal(records[0].eventCount, 0);
  assert.equal(records[0].source, "Nghỉ đã duyệt");
});
