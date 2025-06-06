using System.Web.Mvc;

namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class AdminController : Controller
    {
 

        public ActionResult MisSolicitudes()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult RevisarSolicitudes()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult Index() => RedirectToAction("MisSolicitudes");
    }
}
