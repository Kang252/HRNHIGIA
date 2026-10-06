import { Controller, Get, HttpStatus, Req, Res } from "@nestjs/common";
import type { Request, Response } from "express";
import { HealthService } from "./health.service";

@Controller("health")
export class HealthController {
  public constructor(private readonly health: HealthService) {}

  @Get()
  public async getHealth(
    @Req() request: Request,
    @Res() response: Response,
  ): Promise<void> {
    const result = await this.health.status();
    response
      .status(
        result.status === "ok" ? HttpStatus.OK : HttpStatus.SERVICE_UNAVAILABLE,
      )
      .json({
        success: result.status === "ok",
        requestId: request.requestId,
        ...result,
      });
  }
}
