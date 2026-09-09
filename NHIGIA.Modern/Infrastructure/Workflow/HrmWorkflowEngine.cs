using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure.Workflow
{
    public class HrmWorkflowEngine
    {
        private readonly IConfiguration _configuration;

        public HrmWorkflowEngine(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection OpenConnection()
        {
            return DatabaseConfiguration.OpenConnection(_configuration);
        }

        public List<WorkflowDefinitionModel> GetActiveDefinitions()
        {
            const string sql = @"
                SELECT d.Id, d.Code, d.Name, d.Description, d.Icon, d.IsActive,
                       COUNT(s.Id) AS StepCount
                FROM dbo.HrmWorkflowDefinition d
                LEFT JOIN dbo.HrmWorkflowStep s ON s.WorkflowId = d.Id
                WHERE d.IsActive = 1
                GROUP BY d.Id, d.Code, d.Name, d.Description, d.Icon, d.IsActive
                ORDER BY d.Id";
            using var connection = OpenConnection();
            return connection.Query<WorkflowDefinitionModel>(sql).ToList();
        }

        public WorkflowDefinitionModel GetDefinitionByCode(string code)
        {
            const string sql = @"
                SELECT Id, Code, Name, Description, Icon, IsActive
                FROM dbo.HrmWorkflowDefinition
                WHERE Code = @Code AND IsActive = 1";
            using var connection = OpenConnection();
            return connection.QuerySingleOrDefault<WorkflowDefinitionModel>(sql, new { Code = (code ?? string.Empty).Trim().ToUpperInvariant() });
        }

        public List<WorkflowStepModel> GetSteps(int workflowId)
        {
            const string sql = @"
                SELECT s.Id, s.WorkflowId, s.StepOrder, s.StepName, s.ApproverType,
                       s.SpecificUserId, u.DisplayName AS SpecificUserName,
                       s.ThresholdAmount, s.SlaHours, s.CanRejectToStart
                FROM dbo.HrmWorkflowStep s
                LEFT JOIN dbo.HrmUserAccount u ON u.Id = s.SpecificUserId
                WHERE s.WorkflowId = @WorkflowId
                ORDER BY s.StepOrder ASC";
            using var connection = OpenConnection();
            return connection.Query<WorkflowStepModel>(sql, new { WorkflowId = workflowId }).ToList();
        }

        public (bool Success, string Message, WorkflowInstanceModel Instance) CreateInstance(
            int requesterUserId,
            string workflowCode,
            string title,
            string description,
            decimal? amount,
            string payloadJson,
            string digitalSignature,
            string clientIp,
            string targetTable = null,
            long? targetRecordId = null)
        {
            using var connection = OpenConnection();
            using var tx = connection.BeginTransaction();
            try
            {
                var def = connection.QuerySingleOrDefault<WorkflowDefinitionModel>(
                    "SELECT Id, Code, Name, Description, Icon, IsActive FROM dbo.HrmWorkflowDefinition WHERE Code = @Code AND IsActive = 1",
                    new { Code = (workflowCode ?? string.Empty).Trim().ToUpperInvariant() }, tx);

                if (def == null)
                    return (false, $"Không tìm thấy quy trình có mã '{workflowCode}'", null);

                var steps = connection.Query<WorkflowStepModel>(
                    "SELECT Id, WorkflowId, StepOrder, StepName, ApproverType, SpecificUserId, ThresholdAmount, SlaHours FROM dbo.HrmWorkflowStep WHERE WorkflowId = @WorkflowId ORDER BY StepOrder ASC",
                    new { WorkflowId = def.Id }, tx).ToList();

                if (steps.Count == 0)
                    return (false, $"Quy trình '{def.Name}' chưa được cấu hình các bước phê duyệt.", null);

                var firstStep = steps[0];
                var requester = connection.QuerySingleOrDefault<HrmUserAccountModel>(
                    "SELECT Id, Username, DisplayName, RoleCode, DepartmentId, SupervisorUserId FROM dbo.HrmUserAccount WHERE Id = @Id",
                    new { Id = requesterUserId }, tx);

                if (requester == null)
                    return (false, "Người tạo đơn không hợp lệ.", null);

                int? initialApproverId = ResolveApproverId(connection, tx, firstStep, requester);

                const string insertInstanceSql = @"
                    INSERT INTO dbo.HrmWorkflowInstance (
                        WorkflowId, RequesterUserId, CurrentStepId, CurrentApproverId,
                        StatusCode, Title, Description, Amount, TargetTable, TargetRecordId,
                        PayloadJson, DigitalSignature, CreatedAt
                    ) VALUES (
                        @WorkflowId, @RequesterUserId, @CurrentStepId, @CurrentApproverId,
                        'PENDING', @Title, @Description, @Amount, @TargetTable, @TargetRecordId,
                        @PayloadJson, @DigitalSignature, SYSDATETIME()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var instanceId = connection.QuerySingle<long>(insertInstanceSql, new
                {
                    WorkflowId = def.Id,
                    RequesterUserId = requesterUserId,
                    CurrentStepId = firstStep.Id,
                    CurrentApproverId = initialApproverId,
                    Title = string.IsNullOrWhiteSpace(title) ? def.Name : title.Trim(),
                    Description = description,
                    Amount = amount,
                    TargetTable = targetTable,
                    TargetRecordId = targetRecordId,
                    PayloadJson = payloadJson,
                    DigitalSignature = digitalSignature
                }, tx);

                // Ghi log SUBMIT
                const string insertLogSql = @"
                    INSERT INTO dbo.HrmWorkflowLog (
                        InstanceId, StepId, ActorUserId, ActionCode, Comment, DigitalSignature, IpAddress, CreatedAt
                    ) VALUES (
                        @InstanceId, @StepId, @ActorUserId, 'SUBMIT', N'Khởi tạo đề xuất', @DigitalSignature, @IpAddress, SYSDATETIME()
                    );";

                connection.Execute(insertLogSql, new
                {
                    InstanceId = instanceId,
                    StepId = firstStep.Id,
                    ActorUserId = requesterUserId,
                    DigitalSignature = digitalSignature,
                    IpAddress = clientIp
                }, tx);

                // Tạo thông báo cho người duyệt
                if (initialApproverId.HasValue)
                {
                    QueueNotification(connection, tx, initialApproverId.Value,
                        $"Yêu cầu duyệt mới: {def.Name}",
                        $"{requester.DisplayName} vừa gửi yêu cầu '{title}' đang chờ bạn phê duyệt.",
                        "WORKFLOW_APPROVAL", $"/Work/WorkflowApproval/{instanceId}");
                }

                tx.Commit();

                var createdInstance = GetInstanceById(instanceId);
                return (true, "Gửi yêu cầu phê duyệt thành công.", createdInstance);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, "Lỗi khi khởi tạo quy trình: " + ex.Message, null);
            }
        }

        public (bool Success, string Message) ProcessStep(
            long instanceId,
            int actorUserId,
            bool approve,
            string comment,
            string digitalSignature,
            string clientIp)
        {
            using var connection = OpenConnection();
            using var tx = connection.BeginTransaction();
            try
            {
                var instance = connection.QuerySingleOrDefault<WorkflowInstanceModel>(@"
                    SELECT i.Id, i.InstanceCode, i.WorkflowId, i.RequesterUserId, i.CurrentStepId,
                           i.CurrentApproverId, i.StatusCode, i.Title, i.TargetTable, i.TargetRecordId
                    FROM dbo.HrmWorkflowInstance i
                    WHERE i.Id = @Id", new { Id = instanceId }, tx);

                if (instance == null)
                    return (false, "Không tìm thấy yêu cầu phê duyệt.");

                if (instance.StatusCode != "PENDING")
                    return (false, $"Yêu cầu đã ở trạng thái '{instance.StatusCode}', không thể thao tác.");

                var actor = connection.QuerySingleOrDefault<HrmUserAccountModel>(
                    "SELECT Id, Username, DisplayName, RoleCode FROM dbo.HrmUserAccount WHERE Id = @Id",
                    new { Id = actorUserId }, tx);

                // Cho phép người được chỉ định hoặc ADMIN xử lý
                if (instance.CurrentApproverId != actorUserId && actor?.RoleCode != HrmRoles.Admin && actor?.RoleCode != HrmRoles.Director)
                    return (false, "Bạn không có quyền phê duyệt bước này.");

                var currentStep = connection.QuerySingleOrDefault<WorkflowStepModel>(
                    "SELECT Id, WorkflowId, StepOrder, StepName FROM dbo.HrmWorkflowStep WHERE Id = @Id",
                    new { Id = instance.CurrentStepId }, tx);

                var requester = connection.QuerySingleOrDefault<HrmUserAccountModel>(
                    "SELECT Id, DisplayName, SupervisorUserId, DepartmentId FROM dbo.HrmUserAccount WHERE Id = @Id",
                    new { Id = instance.RequesterUserId }, tx);

                if (!approve)
                {
                    // TỪ CHỐI
                    connection.Execute(@"
                        UPDATE dbo.HrmWorkflowInstance
                        SET StatusCode = 'REJECTED', CompletedAt = SYSDATETIME()
                        WHERE Id = @Id", new { Id = instanceId }, tx);

                    connection.Execute(@"
                        INSERT INTO dbo.HrmWorkflowLog (
                            InstanceId, StepId, ActorUserId, ActionCode, Comment, DigitalSignature, IpAddress, CreatedAt
                        ) VALUES (
                            @InstanceId, @StepId, @ActorUserId, 'REJECT', @Comment, @DigitalSignature, @IpAddress, SYSDATETIME()
                        )", new
                    {
                        InstanceId = instanceId,
                        StepId = instance.CurrentStepId,
                        ActorUserId = actorUserId,
                        Comment = comment ?? "Từ chối phê duyệt",
                        DigitalSignature = digitalSignature,
                        IpAddress = clientIp
                    }, tx);

                    // Báo cho người gửi
                    QueueNotification(connection, tx, instance.RequesterUserId,
                        $"Yêu cầu bị từ chối: {instance.Title}",
                        $"{actor?.DisplayName ?? "Cấp trên"} đã từ chối yêu cầu của bạn. Lý do: {comment}",
                        "WORKFLOW_APPROVAL", $"/Work/WorkflowDetail/{instanceId}");

                    tx.Commit();
                    return (true, "Đã từ chối yêu cầu thành công.");
                }

                // PHÊ DUYỆT BƯỚC HIỆN TẠI
                connection.Execute(@"
                    INSERT INTO dbo.HrmWorkflowLog (
                        InstanceId, StepId, ActorUserId, ActionCode, Comment, DigitalSignature, IpAddress, CreatedAt
                    ) VALUES (
                        @InstanceId, @StepId, @ActorUserId, 'APPROVE', @Comment, @DigitalSignature, @IpAddress, SYSDATETIME()
                    )", new
                {
                    InstanceId = instanceId,
                    StepId = instance.CurrentStepId,
                    ActorUserId = actorUserId,
                    Comment = comment ?? "Đồng ý phê duyệt",
                    DigitalSignature = digitalSignature,
                    IpAddress = clientIp
                }, tx);

                // Tìm bước kế tiếp
                var allSteps = connection.Query<WorkflowStepModel>(
                    "SELECT Id, WorkflowId, StepOrder, StepName, ApproverType, SpecificUserId, ThresholdAmount, SlaHours FROM dbo.HrmWorkflowStep WHERE WorkflowId = @WorkflowId ORDER BY StepOrder ASC",
                    new { WorkflowId = instance.WorkflowId }, tx).ToList();

                var currentOrder = currentStep?.StepOrder ?? 1;
                var nextStep = allSteps.FirstOrDefault(s => s.StepOrder > currentOrder);

                if (nextStep != null)
                {
                    // Chuyển sang bước tiếp theo
                    int? nextApproverId = ResolveApproverId(connection, tx, nextStep, requester);

                    connection.Execute(@"
                        UPDATE dbo.HrmWorkflowInstance
                        SET CurrentStepId = @NextStepId, CurrentApproverId = @NextApproverId
                        WHERE Id = @Id", new
                    {
                        Id = instanceId,
                        NextStepId = nextStep.Id,
                        NextApproverId = nextApproverId
                    }, tx);

                    if (nextApproverId.HasValue)
                    {
                        QueueNotification(connection, tx, nextApproverId.Value,
                            $"Yêu cầu chuyển duyệt: {instance.Title}",
                            $"{actor?.DisplayName} đã duyệt bước trước. Đang chờ bạn phê duyệt bước: {nextStep.StepName}.",
                            "WORKFLOW_APPROVAL", $"/Work/WorkflowApproval/{instanceId}");
                    }

                    tx.Commit();
                    return (true, $"Đã duyệt thành công và chuyển sang bước: {nextStep.StepName}.");
                }
                else
                {
                    // ĐÃ DUYỆT XONG TOÀN BỘ CÁC BƯỚC (APPROVED COMPLETE)
                    connection.Execute(@"
                        UPDATE dbo.HrmWorkflowInstance
                        SET StatusCode = 'APPROVED', CurrentApproverId = NULL, CompletedAt = SYSDATETIME()
                        WHERE Id = @Id", new { Id = instanceId }, tx);

                    // Cập nhật bảng gốc nếu có
                    if (instance.TargetTable == "HrmLeaveRequest" && instance.TargetRecordId.HasValue)
                    {
                        connection.Execute(@"
                            UPDATE dbo.HrmLeaveRequest
                            SET StatusCode = 'APPROVED', ApprovedByHrId = @ActorId, UpdatedAt = SYSDATETIME()
                            WHERE Id = @Id", new { ActorId = actorUserId, Id = instance.TargetRecordId.Value }, tx);
                    }

                    QueueNotification(connection, tx, instance.RequesterUserId,
                        $"Yêu cầu đã được duyệt hoàn tất: {instance.Title}",
                        $"Yêu cầu của bạn đã được các cấp lãnh đạo phê duyệt hoàn tất.",
                        "WORKFLOW_APPROVAL", $"/Work/WorkflowDetail/{instanceId}");

                    tx.Commit();
                    return (true, "Đã phê duyệt hoàn tất toàn bộ quy trình.");
                }
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, "Lỗi khi xử lý phê duyệt: " + ex.Message);
            }
        }

        public WorkflowInstanceModel GetInstanceById(long instanceId)
        {
            const string sql = @"
                SELECT i.Id, i.InstanceCode, i.WorkflowId, d.Code AS WorkflowCode, d.Name AS WorkflowName, d.Icon AS WorkflowIcon,
                       i.RequesterUserId, u.DisplayName AS RequesterName, dept.Name AS RequesterDepartment,
                       i.CurrentStepId, s.StepName AS CurrentStepName,
                       i.CurrentApproverId, app.DisplayName AS CurrentApproverName,
                       i.StatusCode, i.Title, i.Description, i.Amount, i.TargetTable, i.TargetRecordId,
                       i.PayloadJson, i.DigitalSignature, i.CreatedAt, i.CompletedAt
                FROM dbo.HrmWorkflowInstance i
                LEFT JOIN dbo.HrmWorkflowDefinition d ON d.Id = i.WorkflowId
                LEFT JOIN dbo.HrmWorkflowStep s ON s.Id = i.CurrentStepId
                LEFT JOIN dbo.HrmUserAccount u ON u.Id = i.RequesterUserId
                LEFT JOIN dbo.HrmDepartment dept ON dept.Id = u.DepartmentId
                LEFT JOIN dbo.HrmUserAccount app ON app.Id = i.CurrentApproverId
                WHERE i.Id = @Id";

            using var connection = OpenConnection();
            var instance = connection.QuerySingleOrDefault<WorkflowInstanceModel>(sql, new { Id = instanceId });
            if (instance != null)
            {
                const string logSql = @"
                    SELECT l.Id, l.InstanceId, l.StepId, s.StepName, l.ActorUserId, u.DisplayName AS ActorName,
                           l.ActionCode, l.Comment, l.DigitalSignature, l.IpAddress, l.CreatedAt
                    FROM dbo.HrmWorkflowLog l
                    LEFT JOIN dbo.HrmWorkflowStep s ON s.Id = l.StepId
                    LEFT JOIN dbo.HrmUserAccount u ON u.Id = l.ActorUserId
                    WHERE l.InstanceId = @InstanceId
                    ORDER BY l.CreatedAt ASC";
                instance.Logs = connection.Query<WorkflowLogModel>(logSql, new { InstanceId = instanceId }).ToList();
            }
            return instance;
        }

        public List<WorkflowInstanceModel> GetMyRequests(int userId, string statusCode = null)
        {
            string sql = @"
                SELECT i.Id, i.InstanceCode, i.WorkflowId, d.Code AS WorkflowCode, d.Name AS WorkflowName, d.Icon AS WorkflowIcon,
                       i.RequesterUserId, u.DisplayName AS RequesterName,
                       i.CurrentStepId, s.StepName AS CurrentStepName,
                       i.CurrentApproverId, app.DisplayName AS CurrentApproverName,
                       i.StatusCode, i.Title, i.Description, i.Amount, i.CreatedAt, i.CompletedAt
                FROM dbo.HrmWorkflowInstance i
                LEFT JOIN dbo.HrmWorkflowDefinition d ON d.Id = i.WorkflowId
                LEFT JOIN dbo.HrmWorkflowStep s ON s.Id = i.CurrentStepId
                LEFT JOIN dbo.HrmUserAccount u ON u.Id = i.RequesterUserId
                LEFT JOIN dbo.HrmUserAccount app ON app.Id = i.CurrentApproverId
                WHERE i.RequesterUserId = @UserId";

            if (!string.IsNullOrWhiteSpace(statusCode))
            {
                sql += " AND i.StatusCode = @StatusCode";
            }
            sql += " ORDER BY i.CreatedAt DESC";

            using var connection = OpenConnection();
            return connection.Query<WorkflowInstanceModel>(sql, new { UserId = userId, StatusCode = statusCode }).ToList();
        }

        public List<WorkflowInstanceModel> GetPendingApprovals(int approverUserId)
        {
            const string sql = @"
                SELECT i.Id, i.InstanceCode, i.WorkflowId, d.Code AS WorkflowCode, d.Name AS WorkflowName, d.Icon AS WorkflowIcon,
                       i.RequesterUserId, u.DisplayName AS RequesterName, dept.Name AS RequesterDepartment,
                       i.CurrentStepId, s.StepName AS CurrentStepName,
                       i.CurrentApproverId, app.DisplayName AS CurrentApproverName,
                       i.StatusCode, i.Title, i.Description, i.Amount, i.CreatedAt
                FROM dbo.HrmWorkflowInstance i
                LEFT JOIN dbo.HrmWorkflowDefinition d ON d.Id = i.WorkflowId
                LEFT JOIN dbo.HrmWorkflowStep s ON s.Id = i.CurrentStepId
                LEFT JOIN dbo.HrmUserAccount u ON u.Id = i.RequesterUserId
                LEFT JOIN dbo.HrmDepartment dept ON dept.Id = u.DepartmentId
                LEFT JOIN dbo.HrmUserAccount app ON app.Id = i.CurrentApproverId
                WHERE i.StatusCode = 'PENDING'
                  AND (i.CurrentApproverId = @ApproverUserId
                       OR EXISTS (SELECT 1 FROM dbo.HrmUserAccount cur WHERE cur.Id = @ApproverUserId AND cur.RoleCode IN ('ADMIN', 'DIRECTOR')))
                ORDER BY i.CreatedAt DESC";

            using var connection = OpenConnection();
            return connection.Query<WorkflowInstanceModel>(sql, new { ApproverUserId = approverUserId }).ToList();
        }

        private int? ResolveApproverId(SqlConnection connection, SqlTransaction tx, WorkflowStepModel step, HrmUserAccountModel requester)
        {
            switch (step.ApproverType)
            {
                case "DIRECT_MANAGER":
                    if (requester.SupervisorUserId.HasValue) return requester.SupervisorUserId.Value;
                    // Fallback to department manager
                    var deptManager = connection.QueryFirstOrDefault<int?>(@"
                        SELECT Id FROM dbo.HrmUserAccount
                        WHERE DepartmentId = @DeptId AND RoleCode = 'MANAGER' AND IsActive = 1",
                        new { DeptId = requester.DepartmentId }, tx);
                    if (deptManager.HasValue) return deptManager.Value;
                    // Fallback to HR
                    return connection.QueryFirstOrDefault<int?>("SELECT TOP 1 Id FROM dbo.HrmUserAccount WHERE RoleCode = 'HR' AND IsActive = 1", null, tx);

                case "DEPARTMENT_HEAD":
                    var head = connection.QueryFirstOrDefault<int?>(@"
                        SELECT Id FROM dbo.HrmUserAccount
                        WHERE DepartmentId = @DeptId AND RoleCode = 'MANAGER' AND IsActive = 1",
                        new { DeptId = requester.DepartmentId }, tx);
                    return head ?? connection.QueryFirstOrDefault<int?>("SELECT TOP 1 Id FROM dbo.HrmUserAccount WHERE RoleCode IN ('DIRECTOR', 'HR') AND IsActive = 1", null, tx);

                case "DIRECTOR":
                    return connection.QueryFirstOrDefault<int?>("SELECT TOP 1 Id FROM dbo.HrmUserAccount WHERE RoleCode = 'DIRECTOR' AND IsActive = 1", null, tx);

                case "HR_ADMIN":
                    return connection.QueryFirstOrDefault<int?>("SELECT TOP 1 Id FROM dbo.HrmUserAccount WHERE RoleCode = 'HR' AND IsActive = 1", null, tx);

                case "SPECIFIC_USER":
                    return step.SpecificUserId;

                default:
                    return requester.SupervisorUserId;
            }
        }

        private void QueueNotification(SqlConnection connection, SqlTransaction tx, int recipientUserId, string title, string body, string notiType, string url)
        {
            const string sql = @"
                INSERT INTO dbo.HrmNotificationQueue (
                    RecipientUserId, Title, Body, NotificationType, TargetUrl, Channels, IsSent, CreatedAt
                ) VALUES (
                    @RecipientUserId, @Title, @Body, @NotificationType, @TargetUrl, 'PUSH,IN_APP', 0, SYSDATETIME()
                )";
            connection.Execute(sql, new
            {
                RecipientUserId = recipientUserId,
                Title = title,
                Body = body,
                NotificationType = notiType,
                TargetUrl = url
            }, tx);
        }
    }
}

