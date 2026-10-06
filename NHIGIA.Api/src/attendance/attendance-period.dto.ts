import { Matches } from "class-validator";

const attendancePeriodPattern = /^\d{4}-(0[1-9]|1[0-2])$/;

export class AttendancePeriodDto {
  @Matches(attendancePeriodPattern, {
    message: "period phải có định dạng YYYY-MM.",
  })
  public period!: string;
}
