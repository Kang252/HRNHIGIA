import { Global, Module } from "@nestjs/common";
import { ConfigModule } from "@nestjs/config";
import { RuntimeConfigService } from "./runtime-config.service";
import { validateEnvironment } from "./env.validation";

@Global()
@Module({
  imports: [
    ConfigModule.forRoot({
      cache: true,
      isGlobal: true,
      validate: validateEnvironment,
    }),
  ],
  providers: [RuntimeConfigService],
  exports: [RuntimeConfigService],
})
export class ApplicationConfigModule {}
