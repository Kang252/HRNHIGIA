import {
  CanActivate,
  ExecutionContext,
  Injectable,
  ServiceUnavailableException,
  UnauthorizedException,
} from "@nestjs/common";
import { createHmac, timingSafeEqual } from "node:crypto";
import type { Request } from "express";
import { RuntimeConfigService } from "../config/runtime-config.service";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";

const roleCodes = new Set<AuthenticatedIdentity["roleCode"]>([
  "ADMIN",
  "HR",
  "DIRECTOR",
  "MANAGER",
  "EMPLOYEE",
]);

declare global {
  namespace Express {
    interface Request {
      identity?: AuthenticatedIdentity;
    }
  }
}

@Injectable()
export class ProxyIdentityGuard implements CanActivate {
  public constructor(private readonly config: RuntimeConfigService) {}

  public canActivate(context: ExecutionContext): boolean {
    if (this.config.authMode !== "proxy-hmac") {
      throw new ServiceUnavailableException(
        "The NHIGIA.Api authentication bridge is not configured.",
      );
    }

    const request = context.switchToHttp().getRequest<Request>();
    const identity = this.readIdentity(request);
    const timestamp = request.header("x-nhigia-auth-timestamp") ?? "";
    const signature = request.header("x-nhigia-auth-signature") ?? "";
    const timestampNumber = Number(timestamp);
    const maxSkewMs = this.config.authProxyMaxSkewSeconds * 1000;

    if (
      !Number.isSafeInteger(timestampNumber) ||
      Math.abs(Date.now() - timestampNumber * 1000) > maxSkewMs
    ) {
      throw new UnauthorizedException(
        "The proxied identity timestamp is invalid or expired.",
      );
    }

    const expected = createHmac(
      "sha256",
      this.config.authProxySharedSecret ?? "",
    )
      .update(this.canonicalRequest(request, identity, timestamp), "utf8")
      .digest("hex");
    if (!sameSecret(expected, signature)) {
      throw new UnauthorizedException(
        "The proxied identity signature is invalid.",
      );
    }

    request.identity = identity;
    return true;
  }

  private readIdentity(request: Request): AuthenticatedIdentity {
    const userId = Number(request.header("x-nhigia-user-id"));
    const username = (request.header("x-nhigia-username") ?? "").trim();
    const roleCode = (request.header("x-nhigia-role-code") ?? "")
      .trim()
      .toUpperCase() as AuthenticatedIdentity["roleCode"];
    const departmentHeader = request.header("x-nhigia-department-id");
    const departmentId = departmentHeader
      ? Number(departmentHeader)
      : undefined;

    if (
      !Number.isSafeInteger(userId) ||
      userId <= 0 ||
      !/^[A-Za-z0-9._@-]{1,100}$/.test(username) ||
      !roleCodes.has(roleCode)
    ) {
      throw new UnauthorizedException(
        "The proxied identity headers are incomplete or invalid.",
      );
    }
    if (
      departmentId !== undefined &&
      (!Number.isSafeInteger(departmentId) || departmentId <= 0)
    ) {
      throw new UnauthorizedException(
        "The proxied department header is invalid.",
      );
    }
    return {
      userId,
      username,
      roleCode,
      ...(departmentId ? { departmentId } : {}),
    };
  }

  private canonicalRequest(
    request: Request,
    identity: AuthenticatedIdentity,
    timestamp: string,
  ): string {
    const path = request.originalUrl.split("?")[0] ?? request.path;
    return [
      timestamp,
      request.method.toUpperCase(),
      path,
      identity.userId.toString(),
      identity.username,
      identity.roleCode,
      identity.departmentId?.toString() ?? "",
    ].join("\n");
  }
}

function sameSecret(expected: string, supplied: string): boolean {
  const expectedBytes = Buffer.from(expected, "utf8");
  const suppliedBytes = Buffer.from(supplied, "utf8");
  return (
    expectedBytes.length === suppliedBytes.length &&
    timingSafeEqual(expectedBytes, suppliedBytes)
  );
}
