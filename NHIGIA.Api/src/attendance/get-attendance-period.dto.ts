import { IsOptional, Matches } from "class-validator";

const attendancePeriodPattern = /^\d{4}-(0[1-9]|1[0-2])$/;

/**
 * GET /attendance/period accepts an omitted period and resolves it to the
 * current Vietnam business month.  An explicitly supplied value remains
 * strictly validated so a typo never silently selects another month.
 */
export class GetAttendancePeriodDto {
  @IsOptional()
  @Matches(attendancePeriodPattern, {
    message: "period phải có định dạng YYYY-MM.",
  })
  public period?: string;
}
