const assert = require("node:assert/strict");
const test = require("node:test");

const {
  toCommunicationListItem,
} = require("../../dist/communications/communications.service.js");

test("maps legacy PascalCase communication SQL rows to the public camelCase contract", () => {
  const submittedAt = new Date("2026-10-06T08:30:00.000Z");
  const publishedAt = new Date("2026-10-06T09:00:00.000Z");
  const item = toCommunicationListItem({
    Id: 47,
    AuthorUserId: 11,
    AuthorName: "Nguyễn Hoàng Khang",
    AuthorAvatarUrl: "/api/v1/users/11/avatar?v=1",
    AuthorJobTitle: "Nhân viên IT",
    Category: "Thông báo",
    ScopeCode: "ALL",
    DepartmentId: null,
    Title: "Kế hoạch tuần",
    Body: "Nội dung đã duyệt.",
    AttachmentName: null,
    AttachmentContentType: null,
    IsPinned: 1,
    StatusCode: "PUBLISHED",
    SubmittedAt: submittedAt,
    PublishedAt: publishedAt,
    LikeCount: 3,
    CommentCount: 2,
    LikedByCurrentUser: 0,
    IsMine: true,
  });

  assert.deepEqual(item, {
    id: 47,
    authorUserId: 11,
    authorName: "Nguyễn Hoàng Khang",
    authorAvatarUrl: "/api/v1/users/11/avatar?v=1",
    authorJobTitle: "Nhân viên IT",
    category: "Thông báo",
    scopeCode: "ALL",
    departmentId: null,
    title: "Kế hoạch tuần",
    body: "Nội dung đã duyệt.",
    attachmentName: null,
    attachmentContentType: null,
    isPinned: true,
    statusCode: "PUBLISHED",
    submittedAt,
    publishedAt,
    likeCount: 3,
    commentCount: 2,
    likedByCurrentUser: false,
    isMine: true,
  });
});
