using Entidades;
using LogicaNegocio;
using System.Linq;
using System.Web.Mvc;

namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class AdminController : Controller
    {
        private SolicitudService _solicitudService = new SolicitudService();
        public ActionResult MisSolicitudes()
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];
            var solicitudes = _solicitudService.ObtenerSolicitudesPorAdministrador(idAdmin);

            return View(solicitudes);
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
