import { Type } from "class-transformer";
import {
  IsInt,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from "class-validator";

/**
 * Filters only the directory fields that the API exposes.  It deliberately
 * does not accept arbitrary sort or column input so the SQL query remains
 * fixed and parameterized.
 */
export class ListUsersDto {
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
