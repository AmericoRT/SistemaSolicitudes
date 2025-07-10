using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClienteWeb.Models;
using Entidades;
using LogicaNegocio;

namespace ClienteWeb.Controllers
{
    public class TramitarController : Controller
    {
        private SolicitudService _solicitudService;

        public TramitarController()
        {
            _solicitudService = new SolicitudService();
        }

        // Muestra el formulario
        public ActionResult RegistrarSolicitud()
        {
            var tipos = _solicitudService.ObtenerTiposSolicitud();
            ViewBag.TiposSolicitud = new SelectList(tipos, "IdTipoSolicitud", "Nombre");

            return View("~/Views/Usuario/Tramitar.cshtml");
        }

        // Procesa el formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarSolicitud(Solicitud solicitud, IEnumerable<HttpPostedFileBase> DocumentosAdjuntos)
        {
            if (!ModelState.IsValid)
            {
                var tipos = _solicitudService.ObtenerTiposSolicitud();
                ViewBag.TiposSolicitud = new SelectList(tipos, "IdTipoSolicitud", "Nombre");
                return View("~/Views/Usuario/Tramitar.cshtml", solicitud);
            }

            // Asigna el usuario actual desde la sesión
            solicitud.IdAsegurado = (int)Session["IdUsuario"];

            // Ruta base para guardar los archivos
            string rutaBase = Server.MapPath("~/App_Data/Uploads");

            if (!Directory.Exists(rutaBase))
                Directory.CreateDirectory(rutaBase);

            // Lista de rutas si deseas almacenar varias (ejemplo si usas una entidad ArchivoAdjunto)
            List<string> rutasGuardadas = new List<string>();

            if (DocumentosAdjuntos != null)
            {
                foreach (var archivo in DocumentosAdjuntos)
                {
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(archivo.FileName);
                        string nombreUnico = Guid.NewGuid().ToString() + extension;
                        string rutaCompleta = Path.Combine(rutaBase, nombreUnico);

                        archivo.SaveAs(rutaCompleta);
                        rutasGuardadas.Add(rutaCompleta);
                    }
                }
            }

            solicitud.ArchivosAdjuntos = new List<ArchivoAdjunto>();

            foreach (var archivo in DocumentosAdjuntos)
            {
                if (archivo != null && archivo.ContentLength > 0)
                {
                    string extension = Path.GetExtension(archivo.FileName);
                    string nombreUnico = Guid.NewGuid().ToString() + extension;
                    string ruta = Path.Combine(Server.MapPath("~/App_Data/Uploads"), nombreUnico);
                    archivo.SaveAs(ruta);

                    solicitud.ArchivosAdjuntos.Add(new ArchivoAdjunto
                    {
                        Ruta = ruta,
                        NombreOriginal = Path.GetFileName(archivo.FileName)
                    });
                }
            }

            //if (rutasGuardadas.Count > 0)
            //{
            //    solicitud.DocumentoAdjuntoRuta = rutasGuardadas[0]; // o concatenar rutas si solo tienes una columna
            //}

            // Guardar solicitud
            _solicitudService.GuardarSolicitud(solicitud);

            TempData["Mensaje"] = "Solicitud registrada exitosamente.";
            return RedirectToAction("Index", "MisSolicitudes");
        }
    }
}
