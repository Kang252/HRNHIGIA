import { Transform, Type } from "class-transformer";
import {
  IsBoolean,
  IsIn,
  IsInt,
  IsNumber,
  IsOptional,
  IsString,
  Matches,
  Max,
  MaxLength,
  Min,
  MinLength,
} from "class-validator";

const localDatePattern = /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/;
const timePattern = /^([01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?$/;

function trimText({ value }: { value: unknown }): unknown {
  return typeof value === "string" ? value.trim() : value;
}

/**
 * This deliberately contains no biometric field or photo.  Face images stay
 * in the existing C# service until protected-photo interoperability has been
 * designed and verified separately.
 */
export class CreateRemotePlanDto {
  @IsIn(["FIELD", "HOME"], { message: "mode phải là FIELD hoặc HOME." })
  public mode!: "FIELD" | "HOME";

  @Transform(trimText)
  @IsString()
  @MinLength(1)
  @MaxLength(200)
  public placeName!: string;

  @Matches(localDatePattern, {
    message: "fromDate phải có định dạng YYYY-MM-DD.",
  })
  public fromDate!: string;

  @Matches(localDatePattern, {
    message: "toDate phải có định dạng YYYY-MM-DD.",
  })
  public toDate!: string;

  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(127)
  public workDaysMask!: number;

  @IsOptional()
  @Matches(timePattern, {
    message: "windowStart phải có định dạng HH:mm hoặc HH:mm:ss.",
  })
  public windowStart?: string;

  @IsOptional()
  @Matches(timePattern, {
    message: "windowEnd phải có định dạng HH:mm hoặc HH:mm:ss.",
  })
  public windowEnd?: string;

  @IsOptional()
  @IsBoolean()
  public isFlexible?: boolean;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(1440)
  public requiredMinutes?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  @Max(1439)
  public breakMinutes?: number;

  @IsOptional()
  @Type(() => Number)
  @IsNumber({ allowInfinity: false, allowNaN: false })
  @Min(-90)
  @Max(90)
  public latitude?: number;

  @IsOptional()
  @Type(() => Number)
  @IsNumber({ allowInfinity: false, allowNaN: false })
  @Min(-180)
  @Max(180)
  public longitude?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(50)
  @Max(2000)
  public radiusMeters?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(2147483647)
  public leaveRequestId?: number;

  @IsOptional()
  @Transform(trimText)
  @IsString()
  @MaxLength(2000)
  public note?: string;
}
