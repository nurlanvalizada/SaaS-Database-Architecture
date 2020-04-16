using System.Linq;
using System.Web.Mvc;
using SaaS_Database_Architecture.Models;

namespace SaaS_Database_Architecture.Controllers
{

    //[Authorize(Users = "admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private UsersContext _db = new UsersContext();
        public ActionResult Index()
        {
            ViewBag.TenantCount = _db.TenantProfiles.Count();
            ViewBag.UserCount = _db.UserProfiles.Count();
            return View();
        }
    }
}