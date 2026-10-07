import { Transform } from "class-transformer";
import { IsBoolean, IsString, MaxLength, MinLength } from "class-validator";

export class ReviewRemoteItemDto {
  @IsBoolean()
  public approve!: boolean;

  @Transform(({ value }: { value: unknown }) =>
    typeof value === "string" ? value.trim() : value,
  )
  @IsString()
  @MinLength(1)
  @MaxLength(1000)
  public note!: string;
}
