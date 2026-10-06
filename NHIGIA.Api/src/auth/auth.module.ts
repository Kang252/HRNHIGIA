import { Module } from "@nestjs/common";
import { AuthController } from "./auth.controller";
import { AuthService } from "./auth.service";
import { ApiIdentityGuard } from "./session-identity.guard";
import { ProxyIdentityGuard } from "../identity/proxy-identity.guard";
import { LoginRateLimitService } from "./login-rate-limit.service";
import { BrowserMutationGuard } from "./browser-mutation.guard";

@Module({
  controllers: [AuthController],
  providers: [
    AuthService,
    ProxyIdentityGuard,
    ApiIdentityGuard,
    LoginRateLimitService,
    BrowserMutationGuard,
  ],
  exports: [AuthService, ProxyIdentityGuard, ApiIdentityGuard],
})
export class AuthModule {}
