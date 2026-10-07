import {
  BadRequestException,
  Body,
  Controller,
  Get,
  HttpCode,
  HttpStatus,
  Param,
  Post,
  Query,
  Req,
  UseGuards,
} from "@nestjs/common";
import type { Request } from "express";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import { BrowserMutationGuard } from "../auth/browser-mutation.guard";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { CurrentIdentity } from "../identity/identity.decorator";
import { CreateRemotePlanDto } from "./create-remote-plan.dto";
import {
  RemoteAttendanceService,
  type RemoteAttendanceState,
} from "./remote-attendance.service";
import { RemoteAttendanceRangeDto } from "./remote-attendance-range.dto";
import { ReviewRemoteItemDto } from "./review-remote-item.dto";

@Controller("remote-attendance")
@UseGuards(ApiIdentityGuard)
export class RemoteAttendanceController {
  public constructor(
    private readonly remoteAttendance: RemoteAttendanceService,
  ) {}

  /**
   * Returns plans and reviewable evidence only within the C# remote-attendance
   * scope: self, a manager's employee reports, or the company for HR/director/admin.
   */
  @Get()
  public async getState(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: RemoteAttendanceRangeDto,
  ): Promise<{ success: true; data: RemoteAttendanceState }> {
    return {
      success: true,
      data: await this.remoteAttendance.getState(identity, query),
    };
  }

  @Post("plans")
  @HttpCode(HttpStatus.CREATED)
  @UseGuards(BrowserMutationGuard)
  public async createPlan(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Body() body: CreateRemotePlanDto,
    @Req() request: Request,
  ): Promise<{ success: true; data: { id: number; statusCode: "PENDING" } }> {
    return {
      success: true,
      data: await this.remoteAttendance.createPlan(identity, body, request.ip),
    };
  }

  @Post("plans/:id/review")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async reviewPlan(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id") rawId: string,
    @Body() body: ReviewRemoteItemDto,
    @Req() request: Request,
  ): Promise<{
    success: true;
    data: { id: number; statusCode: "APPROVED" | "REJECTED" };
  }> {
    return {
      success: true,
      data: await this.remoteAttendance.reviewPlan(
        identity,
        parseRemoteId(rawId),
        body,
        request.ip,
      ),
    };
  }

  @Post("plans/:id/cancel")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async cancelPlan(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id") rawId: string,
    @Req() request: Request,
  ): Promise<{ success: true; data: { id: number; statusCode: "CANCELLED" } }> {
    return {
      success: true,
      data: await this.remoteAttendance.cancelPlan(
        identity,
        parseRemoteId(rawId),
        request.ip,
      ),
    };
  }

  /** A human reviewer decides the punch; no automatic face-based approval. */
  @Post("punches/:id/review")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async reviewPunch(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id") rawId: string,
    @Body() body: ReviewRemoteItemDto,
    @Req() request: Request,
  ): Promise<{
    success: true;
    data: { id: number; statusCode: "APPROVED" | "REJECTED" };
  }> {
    return {
      success: true,
      data: await this.remoteAttendance.reviewPunch(
        identity,
        parseRemoteId(rawId),
        body,
        request.ip,
      ),
    };
  }
}

function parseRemoteId(value: string): number {
  if (!/^[1-9]\d*$/.test(value)) {
    throw new BadRequestException("Mã đăng ký hoặc lượt chấm không hợp lệ.");
  }
  const id = Number(value);
  if (!Number.isSafeInteger(id)) {
    throw new BadRequestException("Mã đăng ký hoặc lượt chấm không hợp lệ.");
  }
  return id;
}
