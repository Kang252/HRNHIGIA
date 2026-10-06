import {
  Body,
  Controller,
  Get,
  HttpCode,
  HttpStatus,
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
import { AttendancePeriodDto } from "./attendance-period.dto";
import {
  AttendanceService,
  type AttendanceListData,
  type AttendancePeriodData,
} from "./attendance.service";
import { ListAttendanceDto } from "./list-attendance.dto";

@Controller("attendance")
@UseGuards(ApiIdentityGuard)
export class AttendanceController {
  public constructor(private readonly attendance: AttendanceService) {}

  /**
   * Lists the caller's visible attendance rows for a date range.  Employees
   * receive only their own rows; a manager gets their department; HR,
   * director, and admin roles get company scope.
   */
  @Get()
  public async list(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: ListAttendanceDto,
  ): Promise<{ success: true; data: AttendanceListData }> {
    return { success: true, data: await this.attendance.list(identity, query) };
  }

  @Get("period")
  public async getPeriod(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: AttendancePeriodDto,
  ): Promise<{ success: true; data: AttendancePeriodData }> {
    return {
      success: true,
      data: await this.attendance.getPeriod(identity, query.period),
    };
  }

  /**
   * Mirrors the established C# employee-confirmation flow.  It only confirms
   * the authenticated user's own period and never locks or changes events.
   */
  @Post("period/confirm")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async confirmPeriod(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Body() body: AttendancePeriodDto,
    @Req() request: Request,
  ): Promise<{
    success: true;
    data: { period: string; statusCode: "SUBMITTED" };
  }> {
    await this.attendance.confirmPeriod(identity, body.period, request.ip);
    return {
      success: true,
      data: { period: body.period, statusCode: "SUBMITTED" },
    };
  }
}
