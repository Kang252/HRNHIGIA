import {
  BadRequestException,
  ForbiddenException,
  Injectable,
  NotFoundException,
} from "@nestjs/common";
import * as sql from "mssql";
import type { AuthenticatedIdentity } from "../common/correlation/request-context";
import { SchemaReadinessService } from "../database/schema-readiness.service";
import { SqlServerService } from "../database/sql-server.service";
import { sqlScope } from "../dashboard/dashboard.service";
import type { CreateCommunicationCommentDto } from "./create-communication-comment.dto";
import type { CreateCommunicationDto } from "./create-communication.dto";
import type { ListCommunicationCommentsDto } from "./list-communication-comments.dto";
import type { ListCommunicationsDto } from "./list-communications.dto";
import type { ModerateCommunicationDto } from "./moderate-communication.dto";

const defaultCategory = "Thông báo";
const communicationsLink = "/Home/InternalCommunications";
type QueryParent = sql.ConnectionPool | sql.Transaction;
type ScopeCode = "ALL" | "DEPARTMENT" | "MANAGER";

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

export interface CommunicationCommentItem {
  id: number;
  communicationId: number;
  authorUserId: number;
  authorName: string;
  authorAvatarUrl: string | null;
  authorJobTitle: string | null;
  body: string;
  createdAt: Date;
  isMine: boolean;
}

export interface CommunicationReactionResult {
  liked: boolean;
  count: number;
}

export interface CommunicationModerationResult {
  id: number;
  statusCode: "PUBLISHED" | "REJECTED";
}

