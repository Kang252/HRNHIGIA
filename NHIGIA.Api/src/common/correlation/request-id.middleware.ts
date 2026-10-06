import { Injectable, Logger, type NestMiddleware } from "@nestjs/common";
import { randomUUID } from "node:crypto";
import type { NextFunction, Request, Response } from "express";

const requestIdPattern = /^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$/;

declare global {
  namespace Express {
    interface Request {
      requestId?: string;
    }
  }
}

@Injectable()
export class RequestIdMiddleware implements NestMiddleware {
  private readonly logger = new Logger("HTTP");

  public use(request: Request, response: Response, next: NextFunction): void {
    const inbound = request.header("x-request-id");
    const requestId =
      inbound && requestIdPattern.test(inbound) ? inbound : randomUUID();
    const startedAt = process.hrtime.bigint();

    request.requestId = requestId;
    response.setHeader("x-request-id", requestId);
    response.on("finish", () => {
      const elapsedMs = Number(process.hrtime.bigint() - startedAt) / 1_000_000;
      this.logger.log(
        `${request.method} ${request.originalUrl} ${response.statusCode} ${elapsedMs.toFixed(1)}ms requestId=${requestId}`,
      );
    });
    next();
  }
}
