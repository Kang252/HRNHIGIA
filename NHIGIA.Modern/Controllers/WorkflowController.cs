using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Infrastructure.Workflow;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers
{
    [Authorize]
    public class WorkflowController : BaseController
    {
        private readonly HrmWorkflowEngine _workflowEngine;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            HrmWorkflowEngine workflowEngine,
            ILogger<WorkflowController> logger)
            : base(store, userAccessor)
        {
            _workflowEngine = workflowEngine;
            _logger = logger;
        }

        private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        [HttpGet]
        public IActionResult Index(string tab = "pending")
        {
            ViewBag.Title = "Quy trình Phê duyệt Động (Workflow)";
            ViewBag.ActiveTab = tab;
            ViewBag.Definitions = _workflowEngine.GetActiveDefinitions();
            ViewBag.PendingList = _workflowEngine.GetPendingApprovals(CurrentHrmUser.Id);
            ViewBag.MyRequests = _workflowEngine.GetMyRequests(CurrentHrmUser.Id);

            return View();
        }

        [HttpGet]
        public IActionResult Detail(long id)
        {
            var instance = _workflowEngine.GetInstanceById(id);
            if (instance == null) return NotFound();
            ViewBag.Title = $"Chi tiết đề xuất {instance.InstanceCode}";
            return View(instance);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(CreateWorkflowRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.WorkflowCode))
            {
                TempData["Error"] = "Vui lòng chọn loại đề xuất.";
                return RedirectToAction("Index", new { tab = "my-requests" });
            }

            var (success, message, instance) = _workflowEngine.CreateInstance(
                requesterUserId: CurrentHrmUser.Id,
                workflowCode: request.WorkflowCode,
                title: request.Title,
                description: request.Description,
                amount: request.Amount,
                payloadJson: request.PayloadJson,
                digitalSignature: request.DigitalSignature,
                clientIp: ClientIp,
                targetTable: request.TargetTable,
                targetRecordId: request.TargetRecordId);

            if (success)
            {
                TempData["Success"] = $"Đã gửi đề xuất '{request.Title}' thành công (Mã: {instance?.InstanceCode}).";
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Index", new { tab = "my-requests" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Process(ProcessWorkflowRequest request)
        {
            if (request == null || request.InstanceId <= 0)
            {
                TempData["Error"] = "Yêu cầu không hợp lệ.";
                return RedirectToAction("Index", new { tab = "pending" });
            }

            var (success, message) = _workflowEngine.ProcessStep(
                instanceId: request.InstanceId,
                actorUserId: CurrentHrmUser.Id,
                approve: request.Approve,
                comment: request.Comment,
                digitalSignature: request.DigitalSignature,
                clientIp: ClientIp);

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Index", new { tab = "pending" });
        }
    }
}

