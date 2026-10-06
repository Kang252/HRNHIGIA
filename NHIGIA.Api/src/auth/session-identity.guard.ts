import {
  CanActivate,
  ExecutionContext,
  Injectable,
  ServiceUnavailableException,
  UnauthorizedException,
} from "@nestjs/common";
import type { Request } from "express";
import { AuthService } from "./auth.service";
import { RuntimeConfigService } from "../config/runtime-config.service";
import { ProxyIdentityGuard } from "../identity/proxy-identity.guard";

@Injectable()
export class ApiIdentityGuard implements CanActivate {
  public constructor(
    private readonly config: RuntimeConfigService,
    private readonly auth: AuthService,
    private readonly proxyIdentity: ProxyIdentityGuard,
  ) {}

  public async canActivate(context: ExecutionContext): Promise<boolean> {
    if (this.config.authMode === "proxy-hmac")
      return this.proxyIdentity.canActivate(context);
    if (this.config.authMode !== "session") {
      throw new ServiceUnavailableException(
        "Authentication is not configured for NHIGIA.Api.",
      );
    }

    const request = context.switchToHttp().getRequest<Request>();
    const identity = await this.auth.findIdentity(
      this.auth.tokenFromRequest(request),
      request,
    );
    if (!identity)
      throw new UnauthorizedException(
        "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.",
      );
    request.identity = identity;
    return true;
  }
}
