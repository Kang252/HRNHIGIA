import "reflect-metadata";
import { Logger, ValidationPipe, type INestApplication } from "@nestjs/common";
import { NestFactory } from "@nestjs/core";
import helmet from "helmet";
import { existsSync } from "node:fs";
import { join, resolve } from "node:path";
import {
  static as serveStatic,
  type NextFunction,
  type Request,
  type Response,
} from "express";
import { AppModule } from "./app.module";
import { RuntimeConfigService } from "./config/runtime-config.service";
import { HttpExceptionFilter } from "./common/correlation/http-exception.filter";

async function bootstrap(): Promise<void> {
  const app = await NestFactory.create(AppModule, { bufferLogs: true });
  const config = app.get(RuntimeConfigService);
  const expressApp = app.getHttpAdapter().getInstance() as ExpressApplication;
  expressApp.set("trust proxy", config.trustProxyHops);
  app.useLogger([config.logLevel]);
  app.enableShutdownHooks();
  app.use(
    helmet({
      contentSecurityPolicy: {
        directives: {
          defaultSrc: ["'self'"],
          baseUri: ["'self'"],
          connectSrc: ["'self'"],
          fontSrc: ["'self'", "data:"],
          frameAncestors: ["'self'"],
          imgSrc: ["'self'", "data:"],
          objectSrc: ["'none'"],
          scriptSrc: ["'self'"],
          styleSrc: ["'self'"],
        },
      },
      crossOriginEmbedderPolicy: false,
    }),
  );
  app.setGlobalPrefix(config.apiPrefix);
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      forbidNonWhitelisted: true,
      transform: true,
    }),
  );
  app.useGlobalFilters(new HttpExceptionFilter());
  configureClientApp(app, config.clientAppDir);

  await app.listen(config.port, config.host);
  new Logger("Bootstrap").log(
    `NHIGIA.Api listening on http://${config.host}:${config.port}/${config.apiPrefix}`,
  );
}

function configureClientApp(
  app: INestApplication,
  configuredDir?: string,
): void {
  if (!configuredDir) return;
  const clientRoot = resolve(configuredDir);
  const indexFile = join(clientRoot, "index.html");
  if (!existsSync(indexFile)) {
    new Logger("StaticClient").warn(
      `CLIENT_APP_DIR does not contain index.html: ${clientRoot}`,
    );
    return;
  }

  // Register on the underlying Express application. Nest's global prefix is
  // deliberately only for API controllers; the React entrypoint must remain
  // reachable at /modern-ui/ rather than /api/v1/modern-ui/.
  const expressApp = app.getHttpAdapter().getInstance() as ExpressApplication;
  expressApp.use(
    "/modern-ui",
    serveStatic(clientRoot, {
      index: false,
      fallthrough: true,
      immutable: true,
      maxAge: "1y",
      setHeaders: (response, path) => {
        if (path.endsWith("index.html")) {
          response.setHeader("Cache-Control", "no-cache");
        }
      },
    }),
  );
  expressApp.use(
    "/modern-ui",
    (request: Request, response: Response, next: NextFunction) => {
      const acceptsHtml =
        request.headers.accept
          ?.split(",")
          .some((value) =>
            value.trim().toLowerCase().startsWith("text/html"),
          ) ?? false;
      if (
        (request.method === "GET" || request.method === "HEAD") &&
        acceptsHtml
      ) {
        response.setHeader("Cache-Control", "no-cache");
        response.sendFile(indexFile, (error) => {
          if (error) next(error);
        });
        return;
      }
      next();
    },
  );
}

interface ExpressApplication {
  set(setting: string, value: unknown): void;
  use: (...handlers: unknown[]) => void;
}

void bootstrap();
