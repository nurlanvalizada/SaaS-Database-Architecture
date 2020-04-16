using System.Web.Mvc;

namespace SaaS_Database_Architecture.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Page404()
        {
            return View();
        }
    }
}
