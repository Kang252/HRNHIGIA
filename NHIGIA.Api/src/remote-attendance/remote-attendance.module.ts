import { Module } from "@nestjs/common";
import { AuthModule } from "../auth/auth.module";
import { RemoteAttendanceController } from "./remote-attendance.controller";
import { RemoteAttendanceService } from "./remote-attendance.service";

@Module({
  imports: [AuthModule],
  controllers: [RemoteAttendanceController],
  providers: [RemoteAttendanceService],
})
export class RemoteAttendanceModule {}
