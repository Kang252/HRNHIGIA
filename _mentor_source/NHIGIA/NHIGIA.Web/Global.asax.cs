using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;

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
