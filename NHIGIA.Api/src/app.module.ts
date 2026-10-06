import {
  MiddlewareConsumer,
  Module,
  RequestMethod,
  type NestModule,
} from "@nestjs/common";
import { ApplicationConfigModule } from "./config/config.module";
import { SqlServerModule } from "./database/sql-server.module";
import { RequestIdMiddleware } from "./common/correlation/request-id.middleware";
import { HealthModule } from "./health/health.module";
import { AuthModule } from "./auth/auth.module";
import { DashboardModule } from "./dashboard/dashboard.module";
import { CommunicationsModule } from "./communications/communications.module";
import { UsersModule } from "./users/users.module";

@Module({
  imports: [
    ApplicationConfigModule,
    SqlServerModule,
    HealthModule,
    AuthModule,
    DashboardModule,
    CommunicationsModule,
    UsersModule,
  ],
})
export class AppModule implements NestModule {
  public configure(consumer: MiddlewareConsumer): void {
    consumer
      .apply(RequestIdMiddleware)
      .forRoutes({ path: "*path", method: RequestMethod.ALL });
  }
}
