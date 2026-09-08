using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using NHIGIA.Web.Infrastructure;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace NHIGIA.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //HttpConfiguration config = GlobalConfiguration.Configuration;
            //config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
            //    new CamelCasePropertyNamesContractResolver();
            //config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            //Database.SetInitializer<ApplicationDbContext>(null);
            //Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());

            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            try
            {
                HrmDataStore.Instance.EnsureSchema(Server.MapPath("~/App_Data/hrm-mvp.sql"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Không thể khởi tạo schema HRM MVP");
            }
        }

        protected void Application_PostAuthenticateRequest()
        {
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value)) return;
            try
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket == null || ticket.Expired) return;
                Context.User = new GenericPrincipal(new FormsIdentity(ticket), new[] { ticket.UserData });
            }
            catch
            {
                FormsAuthentication.SignOut();
            }
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            Response.Clear();
            Response.Write(ex.StackTrace);
            _logger.Error(ex);
        }
    }
}
