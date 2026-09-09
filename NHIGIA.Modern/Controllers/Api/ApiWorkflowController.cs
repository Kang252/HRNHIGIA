using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Infrastructure.Workflow;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [Route("api/workflow")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ApiWorkflowController : ApiControllerBase
    {
        private readonly HrmWorkflowEngine _workflowEngine;
        private readonly ILogger<ApiWorkflowController> _logger;

        public ApiWorkflowController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            HrmWorkflowEngine workflowEngine,
            ILogger<ApiWorkflowController> logger)
            : base(store, userAccessor)
        {
            _workflowEngine = workflowEngine;
            _logger = logger;
        }

        [HttpGet("definitions")]
        public IActionResult GetDefinitions()
        {
            try
            {
                var list = _workflowEngine.GetActiveDefinitions();
                return OkResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh mục quy trình");
                return FailResponse("Lỗi: " + ex.Message, 500);
            }
        }

        [HttpGet("my-requests")]
        public IActionResult GetMyRequests([FromQuery] string status)
        {
            try
            {
                var list = _workflowEngine.GetMyRequests(CurrentUserId, status);
                return OkResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách yêu cầu của tôi");
                return FailResponse("Lỗi: " + ex.Message, 500);
            }
        }

        [HttpGet("pending")]
        public IActionResult GetPendingApprovals()
        {
            try
            {
                var list = _workflowEngine.GetPendingApprovals(CurrentUserId);
                return OkResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách chờ duyệt");
                return FailResponse("Lỗi: " + ex.Message, 500);
            }
        }

        [HttpGet("detail/{id:long}")]
        public IActionResult GetDetail(long id)
        {
            try
            {
                var instance = _workflowEngine.GetInstanceById(id);
                if (instance == null) return FailResponse("Không tìm thấy yêu cầu.", 404);
                return OkResponse(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết yêu cầu {Id}", id);
                return FailResponse("Lỗi: " + ex.Message, 500);
            }
        }

        [HttpPost("create")]
        public IActionResult CreateRequest([FromBody] CreateWorkflowRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.WorkflowCode))
                return FailResponse("Vui lòng chọn loại quy trình cần gửi.");

            try
            {
                var (success, message, instance) = _workflowEngine.CreateInstance(
                    requesterUserId: CurrentUserId,
                    workflowCode: request.WorkflowCode,
                    title: request.Title,
                    description: request.Description,
                    amount: request.Amount,
                    payloadJson: request.PayloadJson,
                    digitalSignature: request.DigitalSignature,
                    clientIp: ClientIp,
                    targetTable: request.TargetTable,
                    targetRecordId: request.TargetRecordId);

                if (!success)
                    return FailResponse(message);

                return OkResponse(instance, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khởi tạo yêu cầu workflow");
                return FailResponse("Lỗi máy chủ khi tạo đơn: " + ex.Message, 500);
            }
        }

        [HttpPost("process")]
        public IActionResult ProcessRequest([FromBody] ProcessWorkflowRequest request)
        {
            if (request == null || request.InstanceId <= 0)
                return FailResponse("Yêu cầu không hợp lệ.");

            try
            {
                var (success, message) = _workflowEngine.ProcessStep(
                    instanceId: request.InstanceId,
                    actorUserId: CurrentUserId,
                    approve: request.Approve,
                    comment: request.Comment,
                    digitalSignature: request.DigitalSignature,
                    clientIp: ClientIp);

                if (!success)
                    return FailResponse(message);

                return OkResponse(new { instanceId = request.InstanceId, approved = request.Approve }, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý phê duyệt workflow {InstanceId}", request.InstanceId);
                return FailResponse("Lỗi khi xử lý phê duyệt: " + ex.Message, 500);
            }
        }
    }
}

