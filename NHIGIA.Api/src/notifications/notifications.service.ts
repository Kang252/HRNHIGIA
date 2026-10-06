import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SchemaReadinessService } from "../database/schema-readiness.service";
import { SqlServerService } from "../database/sql-server.service";

export interface NotificationItem {
  id: number;
  title: string;
  message: string;
  linkUrl: string | null;
  isRead: boolean;
  createdAt: Date;
}

export interface NotificationList {
  items: NotificationItem[];
  unreadCount: number;
}

export interface NotificationReadResult {
  id: number;
  linkUrl: string | null;
}

interface NotificationRow {
  Id: number;
  Title: string;
  Message: string;
  LinkUrl: string | null;
  IsRead: boolean | number;
  CreatedAt: Date;
}

interface NotificationReadRow {
  Id: number;
  LinkUrl: string | null;
}

interface NotificationCountRow {
  Count: number;
}

@Injectable()
export class NotificationsService {
  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly schemaReadiness: SchemaReadinessService,
  ) {}

  public async list(
    identity: AuthenticatedIdentity,
    take = 20,
  ): Promise<NotificationList> {
    await this.schemaReadiness.assertReady("notifications");
    const pool = await this.sqlServer.getPool();
    const [itemsResult, unreadResult] = await Promise.all([
      pool
        .request()
        .input("UserId", sql.Int, identity.userId)
        .input("Take", sql.Int, take)
        .query<NotificationRow>(`SELECT TOP (@Take) Id,Title,Message,LinkUrl,IsRead,CreatedAt
          FROM dbo.HrmNotification
          WHERE UserId=@UserId
          ORDER BY CreatedAt DESC,Id DESC;`),
      pool.request().input("UserId", sql.Int, identity.userId)
        .query<NotificationCountRow>(`SELECT COUNT(1) AS Count
          FROM dbo.HrmNotification
          WHERE UserId=@UserId AND IsRead=0;`),
    ]);
    return {
      items: itemsResult.recordset.map(toNotificationItem),
      unreadCount: unreadResult.recordset[0]?.Count ?? 0,
    };
  }

  public async unreadCount(identity: AuthenticatedIdentity): Promise<number> {
    await this.schemaReadiness.assertReady("notifications");
    const pool = await this.sqlServer.getPool();
    const result = await pool
      .request()
      .input("UserId", sql.Int, identity.userId)
      .query<NotificationCountRow>(`SELECT COUNT(1) AS Count
        FROM dbo.HrmNotification
        WHERE UserId=@UserId AND IsRead=0;`);
    return result.recordset[0]?.Count ?? 0;
  }

  public async readAll(identity: AuthenticatedIdentity): Promise<number> {
    await this.schemaReadiness.assertReady("notifications");
    const pool = await this.sqlServer.getPool();
    const result = await pool
      .request()
      .input("UserId", sql.Int, identity.userId)
      .query(`UPDATE dbo.HrmNotification
        SET IsRead=1
        WHERE UserId=@UserId AND IsRead=0;`);
    return result.rowsAffected[0] ?? 0;
  }

  public async read(
    identity: AuthenticatedIdentity,
    notificationId: number,
  ): Promise<NotificationReadResult> {
    if (!Number.isSafeInteger(notificationId) || notificationId < 1) {
      throw new BadRequestException("Mã thông báo không hợp lệ.");
    }
    await this.schemaReadiness.assertReady("notifications");
    const pool = await this.sqlServer.getPool();
    const result = await pool
      .request()
      .input("Id", sql.BigInt, notificationId)
      .input("UserId", sql.Int, identity.userId)
      .query<NotificationReadRow>(`UPDATE dbo.HrmNotification
        SET IsRead=1
        OUTPUT INSERTED.Id,INSERTED.LinkUrl
        WHERE Id=@Id AND UserId=@UserId;`);
    const row = result.recordset[0];
    if (!row) throw new NotFoundException("Không tìm thấy thông báo.");
    return { id: row.Id, linkUrl: safeLocalLink(row.LinkUrl) };
  }
}

export function toNotificationItem(row: NotificationRow): NotificationItem {
  return {
    id: row.Id,
    title: row.Title,
    message: row.Message,
    linkUrl: safeLocalLink(row.LinkUrl),
    isRead: row.IsRead === true || row.IsRead === 1,
    createdAt: row.CreatedAt,
  };
}

/**
 * Notification links are navigation hints, never redirects to arbitrary
 * websites.  This keeps rows authored by old integrations safe to render in
 * the modern client while retaining legacy in-app links such as /Work?kind=.
 */
export function safeLocalLink(value: string | null): string | null {
  if (!value) return null;
  const link = value.trim();
  const decoded = decodeURIComponentSafely(link);
  if (
    !link.startsWith("/") ||
    link.startsWith("//") ||
    link.startsWith("/\\") ||
    link.includes("\\") ||
    hasControlCharacter(link) ||
    !decoded ||
    decoded.startsWith("//") ||
    decoded.startsWith("/\\") ||
    decoded.includes("\\") ||
    hasControlCharacter(decoded)
  ) {
    return null;
  }
  return link;
}

function decodeURIComponentSafely(value: string): string | null {
  try {
    return decodeURIComponent(value);
  } catch {
    return null;
  }
}

function hasControlCharacter(value: string): boolean {
  return [...value].some((character) => {
    const code = character.charCodeAt(0);
    return code < 32 || code === 127;
  });
}
