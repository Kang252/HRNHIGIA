import {
  IsIn,
  IsInt,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from "class-validator";
import { Type } from "class-transformer";

export class ListCommunicationsDto {
  @IsOptional()
  @IsString()
  @MaxLength(100)
  public keyword?: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  public category?: string;

  @IsOptional()
  @IsIn(["PUBLISHED", "PENDING", "REJECTED"])
  public status?: "PUBLISHED" | "PENDING" | "REJECTED";

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  public take?: number;
}
