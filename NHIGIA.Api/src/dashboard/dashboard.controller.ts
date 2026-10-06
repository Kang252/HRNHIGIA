import { Controller, Get, UseGuards } from "@nestjs/common";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import { CurrentIdentity } from "../identity/identity.decorator";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { DashboardService, type DashboardData } from "./dashboard.service";

@Controller("dashboard")
@UseGuards(ApiIdentityGuard)
export class DashboardController {
  public constructor(private readonly dashboard: DashboardService) {}

  @Get()
  public async getDashboard(
    @CurrentIdentity() identity: AuthenticatedIdentity,
  ): Promise<{ success: true; data: DashboardData }> {
    return { success: true, data: await this.dashboard.getDashboard(identity) };
  }
}
