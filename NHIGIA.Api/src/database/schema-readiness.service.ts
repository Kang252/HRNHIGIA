import {
  Injectable,
  Logger,
  ServiceUnavailableException,
} from "@nestjs/common";
import { SqlServerService } from "./sql-server.service";

/**
 * Feature names intentionally stay internal to the API.  Controllers and
 * callers must select from this allow-list; it is never built from a request
 * parameter or interpolated into SQL.
 */
export type ApiSchemaFeature =
  "notifications" | "communications" | "attendance" | "procedures";

export interface SchemaProbeRow {
  Requirement: string;
  IsPresent: boolean | number | null;
}

export interface FeatureSchemaHealth {
  feature: ApiSchemaFeature;
  ready: boolean;
  missing: string[];
  checkedAt: Date;
  /** True when SQL could not be queried, rather than a table/column missing. */
  unavailable: boolean;
}

interface CachedSchemaHealth {
  value: FeatureSchemaHealth;
  expiresAt: number;
}

const readinessCacheTtlMs = 30_000;

const featureLabels: Readonly<Record<ApiSchemaFeature, string>> = {
  notifications: "Thông báo",
  communications: "Truyền thông nội bộ",
  attendance: "Chấm công",
  procedures: "Thủ tục",
};

/**
 * These names mirror the static probes below.  Keep the expectations in one
 * place so a missing result row is treated as unsafe instead of accidentally
 * being considered ready.
 */
export const featureSchemaRequirements: Readonly<
  Record<ApiSchemaFeature, readonly string[]>
