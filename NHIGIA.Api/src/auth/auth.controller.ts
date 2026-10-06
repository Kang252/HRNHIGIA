import {
  Body,
  Controller,
  Get,
  HttpCode,
  HttpStatus,
  Post,
  Req,
  Res,
  ServiceUnavailableException,
  UseGuards,
} from "@nestjs/common";
import type { Request, Response } from "express";
import { AuthService } from "./auth.service";
import { LoginDto } from "./login.dto";
import { RuntimeConfigService } from "../config/runtime-config.service";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { ApiIdentityGuard } from "./session-identity.guard";
import { CurrentIdentity } from "../identity/identity.decorator";
import { BrowserMutationGuard } from "./browser-mutation.guard";

@Controller("auth")
export class AuthController {
  public constructor(
    private readonly auth: AuthService,
    private readonly config: RuntimeConfigService,
  ) {}

  @Post("login")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async login(
    @Body() input: LoginDto,
    @Req() request: Request,
    @Res({ passthrough: true }) response: Response,
  ): Promise<{ success: true; data: { user: AuthenticatedIdentity } }> {
    const result = await this.auth.login(input, request);
    const cookie = this.auth.sessionCookie(result.token, result.maxAgeMs);
    response.cookie(cookie.name, cookie.value, cookie);
    return { success: true, data: { user: result.identity } };
  }

  @Post("logout")
  @HttpCode(HttpStatus.NO_CONTENT)
  @UseGuards(BrowserMutationGuard)
  public async logout(
    @Req() request: Request,
    @Res({ passthrough: true }) response: Response,
  ): Promise<void> {
    if (this.config.authMode === "session") {
      await this.auth.logout(this.auth.tokenFromRequest(request));
    } else if (this.config.authMode === "disabled") {
      throw new ServiceUnavailableException(
        "Authentication is not configured for NHIGIA.Api.",
      );
    }
    const cookie = this.auth.clearSessionCookie();
    response.clearCookie(cookie.name, cookie);
  }

  @Get("me")
  @UseGuards(ApiIdentityGuard)
  public getCurrentIdentity(
    @CurrentIdentity() identity: AuthenticatedIdentity,
  ): { success: true; data: AuthenticatedIdentity } {
    return { success: true, data: identity };
  }
}
