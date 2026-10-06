import {
  Controller,
  Get,
  NotFoundException,
  Param,
  Query,
  UseGuards,
} from "@nestjs/common";
import { ApiIdentityGuard } from "../auth/session-identity.guard";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { CurrentIdentity } from "../identity/identity.decorator";
import { ListProceduresDto, type ProcedureSource } from "./list-procedures.dto";
import {
  ProceduresService,
  type ProcedureDetail,
  type ProcedureListItem,
} from "./procedures.service";

@Controller("procedures")
@UseGuards(ApiIdentityGuard)
export class ProceduresController {
  public constructor(private readonly procedures: ProceduresService) {}

  @Get()
  public async list(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Query() query: ListProceduresDto,
  ): Promise<{ success: true; data: ProcedureListItem[] }> {
    return { success: true, data: await this.procedures.list(identity, query) };
  }

  @Get(":source/:id")
  public async getById(
    @CurrentIdentity() identity: AuthenticatedIdentity,
    @Param("source") source: string,
    @Param("id") rawId: string,
  ): Promise<{ success: true; data: ProcedureDetail }> {
    if (!isProcedureSource(source)) {
      throw new NotFoundException("Không tìm thấy thủ tục.");
    }
    const id = parseProcedureId(rawId);

    const procedure = await this.procedures.findVisibleById(
      identity,
      source,
      id,
    );
    // Use one response for missing and out-of-scope records so an account
    // cannot enumerate procedures belonging to other departments.
    if (!procedure) throw new NotFoundException("Không tìm thấy thủ tục.");
    return { success: true, data: procedure };
  }
}

function isProcedureSource(value: string): value is ProcedureSource {
  return value === "leave" || value === "work";
}

/**
 * SQL Server procedure identifiers are positive INT values.  ParseIntPipe
 * accepts negative values and values beyond SQL INT range; rejecting those
 * before querying keeps invalid URLs indistinguishable from inaccessible
 * records and avoids driver-level conversion errors.
 */
function parseProcedureId(value: string): number {
  if (!/^[1-9]\d*$/.test(value)) {
    throw new NotFoundException("Không tìm thấy thủ tục.");
  }

  const id = Number(value);
  if (!Number.isSafeInteger(id) || id > 2147483647) {
    throw new NotFoundException("Không tìm thấy thủ tục.");
  }
  return id;
}
