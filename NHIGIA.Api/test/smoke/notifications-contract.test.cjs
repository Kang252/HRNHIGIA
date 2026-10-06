const assert = require("node:assert/strict");
const test = require("node:test");

const {
  safeLocalLink,
  toNotificationItem,
} = require("../../dist/notifications/notifications.service.js");

test("maps notification rows and only exposes safe local links", () => {
  const createdAt = new Date("2026-10-06T08:30:00.000Z");
  const item = toNotificationItem({
    Id: 47,
    Title: "Đơn nghỉ phép đã duyệt",
    Message: "Đơn của bạn đã được duyệt.",
    LinkUrl: "/Home/LeaveRequests",
    IsRead: 0,
    CreatedAt: createdAt,
  });

  assert.deepEqual(item, {
    id: 47,
    title: "Đơn nghỉ phép đã duyệt",
    message: "Đơn của bạn đã được duyệt.",
    linkUrl: "/Home/LeaveRequests",
    isRead: false,
    createdAt,
  });
});

test("never returns external or browser-ambiguous notification links", () => {
  assert.equal(safeLocalLink("https://example.test"), null);
  assert.equal(safeLocalLink("//example.test"), null);
  assert.equal(safeLocalLink("/\\example.test"), null);
  assert.equal(safeLocalLink("/%2F%2Fexample.test"), null);
  assert.equal(safeLocalLink("/%5Cexample.test"), null);
  assert.equal(safeLocalLink("/Work\r\nLocation: https://example.test"), null);
  assert.equal(safeLocalLink("/Work?kind=helpdesk"), "/Work?kind=helpdesk");
});
