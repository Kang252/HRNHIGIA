const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const test = require("node:test");

const { configureClientApp } = require("../../dist/main.js");

function createApp() {
  const routes = [];
  const middleware = [];
  const expressApp = {
    get(pathname, handler) {
      routes.push({ pathname, handler });
    },
    use(pathname, handler) {
      middleware.push({ pathname, handler });
    },
  };

  return {
    routes,
    middleware,
    nestApp: {
      getHttpAdapter() {
        return { getInstance: () => expressApp };
      },
    },
  };
}

function response() {
  const headers = new Map();
  return {
    headers,
    setHeader(name, value) {
      headers.set(name.toLowerCase(), value);
    },
    redirect(status, location) {
      this.status = status;
      this.location = location;
    },
  };
}

test("registers browser-only root and legacy login entrypoints for the React shell", (t) => {
  const clientRoot = fs.mkdtempSync(path.join(os.tmpdir(), "nhigia-client-"));
  t.after(() => fs.rmSync(clientRoot, { recursive: true, force: true }));
  fs.writeFileSync(path.join(clientRoot, "index.html"), "<!doctype html>");

  const app = createApp();
  configureClientApp(app.nestApp, clientRoot);

  assert.deepEqual(
    app.routes.map(({ pathname }) => pathname),
    ["/", "/Account/Login"],
  );
  assert.equal(app.middleware.length, 2);
  assert.deepEqual(
    app.middleware.map(({ pathname }) => pathname),
    ["/modern-ui", "/modern-ui"],
  );

  for (const route of app.routes) {
    const result = response();
    route.handler({}, result);
    assert.equal(result.status, 302);
    assert.equal(result.location, "/modern-ui/");
    assert.equal(result.headers.get("cache-control"), "no-store");
  }
});