> = {
  notifications: [
    "dbo.HrmNotification",
    "dbo.HrmNotification.Id",
    "dbo.HrmNotification.UserId",
    "dbo.HrmNotification.Title",
    "dbo.HrmNotification.Message",
    "dbo.HrmNotification.LinkUrl",
    "dbo.HrmNotification.IsRead",
    "dbo.HrmNotification.CreatedAt",
  ],
  communications: [
    "dbo.HrmCommunication",
    "dbo.HrmCommunication.Id",
    "dbo.HrmCommunication.AuthorUserId",
    "dbo.HrmCommunication.Category",
    "dbo.HrmCommunication.ScopeCode",
    "dbo.HrmCommunication.DepartmentId",
    "dbo.HrmCommunication.Title",
    "dbo.HrmCommunication.Body",
    "dbo.HrmCommunication.AttachmentName",
    "dbo.HrmCommunication.AttachmentContentType",
    "dbo.HrmCommunication.IsPinned",
    "dbo.HrmCommunication.IsPublished",
    "dbo.HrmCommunication.StatusCode",
    "dbo.HrmCommunication.SubmittedAt",
    "dbo.HrmCommunication.PublishedAt",
    "dbo.HrmCommunication.ApprovedByUserId",
    "dbo.HrmCommunication.ReviewedAt",
    "dbo.HrmCommunication.ReviewNote",
    "dbo.HrmCommunicationReaction",
    "dbo.HrmCommunicationReaction.Id",
    "dbo.HrmCommunicationReaction.CommunicationId",
    "dbo.HrmCommunicationReaction.UserId",
    "dbo.HrmCommunicationReaction.ReactionCode",
    "dbo.HrmCommunicationReaction.CreatedAt",
    "dbo.HrmCommunicationComment",
    "dbo.HrmCommunicationComment.Id",
    "dbo.HrmCommunicationComment.CommunicationId",
    "dbo.HrmCommunicationComment.AuthorUserId",
    "dbo.HrmCommunicationComment.Body",
    "dbo.HrmCommunicationComment.IsDeleted",
    "dbo.HrmCommunicationComment.CreatedAt",
    "dbo.HrmUserAccount",
    "dbo.HrmUserAccount.Id",
    "dbo.HrmUserAccount.DisplayName",
    "dbo.HrmUserAccount.IsActive",
    "dbo.HrmUserAccount.RoleCode",
    "dbo.HrmUserAccount.DepartmentId",
    "dbo.HrmEmployeeProfile",
    "dbo.HrmEmployeeProfile.UserId",
    "dbo.HrmEmployeeProfile.AvatarContent",
    "dbo.HrmEmployeeProfile.AvatarUrl",
    "dbo.HrmEmployeeProfile.JobTitle",
    "dbo.HrmEmployeeProfile.UpdatedAt",
    "dbo.HrmEmployeeProfile.CreatedAt",
    "dbo.HrmNotification",
    "dbo.HrmNotification.UserId",
    "dbo.HrmNotification.Title",
    "dbo.HrmNotification.Message",
    "dbo.HrmNotification.LinkUrl",
    "dbo.HrmAuditLog",
    "dbo.HrmAuditLog.UserId",
    "dbo.HrmAuditLog.ActionCode",
    "dbo.HrmAuditLog.EntityType",
    "dbo.HrmAuditLog.EntityId",
    "dbo.HrmAuditLog.Detail",
    "dbo.HrmAuditLog.IpAddress",
  ],
  attendance: [
    "dbo.HrmAttendanceEvent",
    "dbo.HrmAttendanceEvent.UserId",
    "dbo.HrmAttendanceEvent.CheckTime",
    "dbo.HrmAttendanceEvent.ReceivedAt",
    "dbo.HrmAttendancePeriod",
    "dbo.HrmAttendancePeriod.Period",
    "dbo.HrmAttendancePeriod.StatusCode",
    "dbo.HrmAttendancePeriod.LockedByUserId",
    "dbo.HrmAttendancePeriod.LockedAt",
    "dbo.HrmAttendancePeriod.UnlockReason",
    "dbo.HrmAttendancePeriodConfirmation",
    "dbo.HrmAttendancePeriodConfirmation.Period",
    "dbo.HrmAttendancePeriodConfirmation.UserId",
    "dbo.HrmAttendancePeriodConfirmation.StatusCode",
    "dbo.HrmAttendancePeriodConfirmation.SubmittedAt",
    "dbo.HrmAttendanceAdjustment",
    "dbo.HrmAttendanceAdjustment.Id",
    "dbo.HrmAttendanceAdjustment.UserId",
    "dbo.HrmAttendanceAdjustment.WorkDate",
    "dbo.HrmAttendanceAdjustment.RequestedCheckIn",
    "dbo.HrmAttendanceAdjustment.RequestedCheckOut",
    "dbo.HrmAttendanceAdjustment.StatusCode",
    "dbo.HrmAttendanceAdjustment.ReviewedAt",
    "dbo.HrmEmployeeSchedule",
    "dbo.HrmEmployeeSchedule.Id",
    "dbo.HrmEmployeeSchedule.UserId",
    "dbo.HrmEmployeeSchedule.ShiftName",
    "dbo.HrmEmployeeSchedule.StartTime",
    "dbo.HrmEmployeeSchedule.EndTime",
    "dbo.HrmEmployeeSchedule.BreakMinutes",
    "dbo.HrmEmployeeSchedule.GraceMinutes",
    "dbo.HrmEmployeeSchedule.WorkDaysMask",
    "dbo.HrmEmployeeSchedule.EffectiveFrom",
    "dbo.HrmEmployeeSchedule.EffectiveTo",
    "dbo.HrmEmployeeSchedule.StatusCode",
    "dbo.HrmLeaveRequest",
    "dbo.HrmLeaveRequest.Id",
    "dbo.HrmLeaveRequest.RequestCode",
    "dbo.HrmLeaveRequest.UserId",
    "dbo.HrmLeaveRequest.LeaveType",
    "dbo.HrmLeaveRequest.StartDate",
    "dbo.HrmLeaveRequest.EndDate",
    "dbo.HrmLeaveRequest.SessionCode",
    "dbo.HrmLeaveRequest.StatusCode",
    "dbo.HrmRemoteWorkPlan",
    "dbo.HrmRemoteWorkPlan.Id",
    "dbo.HrmRemoteWorkPlan.Mode",
    "dbo.HrmRemoteWorkPlan.WindowStart",
    "dbo.HrmRemoteWorkPlan.WindowEnd",
    "dbo.HrmRemoteWorkPlan.BreakMinutes",
    "dbo.HrmRemoteWorkPlan.IsFlexible",
    "dbo.HrmRemoteWorkPlan.RequiredMinutes",
    "dbo.HrmRemotePunch",
    "dbo.HrmRemotePunch.Id",
    "dbo.HrmRemotePunch.PlanId",
    "dbo.HrmRemotePunch.UserId",
    "dbo.HrmRemotePunch.Kind",
    "dbo.HrmRemotePunch.CheckTime",
    "dbo.HrmRemotePunch.StatusCode",
    "dbo.HrmRemotePunch.IncludedAt",
    "dbo.HrmAuditLog",
    "dbo.HrmAuditLog.UserId",
    "dbo.HrmAuditLog.ActionCode",
    "dbo.HrmAuditLog.EntityType",
    "dbo.HrmAuditLog.EntityId",
    "dbo.HrmAuditLog.Detail",
    "dbo.HrmAuditLog.IpAddress",
    "dbo.HrmUserAccount",
    "dbo.HrmUserAccount.Id",
    "dbo.HrmUserAccount.DisplayName",
    "dbo.HrmUserAccount.DepartmentId",
    "dbo.HrmUserAccount.IsActive",
    "dbo.HrmUserAccount.RoleCode",
    "dbo.HrmEmployeeProfile",
    "dbo.HrmEmployeeProfile.UserId",
    "dbo.HrmEmployeeProfile.EmployeeCode",
    "dbo.HrmEmployeeProfile.JobTitle",
    "dbo.HrmDepartment",
    "dbo.HrmDepartment.Id",
    "dbo.HrmDepartment.Name",
  ],
  procedures: [
    "dbo.HrmLeaveRequest",
    "dbo.HrmLeaveRequest.Id",
    "dbo.HrmLeaveRequest.RequestCode",
    "dbo.HrmLeaveRequest.UserId",
    "dbo.HrmLeaveRequest.LeaveType",
    "dbo.HrmLeaveRequest.StartDate",
    "dbo.HrmLeaveRequest.EndDate",
    "dbo.HrmLeaveRequest.SessionCode",
    "dbo.HrmLeaveRequest.HandoverTo",
    "dbo.HrmLeaveRequest.Reason",
    "dbo.HrmLeaveRequest.AttachmentName",
    "dbo.HrmLeaveRequest.AttachmentContent",
    "dbo.HrmLeaveRequest.StatusCode",
    "dbo.HrmLeaveRequest.ManagerNote",
    "dbo.HrmLeaveRequest.HrNote",
    "dbo.HrmLeaveRequest.CreatedAt",
    "dbo.HrmLeaveRequest.UpdatedAt",
    "dbo.HrmWorkItem",
    "dbo.HrmWorkItem.Id",
    "dbo.HrmWorkItem.Kind",
    "dbo.HrmWorkItem.Title",
    "dbo.HrmWorkItem.EmployeeId",
    "dbo.HrmWorkItem.CreatedBy",
    "dbo.HrmWorkItem.DepartmentId",
    "dbo.HrmWorkItem.DueDate",
    "dbo.HrmWorkItem.StartAt",
    "dbo.HrmWorkItem.EndAt",
    "dbo.HrmWorkItem.Status",
    "dbo.HrmWorkItem.Reference",
    "dbo.HrmWorkItem.Category",
    "dbo.HrmWorkItem.Description",
    "dbo.HrmWorkItem.Location",
    "dbo.HrmWorkItem.Destination",
    "dbo.HrmWorkItem.Priority",
    "dbo.HrmWorkItem.LastActionNote",
    "dbo.HrmWorkItem.CreatedAt",
    "dbo.HrmWorkItem.UpdatedAt",
    "dbo.HrmWorkItemParticipant",
    "dbo.HrmWorkItemParticipant.WorkItemId",
    "dbo.HrmWorkItemParticipant.UserId",
    "dbo.HrmUserAccount",
    "dbo.HrmUserAccount.Id",
    "dbo.HrmUserAccount.DisplayName",
    "dbo.HrmUserAccount.DepartmentId",
    "dbo.HrmDepartment",
    "dbo.HrmDepartment.Id",
    "dbo.HrmDepartment.Name",
  ],
};

