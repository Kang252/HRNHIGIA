using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [Route("api/ess")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ApiEssController : ApiControllerBase
    {
        private readonly EmployeeSelfServiceStore _essStore;
        private readonly ILogger<ApiEssController> _logger;

        public ApiEssController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            EmployeeSelfServiceStore essStore,
            ILogger<ApiEssController> logger)
            : base(store, userAccessor)
        {
            _essStore = essStore;
            _logger = logger;
        }

        [HttpGet("payslips")]
        public IActionResult GetPayslips()
        {
            try
            {
                var payslips = _essStore.GetMyPayslips(CurrentUserId);
                return OkResponse(payslips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiếu lương điện tử");
                return FailResponse("Lỗi khi tải phiếu lương: " + ex.Message, 500);
            }
        }

        [HttpGet("documents")]
        public IActionResult GetDocuments()
        {
            try
            {
                var docs = _essStore.GetMyDocuments(CurrentUserId);
                return OkResponse(docs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách tài liệu hợp đồng");
                return FailResponse("Lỗi khi tải tài liệu: " + ex.Message, 500);
            }
        }

        [HttpPost("profile")]
        public IActionResult UpdateProfile([FromBody] MobileUpdateProfileRequest request)
        {
            try
            {
                var ok = _essStore.UpdateProfile(CurrentUserId, request);
                if (!ok) return FailResponse("Không thể cập nhật thông tin hồ sơ.");
                return OkResponse(new { updated = true }, "Cập nhật thông tin thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ cá nhân");
                return FailResponse("Lỗi máy chủ khi cập nhật: " + ex.Message, 500);
            }
        }
    }
}

