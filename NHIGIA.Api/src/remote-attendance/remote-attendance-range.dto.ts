import { Matches } from "class-validator";

const localDatePattern = /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/;

/**
 * Remote-work evidence is evaluated in Vietnam business dates.  Date-only
 * parameters keep a browser's time zone from changing the selected day.
 */
export class RemoteAttendanceRangeDto {
  @Matches(localDatePattern, {
    message: "fromDate phải có định dạng YYYY-MM-DD.",
  })
  public fromDate!: string;

  @Matches(localDatePattern, {
    message: "toDate phải có định dạng YYYY-MM-DD.",
  })
  public toDate!: string;
}
