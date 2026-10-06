import { Type } from "class-transformer";
import { IsInt, IsOptional, Matches, Max, Min } from "class-validator";

const isoDatePattern = /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/;

/**
 * The API deliberately accepts date-only values.  Attendance is stored as
 * Vietnam-local business time, so accepting browser timestamps would make the
 * requested work date depend on the caller's time zone.
 */
export class ListAttendanceDto {
  @IsOptional()
  @Matches(isoDatePattern, {
    message: "fromDate phải có định dạng YYYY-MM-DD.",
  })
  public fromDate?: string;

  @IsOptional()
  @Matches(isoDatePattern, {
    message: "toDate phải có định dạng YYYY-MM-DD.",
  })
  public toDate?: string;

  /**
   * Optional staff filter.  SQL scope still applies, so this cannot be used
   * to discover a colleague outside the caller's department/company scope.
   */
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(2147483647)
  public userId?: number;
}
