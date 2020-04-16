using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using SaaS_Database_Architecture.Models;
using WebMatrix.WebData;

namespace SaaS_Database_Architecture.Controllers
{
    [Authorize]
    public class TenantController : Controller
    {
        string _connectionString;

        public TenantController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Administration"].ConnectionString;
        }

        private readonly UsersContext _db = new UsersContext();

        public ActionResult Index()
        {
            var tenantprofile = _db.UserProfiles.FirstOrDefault(p => p.UserName == User.Identity.Name).UserTenantProfile;
            var userprofiles = _db.UserProfiles.Where(p => p.UserTenantProfile.TenantId == tenantprofile.TenantId).ToList();
            ViewBag.Users = userprofiles;
            return View(tenantprofile);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterModel rmodel)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register new user for Tenant
                try
                {
                   var tenantprofile = _db.UserProfiles.FirstOrDefault(p => p.UserName == User.Identity.Name).UserTenantProfile;

                    WebSecurity.CreateUserAndAccount(rmodel.UserName, rmodel.Password, new
                    {
                        FirstName = rmodel.FirstName,
                        LastName = rmodel.LastName,
                        EmailAdress = rmodel.Email,
                        WorkPhone = rmodel.WorkPhone,
                        @UserTenantProfile_TenantId = tenantprofile.TenantId

                    });

                }
                catch (MembershipCreateUserException e)
                {
                     ModelState.AddModelError("", e.Message);
                }
                return RedirectToAction("Index", "Tenant");
            }
            return View(rmodel);
        }
  
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }

}