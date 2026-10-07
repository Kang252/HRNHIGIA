const assert = require("node:assert/strict");
const test = require("node:test");

const {
  AttendanceService,
  attendancePeriodScopeFor,
  attendanceScopeFor,
  projectAttendance,
  resolveAttendancePeriod,
  resolveAttendanceRange,
} = require("../../dist/attendance/attendance.service.js");
const {
  GetAttendancePeriodDto,
} = require("../../dist/attendance/get-attendance-period.dto.js");
const { validate } = require("class-validator");

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

test("attendance period query scope retains caller, department, and company boundaries", () => {
  assert.deepEqual(attendancePeriodScopeFor(identity("EMPLOYEE")), {
    canSeeAll: false,
    isManager: false,
    actorId: 42,
    departmentId: 7,
  });
  assert.deepEqual(attendancePeriodScopeFor(identity("MANAGER")), {
    canSeeAll: false,
    isManager: true,
    actorId: 42,
    departmentId: 7,
  });
  assert.deepEqual(attendancePeriodScopeFor(identity("HR")), {
    canSeeAll: true,
    isManager: false,
    actorId: 42,
    departmentId: 7,
  });
});

test("attendance period defaults in Vietnam only when the query is omitted", async () => {
  assert.equal(
    resolveAttendancePeriod(undefined, new Date("2026-10-31T17:30:00.000Z")),
    "2026-11",
  );
  assert.equal(
    resolveAttendancePeriod("2026-02", new Date("2026-10-01T00:00:00Z")),
    "2026-02",
  );
  assert.throws(() => resolveAttendancePeriod(""), /Kỳ chấm công không hợp lệ/);
  assert.throws(
    () => resolveAttendancePeriod("2026-13"),
    /Kỳ chấm công không hợp lệ/,
  );

  const omitted = new GetAttendancePeriodDto();
  assert.equal((await validate(omitted)).length, 0);
  const malformed = Object.assign(new GetAttendancePeriodDto(), {
    period: "2026-1",
  });
  assert.ok((await validate(malformed)).length > 0);
});

test("attendance period query applies the role scope to every aggregate", async () => {
  const inputs = new Map();
  let queryText = "";
  const request = {
    input(name, _type, value) {
      inputs.set(name, value);
      return this;
    },
    async query(sqlText) {
      queryText = sqlText;
      return { recordset: [] };
    },
  };
  const service = new AttendanceService(
    { getPool: async () => ({ request: () => request }) },
    { assertReady: async () => undefined },
  );

  await service.getPeriod(identity("MANAGER"), "2026-10");

  assert.equal(inputs.get("CanSeeAll"), false);
  assert.equal(inputs.get("IsManager"), true);
  assert.equal(inputs.get("ActorId"), 42);
  assert.equal(inputs.get("DepartmentId"), 7);
  assert.match(
    queryText,
    /@CanSeeAll=1 OR x\.Id=@ActorId\s+OR \(@IsManager=1 AND x\.DepartmentId=@DepartmentId\)/,
  );
  const scopedAggregatePredicate =
    /@CanSeeAll=1 OR a\.Id=@ActorId\s+OR \(@IsManager=1 AND a\.DepartmentId=@DepartmentId\)/g;
  assert.equal([...queryText.matchAll(scopedAggregatePredicate)].length, 3);
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

test("schedule work-day bits follow Sunday=1 through Saturday=64", () => {
  const records = projectAttendance(
    [],
    [
      {
        Id: 1,
        UserId: 101,
        EmployeeCode: "HN-101",
        DisplayName: "Chủ nhật",
        JobTitle: null,
        DepartmentName: null,
        ShiftName: "Ca chủ nhật",
        StartTime: "08:00:00",
        EndTime: "17:30:00",
        BreakMinutes: 60,
        GraceMinutes: 0,
        WorkDaysMask: 1,
        EffectiveFrom: "2026-01-01",
        EffectiveTo: null,
        StatusCode: "ACTIVE",
      },
      {
        Id: 2,
        UserId: 102,
        EmployeeCode: "HN-102",
        DisplayName: "Thứ hai",
        JobTitle: null,
        DepartmentName: null,
        ShiftName: "Ca thứ hai",
        StartTime: "08:00:00",
        EndTime: "17:30:00",
        BreakMinutes: 60,
        GraceMinutes: 0,
        WorkDaysMask: 2,
        EffectiveFrom: "2026-01-01",
        EffectiveTo: null,
        StatusCode: "ACTIVE",
      },
      {
        Id: 3,
        UserId: 103,
        EmployeeCode: "HN-103",
        DisplayName: "Thứ sáu",
        JobTitle: null,
        DepartmentName: null,
        ShiftName: "Ca thứ sáu",
        StartTime: "08:00:00",
        EndTime: "17:30:00",
        BreakMinutes: 60,
        GraceMinutes: 0,
        WorkDaysMask: 32,
        EffectiveFrom: "2026-01-01",
        EffectiveTo: null,
        StatusCode: "ACTIVE",
      },
    ],
    [],
    range("2026-10-05", "2026-10-11"),
    new Date("2026-10-12T18:00:00+07:00"),
  );

  assert.deepEqual(
    records.map((record) => [record.userId, record.workDate]),
    [
      [101, "2026-10-11"],
      [103, "2026-10-09"],
      [102, "2026-10-05"],
    ],
  );
  assert.deepEqual(
    records.map((record) => record.statusCode),
    ["MISSING_CHECK", "MISSING_CHECK", "MISSING_CHECK"],
  );
});

test("a scheduled day without punches or leave is visible as missing check", () => {
  const records = projectAttendance(
    [],
    [
      {
        Id: 1,
        UserId: 42,
        EmployeeCode: "HN-42",
        DisplayName: "Smoke Test",
        JobTitle: "Nhân viên",
        DepartmentName: "Công nghệ thông tin",
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
    [],
    range("2026-10-05", "2026-10-05"),
    new Date("2026-10-05T18:00:00+07:00"),
  );

  assert.equal(records.length, 1);
  assert.equal(records[0].statusCode, "MISSING_CHECK");
  assert.equal(records[0].source, "Lịch làm việc");
  assert.equal(records[0].eventCount, 0);
  assert.equal(records[0].checkIn, null);
  assert.equal(records[0].checkOut, null);
});
