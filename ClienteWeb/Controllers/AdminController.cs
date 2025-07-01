using Entidades;
using LogicaNegocio;
using System.Configuration;
using System.Data.SqlClient;
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
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            var solicitudes = _solicitudService.ObtenerSolicitudesPendientes();

            return View(solicitudes);
        }

        [HttpPost]
        public ActionResult GestionarSolicitud(int id)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];

            // Asignar la solicitud al admin actual
            using (var conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString))
            {
                string sql = "UPDATE Solicitudes SET idAdministrador = @idAdmin WHERE id = @id AND idAdministrador IS NULL";
                var cmd = new SqlCommand(sql, conexion);
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                cmd.Parameters.AddWithValue("@id", id);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("MisSolicitudes");
        }

        public ActionResult Index() => RedirectToAction("MisSolicitudes");
    }
}
