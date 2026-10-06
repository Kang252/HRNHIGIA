import {
  CanActivate,
  ExecutionContext,
  ForbiddenException,
  Injectable,
  UnsupportedMediaTypeException,
} from "@nestjs/common";
import type { Request } from "express";

/**
 * Direct session login is intentionally browser-only.  Requiring the same
 * origin and a non-simple XMLHttpRequest header prevents a third-party HTML
 * form from replacing a user's session with an attacker's account.
 *
 * API clients that need service-to-service access must use the future signed
 * proxy bridge instead of this cookie-login route.
 */
@Injectable()
export class BrowserMutationGuard implements CanActivate {
  public canActivate(context: ExecutionContext): boolean {
    const request = context.switchToHttp().getRequest<Request>();
    const isLogin = request.originalUrl
      .split("?", 1)[0]
      ?.endsWith("/auth/login");

    if (!isSameOriginBrowserRequest(request)) {
      throw new ForbiddenException(
        "Yêu cầu phải được gửi từ giao diện NHIGIA cùng nguồn.",
      );
    }
    if (isLogin && !hasJsonContentType(request)) {
      throw new UnsupportedMediaTypeException(
        "Đăng nhập chỉ nhận dữ liệu application/json.",
      );
    }
    return true;
  }
}

export interface OriginCheckedRequest {
  protocol: string;
  get(name: string): string | undefined;
  header(name: string): string | undefined;
}

export function isSameOriginBrowserRequest(
  request: OriginCheckedRequest,
): boolean {
  const requestedWith = request.header("x-requested-with")?.trim();
  if (requestedWith?.toLowerCase() !== "xmlhttprequest") return false;

  const origin = request.header("origin")?.trim();
  const host = request.get("host")?.trim();
  if (!origin || !host) return false;

  try {
    return new URL(origin).origin === `${request.protocol}://${host}`;
  } catch {
    return false;
  }
}

export function hasJsonContentType(request: OriginCheckedRequest): boolean {
  return (
    request
      .header("content-type")
      ?.toLocaleLowerCase("en-US")
      .startsWith("application/json") ?? false
  );
}
