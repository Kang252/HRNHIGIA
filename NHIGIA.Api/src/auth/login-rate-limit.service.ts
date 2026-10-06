import { HttpException, HttpStatus, Injectable } from "@nestjs/common";
import { createHash } from "node:crypto";
import { RuntimeConfigService } from "../config/runtime-config.service";

interface LoginAttemptWindow {
  startedAt: number;
  attempts: number;
  lastSeenAt: number;
}

/**
 * Bounds password checks for a single API process without retaining raw IP
 * addresses in memory. Production scale-out should put this concern in a
 * shared gateway or Redis, but this protects the single Render service that
 * the migration blueprint provisions.
 */
@Injectable()
export class LoginRateLimitService {
  private readonly attempts = new Map<string, LoginAttemptWindow>();
  private nextCleanupAt = 0;

  public constructor(private readonly config: RuntimeConfigService) {}

  public consume(ipAddress: string | null): void {
    const now = Date.now();
    this.prune(now);
    const key = this.keyFor(ipAddress);
    const existing = this.attempts.get(key);
    const windowMs = this.config.loginRateLimitWindowSeconds * 1000;

    if (!existing || now - existing.startedAt >= windowMs) {
      this.attempts.set(key, { startedAt: now, attempts: 1, lastSeenAt: now });
      return;
    }

    if (existing.attempts >= this.config.loginRateLimitMaxAttempts) {
      existing.lastSeenAt = now;
      throw new HttpException(
        "Có quá nhiều lần đăng nhập không thành công. Vui lòng thử lại sau ít phút.",
        HttpStatus.TOO_MANY_REQUESTS,
      );
    }

    existing.attempts += 1;
    existing.lastSeenAt = now;
  }

  public reset(ipAddress: string | null): void {
    this.attempts.delete(this.keyFor(ipAddress));
  }

  private keyFor(ipAddress: string | null): string {
    return createHash("sha256")
      .update(ipAddress || "unknown", "utf8")
      .digest("base64url");
  }

  private prune(now: number): void {
    if (
      now < this.nextCleanupAt &&
      this.attempts.size < this.config.loginRateLimitMaxTrackedKeys
    ) {
      return;
    }

    const oldestAllowed = now - this.config.loginRateLimitWindowSeconds * 1000;
    for (const [key, attempt] of this.attempts) {
      if (attempt.lastSeenAt < oldestAllowed) this.attempts.delete(key);
    }

    while (this.attempts.size >= this.config.loginRateLimitMaxTrackedKeys) {
      let oldestKey: string | undefined;
      let oldestSeenAt = Number.POSITIVE_INFINITY;
      for (const [key, attempt] of this.attempts) {
        if (attempt.lastSeenAt < oldestSeenAt) {
          oldestKey = key;
          oldestSeenAt = attempt.lastSeenAt;
        }
      }
      if (!oldestKey) break;
      this.attempts.delete(oldestKey);
    }

    this.nextCleanupAt =
      now + Math.min(60_000, this.config.loginRateLimitWindowSeconds * 1000);
  }
}