/**
 * Every probe is an internal, parameterless SQL literal.  Do not replace this
 * with a request-built SQL string: schema checks must never accept identifier
 * input from an API caller.
 */
export const schemaProbeSql: Readonly<Record<ApiSchemaFeature, string>> = {
  notifications: `
    SELECT N'dbo.HrmNotification' AS Requirement,CAST(CASE WHEN OBJECT_ID(N'dbo.HrmNotification',N'U') IS NULL THEN 0 ELSE 1 END AS bit) AS IsPresent
    UNION ALL SELECT N'dbo.HrmNotification.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.Title',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'Title') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.Message',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'Message') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.LinkUrl',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'LinkUrl') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.IsRead',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'IsRead') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit);`,
  communications: `
    SELECT N'dbo.HrmCommunication' AS Requirement,CAST(CASE WHEN OBJECT_ID(N'dbo.HrmCommunication',N'U') IS NULL THEN 0 ELSE 1 END AS bit) AS IsPresent
    UNION ALL SELECT N'dbo.HrmCommunication.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.AuthorUserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'AuthorUserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.Category',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'Category') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.ScopeCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'ScopeCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.DepartmentId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'DepartmentId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.Title',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'Title') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.Body',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'Body') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.AttachmentName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'AttachmentName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.AttachmentContentType',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'AttachmentContentType') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.IsPinned',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'IsPinned') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.IsPublished',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'IsPublished') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.SubmittedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'SubmittedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.PublishedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'PublishedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.ApprovedByUserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'ApprovedByUserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.ReviewedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'ReviewedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunication.ReviewNote',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunication',N'ReviewNote') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmCommunicationReaction',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationReaction',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction.CommunicationId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationReaction',N'CommunicationId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationReaction',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction.ReactionCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationReaction',N'ReactionCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationReaction.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationReaction',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmCommunicationComment',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.CommunicationId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'CommunicationId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.AuthorUserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'AuthorUserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.Body',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'Body') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.IsDeleted',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'IsDeleted') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmCommunicationComment.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmCommunicationComment',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmUserAccount',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DisplayName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DisplayName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.IsActive',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'IsActive') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.RoleCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'RoleCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DepartmentId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DepartmentId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmEmployeeProfile',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.AvatarContent',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'AvatarContent') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.AvatarUrl',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'AvatarUrl') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.JobTitle',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'JobTitle') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.UpdatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'UpdatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmNotification',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.Title',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'Title') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.Message',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'Message') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmNotification.LinkUrl',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmNotification',N'LinkUrl') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAuditLog',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.ActionCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'ActionCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.EntityType',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'EntityType') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.EntityId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'EntityId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.Detail',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'Detail') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.IpAddress',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'IpAddress') IS NULL THEN 0 ELSE 1 END AS bit);`,
  attendance: `
    SELECT N'dbo.HrmAttendanceEvent' AS Requirement,CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAttendanceEvent',N'U') IS NULL THEN 0 ELSE 1 END AS bit) AS IsPresent
    UNION ALL SELECT N'dbo.HrmAttendanceEvent.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceEvent',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceEvent.CheckTime',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceEvent',N'CheckTime') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceEvent.ReceivedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceEvent',N'ReceivedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAttendancePeriod',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod.Period',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriod',N'Period') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriod',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod.LockedByUserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriod',N'LockedByUserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod.LockedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriod',N'LockedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriod.UnlockReason',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriod',N'UnlockReason') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriodConfirmation',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAttendancePeriodConfirmation',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriodConfirmation.Period',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriodConfirmation',N'Period') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriodConfirmation.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriodConfirmation',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriodConfirmation.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriodConfirmation',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendancePeriodConfirmation.SubmittedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendancePeriodConfirmation',N'SubmittedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAttendanceAdjustment',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.WorkDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'WorkDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.RequestedCheckIn',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'RequestedCheckIn') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.RequestedCheckOut',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'RequestedCheckOut') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAttendanceAdjustment.ReviewedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAttendanceAdjustment',N'ReviewedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmEmployeeSchedule',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.ShiftName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'ShiftName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.StartTime',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'StartTime') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.EndTime',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'EndTime') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.BreakMinutes',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'BreakMinutes') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.GraceMinutes',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'GraceMinutes') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.WorkDaysMask',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'WorkDaysMask') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.EffectiveFrom',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'EffectiveFrom') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.EffectiveTo',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'EffectiveTo') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeSchedule.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeSchedule',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmLeaveRequest',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.RequestCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'RequestCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.LeaveType',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'LeaveType') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.StartDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'StartDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.EndDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'EndDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.SessionCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'SessionCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmRemoteWorkPlan',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.Mode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'Mode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.WindowStart',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'WindowStart') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.WindowEnd',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'WindowEnd') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.BreakMinutes',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'BreakMinutes') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.IsFlexible',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'IsFlexible') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemoteWorkPlan.RequiredMinutes',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemoteWorkPlan',N'RequiredMinutes') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmRemotePunch',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.PlanId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'PlanId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.Kind',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'Kind') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.CheckTime',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'CheckTime') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmRemotePunch.IncludedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmRemotePunch',N'IncludedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmAuditLog',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.ActionCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'ActionCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.EntityType',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'EntityType') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.EntityId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'EntityId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.Detail',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'Detail') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmAuditLog.IpAddress',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmAuditLog',N'IpAddress') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmUserAccount',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DisplayName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DisplayName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DepartmentId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DepartmentId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.IsActive',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'IsActive') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.RoleCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'RoleCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmEmployeeProfile',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.EmployeeCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'EmployeeCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmEmployeeProfile.JobTitle',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmEmployeeProfile',N'JobTitle') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmDepartment',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmDepartment',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment.Name',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmDepartment',N'Name') IS NULL THEN 0 ELSE 1 END AS bit);`,
  procedures: `
    SELECT N'dbo.HrmLeaveRequest' AS Requirement,CAST(CASE WHEN OBJECT_ID(N'dbo.HrmLeaveRequest',N'U') IS NULL THEN 0 ELSE 1 END AS bit) AS IsPresent
    UNION ALL SELECT N'dbo.HrmLeaveRequest.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.RequestCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'RequestCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.LeaveType',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'LeaveType') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.StartDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'StartDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.EndDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'EndDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.SessionCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'SessionCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.HandoverTo',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'HandoverTo') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.Reason',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'Reason') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.AttachmentName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'AttachmentName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.AttachmentContent',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'AttachmentContent') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.StatusCode',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'StatusCode') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.ManagerNote',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'ManagerNote') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.HrNote',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'HrNote') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmLeaveRequest.UpdatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmLeaveRequest',N'UpdatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmWorkItem',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Kind',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Kind') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Title',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Title') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.EmployeeId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'EmployeeId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.CreatedBy',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'CreatedBy') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.DepartmentId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'DepartmentId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.DueDate',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'DueDate') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.StartAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'StartAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.EndAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'EndAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Status',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Status') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Reference',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Reference') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Category',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Category') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Description',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Description') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Location',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Location') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Destination',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Destination') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.Priority',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'Priority') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.LastActionNote',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'LastActionNote') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.CreatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'CreatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItem.UpdatedAt',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItem',N'UpdatedAt') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItemParticipant',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmWorkItemParticipant',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItemParticipant.WorkItemId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItemParticipant',N'WorkItemId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmWorkItemParticipant.UserId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmWorkItemParticipant',N'UserId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmUserAccount',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DisplayName',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DisplayName') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmUserAccount.DepartmentId',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmUserAccount',N'DepartmentId') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment',CAST(CASE WHEN OBJECT_ID(N'dbo.HrmDepartment',N'U') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment.Id',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmDepartment',N'Id') IS NULL THEN 0 ELSE 1 END AS bit)
    UNION ALL SELECT N'dbo.HrmDepartment.Name',CAST(CASE WHEN COL_LENGTH(N'dbo.HrmDepartment',N'Name') IS NULL THEN 0 ELSE 1 END AS bit);`,
};

