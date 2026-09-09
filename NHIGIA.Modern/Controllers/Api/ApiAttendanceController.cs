using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [Route("api/attendance")]
    [Authorize(AuthenticationSchemes = "Bearer,Cookies")]
    public class ApiAttendanceController : ApiControllerBase
    {
        private readonly GpsAttendanceService _gpsService;
        private readonly ILogger<ApiAttendanceController> _logger;

        public ApiAttendanceController(
            HrmDataStore store,
            HrmUserAccessor userAccessor,
            GpsAttendanceService gpsService,
            ILogger<ApiAttendanceController> logger)
            : base(store, userAccessor)
        {
            _gpsService = gpsService;
            _logger = logger;
        }

        [HttpGet("offices")]
        public IActionResult GetOffices()
        {
            var offices = _gpsService.GetActiveOffices();
            return OkResponse(offices, "Danh sách văn phòng và bán kính geofence hợp lệ.");
        }

        [HttpPost("checkin-gps")]
        public IActionResult CheckInGps([FromBody] MobileGpsCheckInRequest request)
        {
            try
            {
                var (success, message, log) = _gpsService.ProcessGpsCheckIn(CurrentUserId, request, ClientIp);
                if (!success)
                    return FailResponse(message);

                return OkResponse(log, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm công GPS cho UserId {UserId}", CurrentUserId);
                return FailResponse("Lỗi khi xử lý chấm công GPS: " + ex.Message, 500);
            }
        }

        [HttpGet("history")]
        public IActionResult GetHistory(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var history = _gpsService.GetGpsHistory(CurrentUserId, fromDate, toDate);
                return OkResponse(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử chấm công GPS");
                return FailResponse("Lỗi khi tải lịch sử: " + ex.Message, 500);
            }
        }

        [HttpGet("summary")]
        public IActionResult GetSummary(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var to = (toDate ?? DateTime.Today).Date;
                var from = (fromDate ?? to.AddDays(-30)).Date;
                var records = Store.GetAttendance(CurrentUser, from, to);
                return OkResponse(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tổng hợp bảng công cá nhân");
                return FailResponse("Lỗi: " + ex.Message, 500);
            }
        }
    }
}

