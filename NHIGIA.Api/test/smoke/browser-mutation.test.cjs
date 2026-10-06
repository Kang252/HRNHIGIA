const assert = require("node:assert/strict");
const test = require("node:test");

const {
  hasJsonContentType,
  isSameOriginBrowserRequest,
} = require("../../dist/auth/browser-mutation.guard.js");

function request({
  protocol = "https",
  host = "api.nhigia.test",
  origin = "https://api.nhigia.test",
  requestedWith = "XMLHttpRequest",
  contentType = "application/json",
} = {}) {
  return {
    protocol,
    get(name) {
      return name.toLowerCase() === "host" ? host : undefined;
    },
    header(name) {
      switch (name.toLowerCase()) {
        case "origin":
          return origin;
        case "x-requested-with":
          return requestedWith;
        case "content-type":
          return contentType;
        default:
          return undefined;
      }
    },
  };
}

test("accepts a JSON XMLHttpRequest from the exact served origin", () => {
  const candidate = request({ contentType: "application/json; charset=utf-8" });
  assert.equal(isSameOriginBrowserRequest(candidate), true);
  assert.equal(hasJsonContentType(candidate), true);
});

test("rejects a cross-origin or simple-form request before login can create a session", () => {
  assert.equal(
    isSameOriginBrowserRequest(request({ origin: "https://attacker.example" })),
    false,
  );
  assert.equal(
    isSameOriginBrowserRequest(request({ requestedWith: "" })),
    false,
  );
  assert.equal(
    hasJsonContentType(request({ contentType: "application/x-www-form-urlencoded" })),
    false,
  );
});