/**
 * Converts a raw static SQL probe into an explicit feature health report.
 * The function is exported to make the expectation/missing-column contract
 * independently testable without a SQL Server connection.
 */
export function featureSchemaHealthFromRows(
  feature: ApiSchemaFeature,
  rows: readonly SchemaProbeRow[],
  checkedAt = new Date(),
): FeatureSchemaHealth {
  const present = new Set(
    rows
      .filter((row) => row.IsPresent === true || row.IsPresent === 1)
      .map((row) => row.Requirement),
  );
  const missing = featureSchemaRequirements[feature].filter(
    (requirement) => !present.has(requirement),
  );
  return {
    feature,
    ready: missing.length === 0,
    missing: [...missing],
    checkedAt,
    unavailable: false,
  };
}

@Injectable()
export class SchemaReadinessService {
  private readonly logger = new Logger(SchemaReadinessService.name);
  private readonly cache = new Map<ApiSchemaFeature, CachedSchemaHealth>();
  private readonly inFlight = new Map<
    ApiSchemaFeature,
    Promise<FeatureSchemaHealth>
  >();

  public constructor(private readonly sqlServer: SqlServerService) {}

  /**
   * Checks only the requested feature when it is first used.  The resulting
   * state is cached briefly to avoid adding a metadata query to every request.
   */
  public async inspect(
    feature: ApiSchemaFeature,
  ): Promise<FeatureSchemaHealth> {
    const cached = this.cache.get(feature);
    if (cached && cached.expiresAt > Date.now()) return cached.value;

    const pending = this.inFlight.get(feature);
    if (pending) return pending;

    const probe = this.probe(feature).finally(() => {
      this.inFlight.delete(feature);
    });
    this.inFlight.set(feature, probe);
    return probe;
  }

