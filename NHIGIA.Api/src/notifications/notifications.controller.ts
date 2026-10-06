import {
  Controller,
  Get,
  HttpCode,
  Param,
  ParseIntPipe,
  Post,
  Query,
  UseGuards,
} from "@nestjs/common";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import { BrowserMutationGuard } from "../auth/browser-mutation.guard";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { CurrentIdentity } from "../identity/identity.decorator";
import { ListNotificationsDto } from "./list-notifications.dto";
import {
  NotificationsService,
  type NotificationList,
  type NotificationReadResult,
} from "./notifications.service";

@Controller("notifications")
@UseGuards(ApiIdentityGuard)
export class NotificationsController {
  public constructor(private readonly notifications: NotificationsService) {}

  @Get()
  public async list(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: ListNotificationsDto,
  ): Promise<{ success: true; data: NotificationList }> {
    return {
      success: true,
      data: await this.notifications.list(identity, query.take),
    };
  }

  @Get("unread-count")
  public async unreadCount(
    @CurrentIdentity() identity: AuthenticatedIdentity,
  ): Promise<{ success: true; data: { count: number } }> {
    return {
      success: true,
      data: { count: await this.notifications.unreadCount(identity) },
    };
  }

  @Post("read-all")
  @HttpCode(200)
  @UseGuards(BrowserMutationGuard)
  public async readAll(
    @CurrentIdentity() identity: AuthenticatedIdentity,
  ): Promise<{ success: true; data: { updatedCount: number } }> {
    return {
      success: true,
      data: { updatedCount: await this.notifications.readAll(identity) },
    };
  }

  @Post(":id/read")
  @HttpCode(200)
  @UseGuards(BrowserMutationGuard)
  public async read(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
  ): Promise<{ success: true; data: NotificationReadResult }> {
    return {
      success: true,
      data: await this.notifications.read(identity, id),
    };
  }
}
