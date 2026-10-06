import { createParamDecorator, type ExecutionContext } from "@nestjs/common";
import type { Request } from "express";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";

export const CurrentIdentity = createParamDecorator(
  (_data: unknown, context: ExecutionContext): AuthenticatedIdentity => {
    const request = context.switchToHttp().getRequest<Request>();
    if (!request.identity)
      throw new Error("Authenticated identity was not attached by the guard.");
    return request.identity;
  },
);
