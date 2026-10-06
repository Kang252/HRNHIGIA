import {
  ArgumentsHost,
  Catch,
  HttpException,
  HttpStatus,
  Logger,
  type ExceptionFilter,
} from "@nestjs/common";
import type { Request, Response } from "express";

@Catch()
export class HttpExceptionFilter implements ExceptionFilter {
  private readonly logger = new Logger(HttpExceptionFilter.name);

  public catch(exception: unknown, host: ArgumentsHost): void {
    const context = host.switchToHttp();
    const request = context.getRequest<Request>();
    const response = context.getResponse<Response>();
    const requestId = request.requestId;
    const status =
      exception instanceof HttpException
        ? exception.getStatus()
        : HttpStatus.INTERNAL_SERVER_ERROR;
    const exceptionResponse =
      exception instanceof HttpException ? exception.getResponse() : undefined;
    const message = this.messageFor(exceptionResponse, status);

    if (status >= HttpStatus.INTERNAL_SERVER_ERROR) {
      this.logger.error(
        `${request.method} ${request.originalUrl} failed requestId=${requestId ?? "unknown"}`,
        exception instanceof HttpException
          ? undefined
          : exception instanceof Error
            ? exception.stack
            : undefined,
      );
    }

    response.status(status).json({
      success: false,
      error: {
        code: HttpStatus[status] ?? "ERROR",
        message,
      },
      requestId,
    });
  }

  private messageFor(
    response: string | object | undefined,
    status: number,
  ): string | string[] {
    if (typeof response === "string") return response;
    if (response && "message" in response) {
      const value = (response as { message?: unknown }).message;
      if (typeof value === "string" || Array.isArray(value))
        return value as string | string[];
    }
    return status >= HttpStatus.INTERNAL_SERVER_ERROR
      ? "Internal server error."
      : "Request could not be completed.";
  }
}
