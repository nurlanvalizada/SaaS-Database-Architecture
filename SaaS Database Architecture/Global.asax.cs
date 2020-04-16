using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SaaS_Database_Architecture.Models;
using WebMatrix.WebData;

namespace SaaS_Database_Architecture
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            Database.SetInitializer<UsersContext>(new MyDatabaseInit());
            new UsersContext().UserProfiles.Find(1);
           WebSecurity.InitializeDatabaseConnection("Administration", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }
    }
}