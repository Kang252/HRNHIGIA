import { Transform } from "class-transformer";
import {
  IsBoolean,
  IsIn,
  IsNotEmpty,
  IsOptional,
  IsString,
  MaxLength,
} from "class-validator";

const trimText = ({ value }: { value: unknown }): unknown =>
  typeof value === "string" ? value.trim() : value;

const normalizeScope = ({ value }: { value: unknown }): unknown =>
  typeof value === "string" ? value.trim().toUpperCase() : value;

/**
 * Text-only post input for the first NestJS communications slice. Attachments
 * remain on the legacy endpoint until their multipart file validation and
 * download authorization are moved as one reviewed change.
 */
export class CreateCommunicationDto {
  @IsOptional()
  @Transform(trimText)
  @IsString()
  @MaxLength(80)
  public category?: string;

  @IsOptional()
  @Transform(normalizeScope)
  @IsIn(["ALL", "DEPARTMENT", "MANAGER"])
  public scopeCode?: "ALL" | "DEPARTMENT" | "MANAGER";

  @Transform(trimText)
  @IsString()
  @IsNotEmpty()
  @MaxLength(200)
  public title!: string;

  @Transform(trimText)
  @IsString()
  @IsNotEmpty()
  @MaxLength(30000)
  public body!: string;

  @IsOptional()
  @IsBoolean()
  public isPinned?: boolean;
}
