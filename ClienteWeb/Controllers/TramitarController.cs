using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Entidades;
using LogicaNegocio;

namespace ClienteWeb.Controllers
{
    public class TramitarController : Controller
    {
        private SolicitudService _solicitudService;

        public TramitarController()
        {
            _solicitudService = new SolicitudService();  // Inicializamos el servicio de solicitudes
        }

        // Acción para mostrar el formulario de solicitud
        public ActionResult RegistrarSolicitud()
        {
            // Obtener los tipos de solicitud desde la base de datos usando el servicio
            ViewBag.TiposSolicitud = _solicitudService.ObtenerTiposSolicitud() ?? new List<Entidades.TipoSolicitud>();

            // Pasamos los tipos de solicitud a la vista
            return View("Usuario/RegistrarSolicitud");
        }

        // Acción para manejar el envío del formulario y registrar la solicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarSolicitud(Solicitud solicitud, HttpPostedFileBase DocumentoAdjunto)
        {
            // Verificar si el formulario es válido
            if (!ModelState.IsValid)
            {
                // Si el formulario no es válido, volver a cargar los tipos de solicitud en la vista
                var tiposSolicitud = _solicitudService.ObtenerTiposSolicitud();
                ViewBag.TiposSolicitud = tiposSolicitud;
                return View("Usuario/RegistrarSolicitud");
            }

            // Si se proporciona un documento adjunto, guardarlo
            if (DocumentoAdjunto != null && DocumentoAdjunto.ContentLength > 0)
            {
                var fileName = Path.GetFileName(DocumentoAdjunto.FileName);
                var filePath = Path.Combine(Server.MapPath("~/App_Data/Uploads"), fileName);
                DocumentoAdjunto.SaveAs(filePath);

                // Guardar la ruta del archivo en la solicitud
                solicitud.DocumentoAdjuntoRuta = filePath;
            }

            // Llamar al servicio de negocios para guardar la solicitud
            _solicitudService.GuardarSolicitud(solicitud);

            // Mensaje de éxito al guardar la solicitud
            TempData["Mensaje"] = "Solicitud registrada exitosamente.";

            // Redirigir a la acción "Index" de "MisSolicitudes"
            return RedirectToAction("Index", "MisSolicitudes");
        }
    }
}
