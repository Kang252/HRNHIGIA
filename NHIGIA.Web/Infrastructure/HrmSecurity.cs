using NHIGIA.Web.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace NHIGIA.Web.Infrastructure
{
    public static class HrmPasswordHasher
    {
        private const int Iterations = 100000;

        public static bool Verify(string password, string saltBase64, string expectedHashBase64)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64)) return false;
            var salt = Convert.FromBase64String(saltBase64);
            var expected = Convert.FromBase64String(expectedHashBase64);
            byte[] actual;
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations)) actual = derive.GetBytes(32);
            if (actual.Length != expected.Length) return false;
            var difference = 0;
            for (var i = 0; i < actual.Length; i++) difference |= actual[i] ^ expected[i];
            return difference == 0;
        }
    }

    public static class HrmSecretProtector
    {
        private static readonly string[] Purpose = { "NHIGIA", "HanetCredential", "v1" };

        public static string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(value), Purpose));
        }

        public static string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            try
            {
                var bytes = MachineKey.Unprotect(Convert.FromBase64String(value), Purpose);
                return bytes == null ? null : Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class HrmSession
    {
        public static HrmUserAccountModel Current
        {
            get
            {
                var context = HttpContext.Current;
                return context == null || context.Session == null ? null : context.Session["HrmCurrentUser"] as HrmUserAccountModel;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null) HttpContext.Current.Session["HrmCurrentUser"] = value;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class HrmAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private readonly string[] _roles;

        public HrmAuthorizeAttribute(params string[] roles)
        {
            _roles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            return _roles.Length == 0 || _roles.Any(httpContext.User.IsInRole);
        }

        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Account", action = "AccessDenied" }));
                return;
            }
            base.HandleUnauthorizedRequest(filterContext);
        }
    }

    /// <summary>Chặn nhân viên đi trực tiếp vào các controller Mentor cũ chưa có data-scope.</summary>
    public sealed class HrmDataScopeAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            if (!httpContext.User.IsInRole(HrmRoles.Employee)) return true;
            var route = httpContext.Request.RequestContext.RouteData.Values;
            var controller = Convert.ToString(route["controller"]);
            var action = Convert.ToString(route["action"]);
            if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase) || string.Equals(controller, "Hrm", StringComparison.OrdinalIgnoreCase) || string.Equals(controller, "HanetWebhook", StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase)) return false;
            return new[] { "Index", "Attendance", "WorkSchedules", "LeaveRequests", "InternalCommunications", "MyProfile" }.Contains(action, StringComparer.OrdinalIgnoreCase);
        }

        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Account", action = "AccessDenied" }));
                return;
            }
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
