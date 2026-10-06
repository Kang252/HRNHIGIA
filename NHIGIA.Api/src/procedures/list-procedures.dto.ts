import { Type } from "class-transformer";
import {
  IsIn,
  IsInt,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from "class-validator";

export const procedureSources = ["leave", "work"] as const;
export type ProcedureSource = (typeof procedureSources)[number];

// These are the work records that act as employee procedures or requests.
// Other HrmWorkItem kinds carry sensitive payroll, asset, KPI, recruitment or
// training data and have their own API migration slices.
export const procedureWorkKinds = [
  "overtime",
  "resignation",
  "helpdesk",
  "vehicle",
  "meeting",
  "business-trip",
  "transfer",
] as const;
export type ProcedureWorkKind = (typeof procedureWorkKinds)[number];

export const procedureKinds = ["leave", ...procedureWorkKinds] as const;
export type ProcedureKind = (typeof procedureKinds)[number];

export class ListProceduresDto {
  @IsOptional()
  @IsIn(procedureSources)
  public source?: ProcedureSource;

  @IsOptional()
  @IsIn(procedureKinds)
  public kind?: ProcedureKind;

  @IsOptional()
  @IsString()
  @MaxLength(50)
  public status?: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  public keyword?: string;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  public take?: number;
}
