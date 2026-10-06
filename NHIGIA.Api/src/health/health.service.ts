import { Injectable } from "@nestjs/common";
import {
  SqlServerService,
  type SqlServerHealth,
} from "../database/sql-server.service";

export interface HealthStatus {
  status: "ok" | "degraded";
  service: "nhigia-api";
  time: string;
  dependencies: {
    sqlServer: SqlServerHealth;
  };
}

@Injectable()
export class HealthService {
  public constructor(private readonly sqlServer: SqlServerService) {}

  public async status(): Promise<HealthStatus> {
    const sqlServer = await this.sqlServer.health();
    const sessionSchemaReady =
      sqlServer.apiSessionSchema?.ready === undefined ||
      sqlServer.apiSessionSchema.ready;
    return {
      status:
        (sqlServer.configured && !sqlServer.connected) || !sessionSchemaReady
          ? "degraded"
          : "ok",
      service: "nhigia-api",
      time: new Date().toISOString(),
      dependencies: { sqlServer },
    };
  }
}
