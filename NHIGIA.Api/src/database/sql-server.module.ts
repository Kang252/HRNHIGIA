import { Global, Module } from "@nestjs/common";
import { SchemaReadinessService } from "./schema-readiness.service";
import { SqlServerService } from "./sql-server.service";

@Global()
@Module({
  providers: [SqlServerService, SchemaReadinessService],
  exports: [SqlServerService, SchemaReadinessService],
})
export class SqlServerModule {}
