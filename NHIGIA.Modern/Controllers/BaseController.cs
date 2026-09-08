using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected readonly HrmDataStore Store;
    protected readonly HrmUserAccessor UserAccessor;

    protected BaseController(HrmDataStore store, HrmUserAccessor userAccessor)
    {
        Store = store;
        UserAccessor = userAccessor;
    }

    protected HrmUserAccountModel CurrentHrmUser => UserAccessor.Current;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var current = CurrentHrmUser;
        if (current != null)
        {
            ViewBag.CurrentUserName = current.DisplayName;
            ViewBag.CurrentRoleCode = current.RoleCode;
            ViewBag.CurrentRoleLabel = current.RoleLabel;
            ViewBag.CurrentDepartment = current.DepartmentName;
        }
        base.OnActionExecuting(context);
    }
}
