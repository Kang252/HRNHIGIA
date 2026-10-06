import { Module } from "@nestjs/common";
import { AuthModule } from "../auth/auth.module";
import { ProceduresController } from "./procedures.controller";
import { ProceduresService } from "./procedures.service";

@Module({
  imports: [AuthModule],
  controllers: [ProceduresController],
  providers: [ProceduresService],
})
export class ProceduresModule {}