/**
 * The legacy SQL schema preserves PascalCase field names. Mapping it here
 * avoids silently returning empty camelCase fields to the React client.
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

interface CommentRow {
  Id: number;
  CommunicationId: number;
  AuthorUserId: number;
  AuthorName: string;
  AuthorAvatarUrl: string | null;
  AuthorJobTitle: string | null;
  Body: string;
  CreatedAt: Date;
  IsMine: boolean | number;
}

interface EditableRow {
  Id: number;
  AuthorUserId: number;
  Category: string | null;
  ScopeCode: string;
  DepartmentId: number | null;
  Title: string;
  IsPinned: boolean | number;
}

@Injectable()
export class CommunicationsService {
  public constructor(
    private readonly sqlServer: SqlServerService,
    private readonly schemaReadiness: SchemaReadinessService,
  ) {}

  public async list(
    identity: AuthenticatedIdentity,
    filter: ListCommunicationsDto,
  ): Promise<CommunicationListItem[]> {
    await this.schemaReadiness.assertReady("communications");
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
      .query<CommunicationRow>(listSql);
    return result.recordset.map(toCommunicationListItem);
  }

  public async create(
    identity: AuthenticatedIdentity,
    input: CreateCommunicationDto,
    ipAddress: string | undefined,
  ): Promise<CommunicationListItem> {
    await this.schemaReadiness.assertReady("communications");
    const scopeCode = normalizeScope(input.scopeCode ?? "DEPARTMENT");
    assertScopePermission(identity, scopeCode);
    const departmentId = departmentForNewPost(scopeCode, identity);
    const canModerate = sqlScope(identity).canSeeAll;
    const category = optionalText(input.category) ?? defaultCategory;
    const title = requiredText(input.title, "Tiêu đề bài đăng");
    const body = requiredText(input.body, "Nội dung bài đăng");
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        const created = await new sql.Request(tx)
          .input("AuthorUserId", sql.Int, identity.userId)
          .input("Category", sql.NVarChar(80), category)
          .input("ScopeCode", sql.NVarChar(30), scopeCode)
          .input("DepartmentId", sql.Int, departmentId)
          .input("Title", sql.NVarChar(200), title)
          .input("Body", sql.NVarChar(sql.MAX), body)
          .input("IsPinned", sql.Bit, canModerate && input.isPinned === true)
          .input("IsPublished", sql.Bit, canModerate)
          .input(
            "StatusCode",
            sql.NVarChar(20),
            canModerate ? "PUBLISHED" : "PENDING",
          )
          .input(
            "ApprovedByUserId",
            sql.Int,
            canModerate ? identity.userId : null,
          ).query<{ Id: number }>(`
          INSERT dbo.HrmCommunication
            (AuthorUserId,Category,ScopeCode,DepartmentId,Title,Body,IsPinned,IsPublished,
             StatusCode,SubmittedAt,ApprovedByUserId,ReviewedAt)
          OUTPUT INSERTED.Id AS Id
          VALUES
            (@AuthorUserId,@Category,@ScopeCode,@DepartmentId,@Title,@Body,@IsPinned,@IsPublished,
             @StatusCode,SYSDATETIME(),@ApprovedByUserId,
             CASE WHEN @IsPublished=1 THEN SYSDATETIME() ELSE NULL END);`);
        const id = created.recordset[0]?.Id;
        if (!id) throw new Error("Không thể tạo bài đăng truyền thông.");

        await this.audit(
          tx,
          identity.userId,
          canModerate ? "PUBLISH" : "SUBMIT",
          id,
          title,
          ipAddress,
        );
        if (canModerate) await this.notifyPublished(tx, id);
        else await this.notifyReviewers(tx, id, identity.userId, false);

        const item = await this.readPost(tx, identity, id);
        if (!item) throw new Error("Không thể đọc bài đăng vừa tạo.");
        return item;
      },
    );
  }

  public async update(
    identity: AuthenticatedIdentity,
    id: number,
    input: CreateCommunicationDto,
    ipAddress: string | undefined,
  ): Promise<CommunicationListItem> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    const canModerate = sqlScope(identity).canSeeAll;
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        const current = await this.editablePost(tx, identity, id, canModerate);
        const scopeCode = normalizeScope(input.scopeCode ?? current.ScopeCode);
        assertScopePermission(identity, scopeCode);
        const departmentId = departmentForUpdatedPost(
          scopeCode,
          identity,
          current,
        );
        const category =
          optionalText(input.category) ?? current.Category ?? defaultCategory;
        const title = requiredText(input.title, "Tiêu đề bài đăng");
        const body = requiredText(input.body, "Nội dung bài đăng");
        const isPinned = canModerate
          ? (input.isPinned ?? asBool(current.IsPinned))
          : asBool(current.IsPinned);

        await new sql.Request(tx)
          .input("Id", sql.Int, id)
          .input("ActorId", sql.Int, identity.userId)
          .input("CanModerate", sql.Bit, canModerate)
          .input("Category", sql.NVarChar(80), category)
          .input("ScopeCode", sql.NVarChar(30), scopeCode)
          .input("DepartmentId", sql.Int, departmentId)
          .input("Title", sql.NVarChar(200), title)
          .input("Body", sql.NVarChar(sql.MAX), body)
          .input("IsPinned", sql.Bit, isPinned).query(`
          UPDATE dbo.HrmCommunication
          SET Category=@Category,ScopeCode=@ScopeCode,DepartmentId=@DepartmentId,
              Title=@Title,Body=@Body,
              IsPinned=CASE WHEN @CanModerate=1 THEN @IsPinned ELSE IsPinned END,
              IsPublished=CASE WHEN @CanModerate=1 THEN IsPublished ELSE 0 END,
              StatusCode=CASE WHEN @CanModerate=1 THEN StatusCode ELSE 'PENDING' END,
              SubmittedAt=CASE WHEN @CanModerate=1 THEN SubmittedAt ELSE SYSDATETIME() END,
              ApprovedByUserId=CASE WHEN @CanModerate=1 THEN ApprovedByUserId ELSE NULL END,
              ReviewedAt=CASE WHEN @CanModerate=1 THEN ReviewedAt ELSE NULL END,
              ReviewNote=CASE WHEN @CanModerate=1 THEN ReviewNote ELSE NULL END
          WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1);`);
        await this.audit(tx, identity.userId, "UPDATE", id, title, ipAddress);
        if (!canModerate)
          await this.notifyReviewers(tx, id, identity.userId, true);

        const item = await this.readPost(tx, identity, id);
        if (!item) throw new NotFoundException("Không tìm thấy bài đăng.");
        return item;
      },
    );
  }

  public async remove(
    identity: AuthenticatedIdentity,
    id: number,
    ipAddress: string | undefined,
  ): Promise<void> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    const canModerate = sqlScope(identity).canSeeAll;
    const pool = await this.sqlServer.getPool();
    await this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        const current = await this.editablePost(tx, identity, id, canModerate);
        await new sql.Request(tx)
          .input("Id", sql.Int, id)
          .input("ActorId", sql.Int, identity.userId)
          .input("CanModerate", sql.Bit, canModerate).query(`
          DELETE FROM dbo.HrmCommunicationReaction WHERE CommunicationId=@Id;
          DELETE FROM dbo.HrmCommunicationComment WHERE CommunicationId=@Id;
          DELETE FROM dbo.HrmCommunication
          WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1);`);
        await this.audit(
          tx,
          identity.userId,
          "DELETE",
          id,
          current.Title,
          ipAddress,
        );
      },
    );
  }

  public async moderate(
    identity: AuthenticatedIdentity,
    id: number,
    input: ModerateCommunicationDto,
    ipAddress: string | undefined,
  ): Promise<CommunicationModerationResult> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    if (!sqlScope(identity).canSeeAll) {
      throw new ForbiddenException("Bạn không có quyền duyệt bài đăng.");
    }
    const statusCode: CommunicationModerationResult["statusCode"] =
      input.approve ? "PUBLISHED" : "REJECTED";
    const note = optionalText(input.note);
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        const result = await new sql.Request(tx)
          .input("Id", sql.Int, id)
          .input("ActorId", sql.Int, identity.userId)
          .input("StatusCode", sql.NVarChar(20), statusCode)
          .input("IsPublished", sql.Bit, input.approve)
          .input("ReviewNote", sql.NVarChar(1000), note).query<{ Id: number }>(`
          UPDATE dbo.HrmCommunication
          SET StatusCode=@StatusCode,IsPublished=@IsPublished,
              ApprovedByUserId=@ActorId,ReviewedAt=SYSDATETIME(),ReviewNote=@ReviewNote,
              PublishedAt=CASE WHEN @IsPublished=1 THEN SYSDATETIME() ELSE PublishedAt END
          OUTPUT INSERTED.Id AS Id
          WHERE Id=@Id AND COALESCE(StatusCode,CASE WHEN IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)='PENDING';`);
        const post = result.recordset[0];
        if (!post) {
          throw new NotFoundException(
            "Bài đăng không tồn tại hoặc đã được xử lý.",
          );
        }
        await this.audit(
          tx,
          identity.userId,
          input.approve ? "APPROVE" : "REJECT",
          id,
          note,
          ipAddress,
        );
        if (input.approve) await this.notifyPublished(tx, id);
        await this.notifyAuthor(
          tx,
          id,
          input.approve ? "Bài đăng đã duyệt" : "Bài đăng bị từ chối",
          note,
          identity.userId,
        );
        return { id: post.Id, statusCode };
      },
    );
  }

  public async toggleLike(
    identity: AuthenticatedIdentity,
    id: number,
  ): Promise<CommunicationReactionResult> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.SERIALIZABLE,
      async (tx) => {
        await this.visiblePublishedPost(tx, identity, id, true);
        const existing = await new sql.Request(tx)
          .input("CommunicationId", sql.Int, id)
          .input("UserId", sql.Int, identity.userId).query<{ Id: number }>(`
          SELECT Id FROM dbo.HrmCommunicationReaction WITH (UPDLOCK,HOLDLOCK)
          WHERE CommunicationId=@CommunicationId AND UserId=@UserId;`);
        const liked = !existing.recordset[0];
        if (liked) {
          await new sql.Request(tx)
            .input("CommunicationId", sql.Int, id)
            .input("UserId", sql.Int, identity.userId)
            .query(`INSERT dbo.HrmCommunicationReaction(CommunicationId,UserId,ReactionCode)
                   VALUES(@CommunicationId,@UserId,'LIKE');`);
          await this.notifyAuthor(
            tx,
            id,
            "Tương tác mới",
            `${identity.displayName ?? identity.username} đã thích bài đăng của bạn.`,
            identity.userId,
          );
        } else {
          await new sql.Request(tx)
            .input("CommunicationId", sql.Int, id)
            .input("UserId", sql.Int, identity.userId)
            .query(`DELETE FROM dbo.HrmCommunicationReaction
                   WHERE CommunicationId=@CommunicationId AND UserId=@UserId;`);
        }
        const count = await new sql.Request(tx).input(
          "CommunicationId",
          sql.Int,
          id,
        ).query<{ Count: number }>(`SELECT COUNT(1) AS Count
          FROM dbo.HrmCommunicationReaction WHERE CommunicationId=@CommunicationId;`);
        return { liked, count: count.recordset[0]?.Count ?? 0 };
      },
    );
  }

  public async listComments(
    identity: AuthenticatedIdentity,
    id: number,
    query: ListCommunicationCommentsDto,
  ): Promise<CommunicationCommentItem[]> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    const pool = await this.sqlServer.getPool();
    await this.visiblePublishedPost(pool, identity, id);
    const result = await pool
      .request()
      .input("CommunicationId", sql.Int, id)
      .input("ActorId", sql.Int, identity.userId)
      .input("Take", sql.Int, query.take ?? 30)
      .query<CommentRow>(commentsSql);
    return result.recordset.map(toCommunicationCommentItem);
  }

  public async addComment(
    identity: AuthenticatedIdentity,
    id: number,
    input: CreateCommunicationCommentDto,
    ipAddress: string | undefined,
  ): Promise<CommunicationCommentItem> {
    assertId(id);
    await this.schemaReadiness.assertReady("communications");
    const body = requiredText(input.body, "Nội dung bình luận");
    const pool = await this.sqlServer.getPool();
    return this.transaction(
      pool,
      sql.ISOLATION_LEVEL.READ_COMMITTED,
      async (tx) => {
        await this.visiblePublishedPost(tx, identity, id, true);
        const result = await new sql.Request(tx)
          .input("CommunicationId", sql.Int, id)
          .input("AuthorUserId", sql.Int, identity.userId)
          .input("ActorId", sql.Int, identity.userId)
          .input("Body", sql.NVarChar(1500), body).query<CommentRow>(`
          INSERT dbo.HrmCommunicationComment(CommunicationId,AuthorUserId,Body)
          VALUES(@CommunicationId,@AuthorUserId,@Body);
          DECLARE @Id INT=CAST(SCOPE_IDENTITY() AS INT);
          SELECT m.Id,m.CommunicationId,m.AuthorUserId,u.DisplayName AS AuthorName,
            CASE WHEN p.AvatarContent IS NOT NULL
              THEN CONCAT('/api/v1/users/',u.Id,'/avatar?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
              ELSE p.AvatarUrl END AS AuthorAvatarUrl,
            p.JobTitle AS AuthorJobTitle,m.Body,m.CreatedAt,
            CAST(CASE WHEN m.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS bit) AS IsMine
          FROM dbo.HrmCommunicationComment m
          INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId
          LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
          WHERE m.Id=@Id;`);
        const row = result.recordset[0];
        if (!row) throw new Error("Không thể tạo bình luận.");
        await this.audit(tx, identity.userId, "COMMENT", id, body, ipAddress);
        await this.notifyAuthor(
          tx,
          id,
          "Bình luận mới",
          `${identity.displayName ?? identity.username}: ${body}`,
          identity.userId,
        );
        return toCommunicationCommentItem(row);
      },
    );
  }

  private async readPost(
    parent: QueryParent,
    identity: AuthenticatedIdentity,
    id: number,
  ): Promise<CommunicationListItem | undefined> {
    const result = await requestFor(parent)
      .input("Id", sql.Int, id)
      .input("ActorId", sql.Int, identity.userId)
      .query<CommunicationRow>(`${postSelectSql} WHERE c.Id=@Id;`);
    const row = result.recordset[0];
    return row ? toCommunicationListItem(row) : undefined;
  }

  private async editablePost(
    parent: QueryParent,
    identity: AuthenticatedIdentity,
    id: number,
    canModerate: boolean,
  ): Promise<EditableRow> {
    const result = await requestFor(parent)
      .input("Id", sql.Int, id)
      .input("ActorId", sql.Int, identity.userId)
      .input("CanModerate", sql.Bit, canModerate).query<EditableRow>(`
        SELECT TOP 1 Id,AuthorUserId,Category,ScopeCode,DepartmentId,Title,IsPinned
        FROM dbo.HrmCommunication WITH (UPDLOCK,HOLDLOCK)
        WHERE Id=@Id AND (AuthorUserId=@ActorId OR @CanModerate=1);`);
    const row = result.recordset[0];
    if (!row) throw new NotFoundException("Không tìm thấy bài đăng.");
    return row;
  }

  private async visiblePublishedPost(
    parent: QueryParent,
    identity: AuthenticatedIdentity,
    id: number,
    lockForMutation = false,
  ): Promise<void> {
    const scope = sqlScope(identity);
    const hint = lockForMutation ? " WITH (UPDLOCK,HOLDLOCK)" : "";
    const result = await requestFor(parent)
      .input("CommunicationId", sql.Int, id)
      .input("ActorId", sql.Int, identity.userId)
      .input("DepartmentId", sql.Int, identity.departmentId ?? null)
      .input("IsManager", sql.Bit, scope.isManager).query<{ Id: number }>(`
        SELECT TOP 1 c.Id
        FROM dbo.HrmCommunication c${hint}
        WHERE c.Id=@CommunicationId AND c.IsPublished=1
          AND (c.ScopeCode='ALL'
            OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
            OR (c.ScopeCode='MANAGER' AND @IsManager=1)
            OR c.AuthorUserId=@ActorId);`);
    if (!result.recordset[0]) {
      throw new NotFoundException(
        "Không tìm thấy bài đăng hoặc bạn không có quyền xem.",
      );
    }
  }

  private async audit(
    tx: sql.Transaction,
    userId: number,
    actionCode: string,
    id: number,
    detail: string | null,
    ipAddress: string | undefined,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("UserId", sql.Int, userId)
      .input("ActionCode", sql.NVarChar(80), actionCode)
      .input("EntityId", sql.NVarChar(80), String(id))
      .input("Detail", sql.NVarChar(sql.MAX), detail)
      .input("IpAddress", sql.NVarChar(80), cleanIp(ipAddress))
      .query(`INSERT dbo.HrmAuditLog(UserId,ActionCode,EntityType,EntityId,Detail,IpAddress)
              VALUES(@UserId,@ActionCode,'HrmCommunication',@EntityId,@Detail,@IpAddress);`);
  }

  private async notifyPublished(
    tx: sql.Transaction,
    id: number,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("CommunicationId", sql.Int, id)
      .input("LinkUrl", sql.NVarChar(500), communicationsLink).query(`
        INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
        SELECT u.Id,LEFT(CONCAT(N'Bài đăng mới: ',c.Title),200),
          N'Có bài đăng mới trong truyền thông nội bộ.',@LinkUrl
        FROM dbo.HrmCommunication c
        INNER JOIN dbo.HrmUserAccount u ON u.IsActive=1 AND u.Id<>c.AuthorUserId
          AND (c.ScopeCode='ALL'
            OR (c.ScopeCode='DEPARTMENT' AND u.DepartmentId=c.DepartmentId)
            OR (c.ScopeCode='MANAGER' AND u.RoleCode IN ('MANAGER','HR','DIRECTOR','ADMIN')))
        WHERE c.Id=@CommunicationId AND c.IsPublished=1;`);
  }

  private async notifyReviewers(
    tx: sql.Transaction,
    id: number,
    actorId: number,
    wasUpdated: boolean,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("CommunicationId", sql.Int, id)
      .input("ActorId", sql.Int, actorId)
      .input(
        "Label",
        sql.NVarChar(200),
        wasUpdated ? "Bài đăng đã sửa, chờ duyệt" : "Bài đăng chờ duyệt",
      )
      .input(
        "Message",
        sql.NVarChar(1000),
        wasUpdated
          ? "Bài đăng vừa được tác giả cập nhật và cần duyệt lại."
          : "Có bài đăng mới cần duyệt.",
      )
      .input("LinkUrl", sql.NVarChar(500), communicationsLink).query(`
        INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
        SELECT u.Id,LEFT(CONCAT(@Label,N': ',c.Title),200),LEFT(@Message,1000),@LinkUrl
        FROM dbo.HrmCommunication c
        INNER JOIN dbo.HrmUserAccount u ON u.IsActive=1
          AND u.RoleCode IN ('ADMIN','HR','DIRECTOR') AND u.Id<>@ActorId
        WHERE c.Id=@CommunicationId;`);
  }

  private async notifyAuthor(
    tx: sql.Transaction,
    id: number,
    label: string,
    message: string | null,
    actorId: number,
  ): Promise<void> {
    await new sql.Request(tx)
      .input("CommunicationId", sql.Int, id)
      .input("ActorId", sql.Int, actorId)
      .input("Label", sql.NVarChar(200), label)
      .input("Message", sql.NVarChar(1000), message ?? label)
      .input("LinkUrl", sql.NVarChar(500), communicationsLink).query(`
        INSERT dbo.HrmNotification(UserId,Title,Message,LinkUrl)
        SELECT u.Id,LEFT(CONCAT(@Label,N': ',c.Title),200),LEFT(@Message,1000),@LinkUrl
        FROM dbo.HrmCommunication c
        INNER JOIN dbo.HrmUserAccount u ON u.IsActive=1
          AND u.Id=c.AuthorUserId AND u.Id<>@ActorId
        WHERE c.Id=@CommunicationId;`);
  }

  private async transaction<T>(
    pool: sql.ConnectionPool,
    isolationLevel: number,
    work: (tx: sql.Transaction) => Promise<T>,
  ): Promise<T> {
    const tx = new sql.Transaction(pool);
    let committed = false;
    await tx.begin(isolationLevel);
    try {
      const result = await work(tx);
      await tx.commit();
      committed = true;
      return result;
    } catch (error: unknown) {
      if (!committed) await tx.rollback().catch(() => undefined);
      throw error;
    }
  }
}

const postSelectSql = `
  SELECT c.Id,c.AuthorUserId,u.DisplayName AS AuthorName,
    CASE WHEN authorProfile.AvatarContent IS NOT NULL
      THEN CONCAT('/api/v1/users/',u.Id,'/avatar?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(authorProfile.UpdatedAt,authorProfile.CreatedAt)))
      ELSE authorProfile.AvatarUrl END AS AuthorAvatarUrl,
    authorProfile.JobTitle AS AuthorJobTitle,c.Category,c.ScopeCode,c.DepartmentId,c.Title,c.Body,
    c.AttachmentName,c.AttachmentContentType,c.IsPinned,
    COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END) AS StatusCode,
    COALESCE(c.SubmittedAt,c.PublishedAt) AS SubmittedAt,c.PublishedAt,
    (SELECT COUNT(1) FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id) AS LikeCount,
    (SELECT COUNT(1) FROM dbo.HrmCommunicationComment m WHERE m.CommunicationId=c.Id AND m.IsDeleted=0) AS CommentCount,
    CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.HrmCommunicationReaction r WHERE r.CommunicationId=c.Id AND r.UserId=@ActorId)
      THEN 1 ELSE 0 END AS bit) AS LikedByCurrentUser,
    CAST(CASE WHEN c.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS bit) AS IsMine
  FROM dbo.HrmCommunication c
  INNER JOIN dbo.HrmUserAccount u ON u.Id=c.AuthorUserId
  LEFT JOIN dbo.HrmEmployeeProfile authorProfile ON authorProfile.UserId=u.Id`;

const listSql = `${postSelectSql}
  WHERE ((c.IsPublished=1 AND (c.ScopeCode='ALL'
      OR (c.ScopeCode='DEPARTMENT' AND c.DepartmentId=@DepartmentId)
      OR (c.ScopeCode='MANAGER' AND @IsManager=1)))
    OR c.AuthorUserId=@ActorId
    OR (@CanModerate=1 AND COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)='PENDING'))
    AND (@Status='' OR COALESCE(c.StatusCode,CASE WHEN c.IsPublished=1 THEN 'PUBLISHED' ELSE 'PENDING' END)=@Status)
    AND (@Keyword='' OR c.Title LIKE '%' + @Keyword + '%' OR c.Body LIKE '%' + @Keyword + '%')
    AND (@Category='' OR c.Category=@Category)
  ORDER BY CASE WHEN COALESCE(c.StatusCode,'PUBLISHED')='PENDING' THEN 0 ELSE 1 END,
    c.IsPinned DESC,COALESCE(c.SubmittedAt,c.PublishedAt) DESC;`;

const commentsSql = `
  ;WITH newest AS (
    SELECT TOP (@Take) m.Id,m.CommunicationId,m.AuthorUserId,m.Body,m.CreatedAt
    FROM dbo.HrmCommunicationComment m
    WHERE m.CommunicationId=@CommunicationId AND m.IsDeleted=0
    ORDER BY m.CreatedAt DESC,m.Id DESC
  )
  SELECT m.Id,m.CommunicationId,m.AuthorUserId,u.DisplayName AS AuthorName,
    CASE WHEN p.AvatarContent IS NOT NULL
      THEN CONCAT('/api/v1/users/',u.Id,'/avatar?v=',DATEDIFF_BIG(MILLISECOND,'19700101',COALESCE(p.UpdatedAt,p.CreatedAt)))
      ELSE p.AvatarUrl END AS AuthorAvatarUrl,
    p.JobTitle AS AuthorJobTitle,m.Body,m.CreatedAt,
    CAST(CASE WHEN m.AuthorUserId=@ActorId THEN 1 ELSE 0 END AS bit) AS IsMine
  FROM newest m
  INNER JOIN dbo.HrmUserAccount u ON u.Id=m.AuthorUserId
  LEFT JOIN dbo.HrmEmployeeProfile p ON p.UserId=u.Id
  ORDER BY m.CreatedAt ASC,m.Id ASC;`;

export function toCommunicationListItem(
  row: CommunicationRow,
): CommunicationListItem {
  return {
    id: row.Id,
    authorUserId: row.AuthorUserId,
    authorName: row.AuthorName,
    authorAvatarUrl: row.AuthorAvatarUrl,
    authorJobTitle: row.AuthorJobTitle,
    category: row.Category,
    scopeCode: row.ScopeCode,
    departmentId: row.DepartmentId,
    title: row.Title,
    body: row.Body,
    attachmentName: row.AttachmentName,
    attachmentContentType: row.AttachmentContentType,
    isPinned: asBool(row.IsPinned),
    statusCode: row.StatusCode,
    submittedAt: row.SubmittedAt,
    publishedAt: row.PublishedAt,
    likeCount: row.LikeCount,
    commentCount: row.CommentCount,
    likedByCurrentUser: asBool(row.LikedByCurrentUser),
    isMine: asBool(row.IsMine),
  };
}

export function toCommunicationCommentItem(
  row: CommentRow,
): CommunicationCommentItem {
  return {
    id: row.Id,
    communicationId: row.CommunicationId,
    authorUserId: row.AuthorUserId,
    authorName: row.AuthorName,
    authorAvatarUrl: row.AuthorAvatarUrl,
    authorJobTitle: row.AuthorJobTitle,
    body: row.Body,
    createdAt: row.CreatedAt,
    isMine: asBool(row.IsMine),
  };
}

function assertId(id: number): void {
  if (!Number.isSafeInteger(id) || id < 1) {
    throw new BadRequestException("Mã bài đăng không hợp lệ.");
  }
}

function normalizeScope(value: string): ScopeCode {
  const normalized = value.trim().toUpperCase();
  if (
    normalized === "ALL" ||
    normalized === "DEPARTMENT" ||
    normalized === "MANAGER"
  ) {
    return normalized;
  }
  throw new BadRequestException("Phạm vi bài đăng không hợp lệ.");
}

function assertScopePermission(
  identity: AuthenticatedIdentity,
  scopeCode: ScopeCode,
): void {
  const scope = sqlScope(identity);
  if (scopeCode === "ALL" && !scope.canSeeAll) {
    throw new ForbiddenException(
      "Chỉ HR, Ban giám đốc hoặc quản trị viên mới có thể đăng toàn công ty.",
    );
  }
  if (scopeCode === "MANAGER" && !scope.isManager) {
    throw new ForbiddenException(
      "Chỉ quản lý hoặc cấp có quyền cao hơn mới có thể đăng cho nhóm quản lý.",
    );
  }
}

function departmentForNewPost(
  scopeCode: ScopeCode,
  identity: AuthenticatedIdentity,
): number | null {
  if (scopeCode !== "DEPARTMENT") return null;
  if (!identity.departmentId) {
    throw new BadRequestException(
      "Bạn chưa được gán phòng ban nên không thể đăng trong phạm vi phòng ban.",
    );
  }
  return identity.departmentId;
}

function departmentForUpdatedPost(
  scopeCode: ScopeCode,
  identity: AuthenticatedIdentity,
  current: EditableRow,
): number | null {
  if (scopeCode !== "DEPARTMENT") return null;
  const departmentId =
    current.AuthorUserId === identity.userId
      ? identity.departmentId
      : current.DepartmentId;
  if (!departmentId) {
    throw new BadRequestException(
      "Bài đăng chưa có phòng ban hợp lệ. Hãy chọn phạm vi toàn công ty hoặc cập nhật phòng ban trước.",
    );
  }
  return departmentId;
}

function requiredText(value: string, label: string): string {
  const text = value?.trim();
  if (!text) throw new BadRequestException(`${label} không được để trống.`);
  return text;
}

function optionalText(value: string | undefined): string | null {
  const text = value?.trim();
  return text ? text : null;
}

function cleanIp(value: string | undefined): string | null {
  const ip = value?.trim();
  return ip ? ip.slice(0, 80) : null;
}

function asBool(value: boolean | number): boolean {
  return value === true || value === 1;
}

function requestFor(parent: QueryParent): sql.Request {
  if (parent instanceof sql.Transaction) return new sql.Request(parent);
  return new sql.Request(parent);
}
