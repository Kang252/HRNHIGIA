import { Controller, Get, Query, UseGuards } from "@nestjs/common";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import { CurrentIdentity } from "../identity/identity.decorator";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { ListCommunicationsDto } from "./list-communications.dto";
import {
  CommunicationsService,
  type CommunicationListItem,
} from "./communications.service";

@Controller("communications")
@UseGuards(ApiIdentityGuard)
export class CommunicationsController {
  public constructor(private readonly communications: CommunicationsService) {}

  @Get()
  public async list(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: ListCommunicationsDto,
  ): Promise<{ success: true; data: CommunicationListItem[] }> {
    return {
      success: true,
      data: await this.communications.list(identity, query),
    };
  }
}
