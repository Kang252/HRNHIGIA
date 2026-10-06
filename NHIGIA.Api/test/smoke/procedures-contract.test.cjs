const assert = require("node:assert/strict");
const test = require("node:test");

const {
  procedureScope,
  toLeaveProcedureDetail,
  toLeaveProcedureListItem,
  toWorkProcedureDetail,
  toWorkProcedureListItem,
} = require("../../dist/procedures/procedures.service.js");

function identity(roleCode) {
  return {
    userId: 42,
    username: "smoke-test",
    displayName: "Smoke Test",
    roleCode,
    departmentId: 7,
  };
}

test("procedure visibility uses self, department, and company scopes", () => {
  assert.deepEqual(procedureScope(identity("EMPLOYEE")), {
    canSeeAll: false,
    isManager: false,
  });
  assert.deepEqual(procedureScope(identity("MANAGER")), {
    canSeeAll: false,
    isManager: true,
  });
  for (const roleCode of ["HR", "DIRECTOR", "ADMIN"]) {
    assert.deepEqual(procedureScope(identity(roleCode)), {
      canSeeAll: true,
      isManager: false,
    });
  }
});

test("leave mapping keeps the owner, dates, attachment bit, and approval notes", () => {
  const createdAt = new Date("2026-10-06T08:30:00.000Z");
  const updatedAt = new Date("2026-10-06T09:45:00.000Z");
  const startDate = new Date("2026-10-15T00:00:00.000Z");
  const endDate = new Date("2026-10-16T00:00:00.000Z");
  const row = {
    Id: 31,
    RequestCode: "NP00000031",
    UserId: 42,
    OwnerName: "Nguyễn Hoàng Khang",
    DepartmentName: "Công nghệ thông tin",
    LeaveType: "Nghỉ phép năm",
    StartDate: startDate,
    EndDate: endDate,
    StatusCode: "PENDING_MANAGER",
    Summary: "Việc gia đình",
    HasAttachment: 1,
    CreatedAt: createdAt,
    UpdatedAt: updatedAt,
    SessionCode: "FULL_DAY",
    HandoverTo: "Nguyễn Văn A",
    Reason: "Việc gia đình",
    AttachmentName: "don-nghi.pdf",
    ManagerNote: "Đang xem xét",
    HrNote: null,
  };

  assert.deepEqual(toLeaveProcedureListItem(row, 42), {
    id: 31,
    source: "leave",
    code: "NP00000031",
    kind: "leave",
    requestType: "Nghỉ phép năm",
    title: "Nghỉ phép năm",
    summary: "Việc gia đình",
    ownerUserId: 42,
    ownerName: "Nguyễn Hoàng Khang",
    departmentName: "Công nghệ thông tin",
    startAt: startDate,
    endAt: endDate,
    dueDate: null,
    status: "PENDING_MANAGER",
    hasAttachment: true,
    createdAt,
    updatedAt,
    isMine: true,
  });

  assert.deepEqual(toLeaveProcedureDetail(row, 99), {
    ...toLeaveProcedureListItem(row, 99),
    source: "leave",
    kind: "leave",
    sessionCode: "FULL_DAY",
    handoverTo: "Nguyễn Văn A",
    reason: "Việc gia đình",
    attachmentName: "don-nghi.pdf",
    managerNote: "Đang xem xét",
    hrNote: null,
  });
});

test("work mapping uses the established procedure code and does not fabricate attachments", () => {
  const createdAt = new Date("2026-10-06T08:30:00.000Z");
  const dueDate = new Date("2026-10-11T00:00:00.000Z");
  const row = {
    Id: 8,
    Kind: "business-trip",
    Title: "Công tác Tây Ninh",
    OwnerUserId: 18,
    OwnerName: "Trần Minh Anh",
    DepartmentName: "Kinh doanh",
    DueDate: dueDate,
    StartAt: null,
    EndAt: null,
    Status: null,
    Summary: "Gặp khách hàng và khảo sát thị trường.",
    CreatedAt: createdAt,
    UpdatedAt: null,
    Category: "Khách hàng",
    Description: "Gặp khách hàng và khảo sát thị trường.",
    Location: "TP. Hồ Chí Minh",
    Destination: "Tây Ninh",
    Priority: null,
    LastActionNote: "Chờ Trưởng phòng duyệt",
  };

  assert.deepEqual(toWorkProcedureListItem(row, 42), {
    id: 8,
    source: "work",
    code: "CT00000008",
    kind: "business-trip",
    requestType: "Công tác",
    title: "Công tác Tây Ninh",
    summary: "Gặp khách hàng và khảo sát thị trường.",
    ownerUserId: 18,
    ownerName: "Trần Minh Anh",
    departmentName: "Kinh doanh",
    startAt: dueDate,
    endAt: dueDate,
    dueDate,
    status: "OPEN",
    hasAttachment: false,
    createdAt,
    updatedAt: null,
    isMine: false,
  });

  assert.deepEqual(toWorkProcedureDetail(row, 42), {
    ...toWorkProcedureListItem(row, 42),
    source: "work",
    kind: "business-trip",
    category: "Khách hàng",
    description: "Gặp khách hàng và khảo sát thị trường.",
    location: "TP. Hồ Chí Minh",
    destination: "Tây Ninh",
    priority: "NORMAL",
    lastActionNote: "Chờ Trưởng phòng duyệt",
  });
});
