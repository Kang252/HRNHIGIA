import type { IncomingHttpHeaders } from "node:http";

export interface AuthenticatedIdentity {
  userId: number;
  username: string;
  roleCode: "ADMIN" | "HR" | "DIRECTOR" | "MANAGER" | "EMPLOYEE";
  departmentId?: number;
  displayName?: string;
  departmentName?: string;
  supervisorUserId?: number;
}

export interface CorrelatedRequest {
  requestId?: string;
  identity?: AuthenticatedIdentity;
  headers: IncomingHttpHeaders;
}
