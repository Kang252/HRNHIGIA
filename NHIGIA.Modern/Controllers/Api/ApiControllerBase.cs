using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers.Api
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly HrmDataStore Store;
        protected readonly HrmUserAccessor UserAccessor;

        protected ApiControllerBase(HrmDataStore store, HrmUserAccessor userAccessor)
        {
            Store = store;
            UserAccessor = userAccessor;
        }

        protected HrmUserAccountModel CurrentUser => UserAccessor.Current;

        protected int CurrentUserId
        {
            get
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(idStr, out var id) ? id : (CurrentUser?.Id ?? 0);
            }
        }

        protected string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        protected IActionResult OkResponse(object data, string message = "Thao tác thành công.")
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data
            });
        }

        protected IActionResult FailResponse(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                success = false,
                message = message,
                data = (object)null
            });
        }
    }
}

