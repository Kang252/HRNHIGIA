import {
  Body,
  Controller,
  Delete,
  Get,
  HttpCode,
  HttpStatus,
  Param,
  ParseIntPipe,
  Patch,
  Post,
  Query,
  Req,
  UseGuards,
} from "@nestjs/common";
import type { Request } from "express";
import { BrowserMutationGuard } from "../auth/browser-mutation.guard";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { CurrentIdentity } from "../identity/identity.decorator";
import { CreateCommunicationCommentDto } from "./create-communication-comment.dto";
import { CreateCommunicationDto } from "./create-communication.dto";
import { ListCommunicationCommentsDto } from "./list-communication-comments.dto";
import { ListCommunicationsDto } from "./list-communications.dto";
import { ModerateCommunicationDto } from "./moderate-communication.dto";
import {
  CommunicationsService,
  type CommunicationCommentItem,
  type CommunicationListItem,
  type CommunicationModerationResult,
  type CommunicationReactionResult,
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

  @Post()
  @UseGuards(BrowserMutationGuard)
  public async create(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Body() input: CreateCommunicationDto,
    @Req() request: Request,
  ): Promise<{ success: true; data: CommunicationListItem }> {
    return {
      success: true,
      data: await this.communications.create(identity, input, request.ip),
    };
  }

  @Patch(":id")
  @UseGuards(BrowserMutationGuard)
  public async update(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
    @Body() input: CreateCommunicationDto,
    @Req() request: Request,
  ): Promise<{ success: true; data: CommunicationListItem }> {
    return {
      success: true,
      data: await this.communications.update(identity, id, input, request.ip),
    };
  }

  @Delete(":id")
  @UseGuards(BrowserMutationGuard)
  public async remove(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
    @Req() request: Request,
  ): Promise<{ success: true; data: { id: number } }> {
    await this.communications.remove(identity, id, request.ip);
    return { success: true, data: { id } };
  }

  @Post(":id/moderation")
  @UseGuards(BrowserMutationGuard)
  public async moderate(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
    @Body() input: ModerateCommunicationDto,
    @Req() request: Request,
  ): Promise<{ success: true; data: CommunicationModerationResult }> {
    return {
      success: true,
      data: await this.communications.moderate(identity, id, input, request.ip),
    };
  }

  @Post(":id/like")
  @HttpCode(HttpStatus.OK)
  @UseGuards(BrowserMutationGuard)
  public async toggleLike(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
  ): Promise<{ success: true; data: CommunicationReactionResult }> {
    return {
      success: true,
      data: await this.communications.toggleLike(identity, id),
    };
  }

  @Get(":id/comments")
  public async listComments(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
    @Query() query: ListCommunicationCommentsDto,
  ): Promise<{ success: true; data: CommunicationCommentItem[] }> {
    return {
      success: true,
      data: await this.communications.listComments(identity, id, query),
    };
  }

  @Post(":id/comments")
  @UseGuards(BrowserMutationGuard)
  public async addComment(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("id", new ParseIntPipe({ errorHttpStatusCode: 404 })) id: number,
    @Body() input: CreateCommunicationCommentDto,
    @Req() request: Request,
  ): Promise<{ success: true; data: CommunicationCommentItem }> {
    return {
      success: true,
      data: await this.communications.addComment(
        identity,
        id,
        input,
        request.ip,
      ),
    };
  }
}
