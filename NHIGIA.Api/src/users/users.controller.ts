import {
  Controller,
  Get,
  NotFoundException,
  Param,
  ParseIntPipe,
  Query,
  Res,
  UseGuards,
} from "@nestjs/common";
import type { Response } from "express";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { CurrentIdentity } from "../identity/identity.decorator";
import { ListUsersDto } from "./list-users.dto";
import { UsersService, type EmployeeDirectoryItem } from "./users.service";

@Controller("users")
@UseGuards(ApiIdentityGuard)
export class UsersController {
  public constructor(private readonly users: UsersService) {}

  @Get()
  public async list(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: ListUsersDto,
  ): Promise<{ success: true; data: EmployeeDirectoryItem[] }> {
    return { success: true, data: await this.users.list(identity, query) };
  }

  @Get(":id/avatar")
  public async getAvatar(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) userId: number,
    @Res() response: Response,
  ): Promise<void> {
    const avatar = await this.users.findVisibleAvatar(identity, userId);
    if (!avatar) throw new NotFoundException("Không tìm thấy ảnh đại diện.");
    response
      .setHeader("Cache-Control", "private, max-age=86400")
      .type(avatar.contentType)
      .send(avatar.content);
  }

  @Get(":id")
  public async getById(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) userId: number,
  ): Promise<{ success: true; data: EmployeeDirectoryItem }> {
    const user = await this.users.findVisibleById(identity, userId);
    // The same response covers a missing account and an account outside the
    // caller's directory scope so this endpoint does not reveal its existence.
    if (!user) throw new NotFoundException("Không tìm thấy nhân viên.");
    return { success: true, data: user };
  }
}
