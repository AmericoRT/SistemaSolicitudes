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
            int nuevoEstado = 4; // "En Revisión"

            using (var conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString))
            {
                conexion.Open();

                // 1. Obtener el estado actual
                int estadoAnterior = 0;
                string sqlEstado = "SELECT idEstado FROM Solicitudes WHERE id = @id";
                using (var cmdEstado = new SqlCommand(sqlEstado, conexion))
                {
                    cmdEstado.Parameters.AddWithValue("@id", id);
                    estadoAnterior = (int)cmdEstado.ExecuteScalar();
                }

                // 2. Asignar administrador y actualizar estado
                string sqlUpdate = @"
            UPDATE Solicitudes
            SET idAdministrador = @idAdmin,
                idEstado = @nuevoEstado,
                fechaUltimaModificacion = GETDATE()
            WHERE id = @id";

                using (var cmdUpdate = new SqlCommand(sqlUpdate, conexion))
                {
                    cmdUpdate.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmdUpdate.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                    cmdUpdate.Parameters.AddWithValue("@id", id);
                    cmdUpdate.ExecuteNonQuery();
                }

                // 3. Insertar en Modificaciones_Solicitud
                string sqlInsert = @"
            INSERT INTO Modificaciones_Solicitud
            (idSolicitud, idAdministrador, idEstadoAnterior, idEstadoNuevo, fechaModificacion, comentario)
            VALUES
            (@id, @idAdmin, @estadoAnterior, @nuevoEstado, GETDATE(), @comentario)";

                using (var cmdInsert = new SqlCommand(sqlInsert, conexion))
                {
                    cmdInsert.Parameters.AddWithValue("@id", id);
                    cmdInsert.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmdInsert.Parameters.AddWithValue("@estadoAnterior", estadoAnterior);
                    cmdInsert.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                    cmdInsert.Parameters.AddWithValue("@comentario", "Solicitud asignada y puesta en revisión");
                    cmdInsert.ExecuteNonQuery();
                }

                conexion.Close();
            }

            return RedirectToAction("MisSolicitudes");
        }

        public ActionResult Detalle(int id)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            var solicitud = _solicitudService.ObtenerSolicitudPorId(id);
            var estados = _solicitudService.ObtenerEstadosSolicitud();

            ViewBag.Estados = new SelectList(estados, "Id", "Nombre", solicitud.IdEstado);

            return View(solicitud);
        }
        [HttpPost]
        public ActionResult Detalle(int id, int nuevoEstado, string comentario)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];
            var solicitud = _solicitudService.ObtenerSolicitudPorId(id);

            if (solicitud.IdEstado == nuevoEstado)
            {
                ViewBag.Mensaje = "Debe seleccionar un estado diferente al actual.";
                ViewBag.Estados = new SelectList(_solicitudService.ObtenerEstadosSolicitud(), "Id", "Nombre", nuevoEstado);
                return View(solicitud);
            }

            _solicitudService.ActualizarEstadoSolicitud(solicitud.Id, solicitud.IdEstado, nuevoEstado, idAdmin, comentario);

            return RedirectToAction("MisSolicitudes");
        }


        public ActionResult Index() => RedirectToAction("MisSolicitudes");
    }
}