  /**
   * Intended for endpoint services before a feature query.  It deliberately
   * turns schema drift into a stable, actionable 503 instead of leaking a SQL
   * Server "invalid column" error to an employee.
   */
  public async assertReady(feature: ApiSchemaFeature): Promise<void> {
    const health = await this.inspect(feature);
    if (health.ready) return;

    throw new ServiceUnavailableException(
      schemaUnavailableMessage(feature, health),
    );
  }

  private async probe(feature: ApiSchemaFeature): Promise<FeatureSchemaHealth> {
    let health: FeatureSchemaHealth;
    try {
      const pool = await this.sqlServer.getPool();
      const result = await pool
        .request()
        .query<SchemaProbeRow>(schemaProbeSql[feature]);
      health = featureSchemaHealthFromRows(feature, result.recordset);
    } catch (error: unknown) {
      this.logger.warn(
        `Cannot verify ${feature} schema. errorType=${errorType(error)}`,
      );
      health = {
        feature,
        ready: false,
        missing: [],
        checkedAt: new Date(),
        unavailable: true,
      };
    }

    this.cache.set(feature, {
      value: health,
      expiresAt: Date.now() + readinessCacheTtlMs,
    });
    return health;
  }
}

export function schemaUnavailableMessage(
  feature: ApiSchemaFeature,
  health: FeatureSchemaHealth,
): string {
  const label = featureLabels[feature];
  if (health.unavailable) {
    return `Chưa thể kiểm tra schema cơ sở dữ liệu cho ${label}. Hãy kiểm tra kết nối SQL và áp dụng migration HRM trước khi dùng API này.`;
  }
  const shown = health.missing.slice(0, 8);
  const remainder = health.missing.length - shown.length;
  const missing =
    remainder > 0
      ? `${shown.join(", ")} và ${remainder} mục khác`
      : shown.join(", ");
  return `Schema cơ sở dữ liệu cho ${label} chưa sẵn sàng. Thiếu ${missing}. Hãy áp dụng migration HRM trước khi dùng API này.`;
}

function errorType(error: unknown): string {
  return error instanceof Error ? error.name : "UnknownError";
}
