const assert = require("node:assert/strict");
const test = require("node:test");

const {
  LoginRateLimitService,
} = require("../../dist/auth/login-rate-limit.service.js");

function createRateLimiter(maxAttempts = 2) {
  return new LoginRateLimitService({
    loginRateLimitWindowSeconds: 60,
    loginRateLimitMaxAttempts: maxAttempts,
    loginRateLimitMaxTrackedKeys: 100,
  });
}

test("login rate limiter blocks the attempt after the configured threshold", () => {
  const limiter = createRateLimiter(2);
  const ipAddress = "203.0.113.24";

  assert.doesNotThrow(() => limiter.consume(ipAddress));
  assert.doesNotThrow(() => limiter.consume(ipAddress));
  assert.throws(
    () => limiter.consume(ipAddress),
    (error) => error?.getStatus?.() === 429,
  );
});

test("login rate limiter clears an IP window after a successful-login reset", () => {
  const limiter = createRateLimiter(1);
  const ipAddress = "198.51.100.17";

  limiter.consume(ipAddress);
  assert.throws(() => limiter.consume(ipAddress));

  limiter.reset(ipAddress);
  assert.doesNotThrow(() => limiter.consume(ipAddress));
  assert.throws(() => limiter.consume(ipAddress));
});
