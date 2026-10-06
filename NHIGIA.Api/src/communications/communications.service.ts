import { Injectable } from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SqlServerService } from "../database/sql-server.service";
import { sqlScope } from "../dashboard/dashboard.service";
import type { ListCommunicationsDto } from "./list-communications.dto";

export interface CommunicationListItem {
  id: number;
  authorUserId: number;
  authorName: string;
  authorAvatarUrl: string | null;
  authorJobTitle: string | null;
  category: string | null;
  scopeCode: string;
  departmentId: number | null;
  title: string;
  body: string;
  attachmentName: string | null;
  attachmentContentType: string | null;
  isPinned: boolean;
  statusCode: string;
  submittedAt: Date | null;
  publishedAt: Date | null;
  likeCount: number;
  commentCount: number;
  likedByCurrentUser: boolean;
  isMine: boolean;
}

/**
 * Raw SQL Server row.  The legacy database schema uses PascalCase column
 * names, so keep that contract explicit here and map it before returning the
 * JSON response.  Do not type this as CommunicationListItem: mssql preserves
 * the SQL column names and doing so silently produced empty React cards.
 */
export interface CommunicationRow {
  Id: number;
  AuthorUserId: number;
  AuthorName: string;
  AuthorAvatarUrl: string | null;
  AuthorJobTitle: string | null;
  Category: string | null;
  ScopeCode: string;
  DepartmentId: number | null;
  Title: string;
  Body: string;
  AttachmentName: string | null;
  AttachmentContentType: string | null;
  IsPinned: boolean | number;
  StatusCode: string;
  SubmittedAt: Date | null;
  PublishedAt: Date | null;
  LikeCount: number;
  CommentCount: number;
  LikedByCurrentUser: boolean | number;
  IsMine: boolean | number;
}

@Injectable()
export class CommunicationsService {
  public constructor(private readonly sqlServer: SqlServerService) {}

  public async list(
    identity: AuthenticatedIdentity,
    filter: ListCommunicationsDto,
  ): Promise<CommunicationListItem[]> {
    const scope = sqlScope(identity);
    const pool = await this.sqlServer.getPool();
    const result = await pool
      .request()
      .input("Take", sql.Int, filter.take ?? 20)
      .input("DepartmentId", sql.Int, identity.departmentId ?? null)
      .input("IsManager", sql.Bit, scope.isManager)
      .input("CanModerate", sql.Bit, scope.canSeeAll)
      .input("ActorId", sql.Int, identity.userId)
      .input("Keyword", sql.NVarChar(100), filter.keyword?.trim() ?? "")
      .input("Category", sql.NVarChar(100), filter.category?.trim() ?? "")
      .input("Status", sql.VarChar(20), filter.status ?? "")
      .query<CommunicationRow>(`SELECT TOP (@Take) c.Id,c.AuthorUserId,u.DisplayName AS AuthorName,
        CASE WHEN authorProfile.AvatarContent IS NOT NULL
          THEN CONCAT('/api/v1/users/',u.Id,'/avatar?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(authorProfile.UpdatedAt,authorProfile.CreatedAt)))
          ELSE authorProfile.AvatarUrl END AS AuthorAvatarUrl,
        authorProfile.JobTitle AS AuthorJobTitle,c.Category,c.ScopeCode,c.DepartmentId,c.Title,c.Body,
        c.AttachmentName,c.AttachmentContentType,c.IsPinned,
        COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END) AS StatusCode,
        COALESCE(c.SubmittedAt,c.PublishedAt) AS SubmittedAt,c.PublishedAt,
        (SELECT COUNT(1) FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id) AS LikeCount,
        (SELECT COUNT(1) FROM dbo.HrmCommunicationComment m WHERE m.CommunicationId=c.Id AND m.IsDeleted=0) AS CommentCount,
        CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id AND r.UserId=@ActorId) THEN 1 ELSE 0 END AS bit) AS LikedByCurrentUser,
        CAST(CASE WHEN c.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS bit) AS IsMine
        FROM dbo.HrmCommunication c
        INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
        LEFT JOIN dbo.HrmEmployeeProfile authorProfile ON authorProfile.UserId=u.Id
        WHERE ((c.IsPublished=1 AND (c.ScopeCode='ALL' OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
                  OR (c.ScopeCode='MANAGER' AND @IsManager=1)))
               OR c.AuthorUserId=@ActorId
               OR (@CanModerate=1 AND COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)='PENDING'))
          AND (@Status='' OR COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)=@Status)
          AND (@Keyword='' OR c.Title LIKE '%' + @Keyword + '%' OR c.Body LIKE '%' + @Keyword + '%')
          AND (@Category='' OR c.Category=@Category)
        ORDER BY CASE WHEN COALESCE(c.StatusCode,'PUBLISHED')='PENDING' THEN 0 ELSE 1 END,
          c.IsPinned DESC,COALESCE(c.SubmittedAt,c.PublishedAt) DESC;`);
    return result.recordset.map(toCommunicationListItem);
  }
}

export function toCommunicationListItem(
  item: CommunicationRow,
): CommunicationListItem {
  return {
    id: item.Id,
    authorUserId: item.AuthorUserId,
    authorName: item.AuthorName,
    authorAvatarUrl: item.AuthorAvatarUrl,
    authorJobTitle: item.AuthorJobTitle,
    category: item.Category,
    scopeCode: item.ScopeCode,
    departmentId: item.DepartmentId,
    title: item.Title,
    body: item.Body,
    attachmentName: item.AttachmentName,
    attachmentContentType: item.AttachmentContentType,
    isPinned: isSqlBitSet(item.IsPinned),
    statusCode: item.StatusCode,
    submittedAt: item.SubmittedAt,
    publishedAt: item.PublishedAt,
    likeCount: item.LikeCount,
    commentCount: item.CommentCount,
    likedByCurrentUser: isSqlBitSet(item.LikedByCurrentUser),
    isMine: isSqlBitSet(item.IsMine),
  };
}

function isSqlBitSet(value: boolean | number): boolean {
  return value === true || value === 1;
}
