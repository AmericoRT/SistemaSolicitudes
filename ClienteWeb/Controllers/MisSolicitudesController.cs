using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AccesoDatos.Repositories;
using Entidades;
using LogicaNegocio;
using Newtonsoft.Json;
using Utiles;

public class MisSolicitudesController : Controller
{
    string ApiUrl = "https://localhost:44300/";

    public async Task<ActionResult> Index(string fechaInicio = null, string fechaFin = null, string estado = null)
    {
        if (Session["Usuario"] == null)
            return RedirectToAction("Login", "Account");

        if (!DateUtils.ValidarRango(fechaInicio, fechaFin, out string mensajeError))
        {
            ViewBag.Error = mensajeError;
            fechaInicio = fechaFin = null;
        }

        try
        {
            var repo = new SolicitudRepository();
            var estados = repo.ObtenerEstadosSolicitud();
            ViewBag.EstadosSolicitud = new SelectList(estados, "Id", "Nombre", estado);
        }
        catch
        {
            var estadosDefault = new List<dynamic>
            {
                new { Id = "Pendiente", Nombre = "Pendiente" },
                new { Id = "En Proceso", Nombre = "En Proceso" },
                new { Id = "Completada", Nombre = "Completada" },
                new { Id = "Rechazada", Nombre = "Rechazada" }
            };
            ViewBag.EstadosSolicitud = new SelectList(estadosDefault, "Id", "Nombre", estado);
        }

        int idUsuario = (int)Session["IdUsuario"];
        var solicitudes = await ObtenerSolicitudesFiltradas(idUsuario, fechaInicio, fechaFin, estado);

        ViewBag.FechaInicio = fechaInicio;
        ViewBag.FechaFin = fechaFin;
        ViewBag.Estado = estado;

        List<string> filtrosActivos = new List<string>();
        if (!string.IsNullOrEmpty(fechaInicio)) filtrosActivos.Add($"Desde: {fechaInicio}");
        if (!string.IsNullOrEmpty(fechaFin)) filtrosActivos.Add($"Hasta: {fechaFin}");
        if (!string.IsNullOrEmpty(estado)) filtrosActivos.Add($"Estado: {estado}");

        ViewBag.FiltrosActivos = filtrosActivos.Count > 0 ? string.Join(", ", filtrosActivos) : null;

        return View(solicitudes);
    }

    [HttpPost]
    public async Task<ActionResult> VerDetalle(int id)
    {
        if (Session["Usuario"] == null)
            return RedirectToAction("Login", "Account");

        Solicitud solicitud = null;
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(ApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"/api/solicitudes/solicitudes/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                solicitud = JsonConvert.DeserializeObject<Solicitud>(json);
            }
        }

        int idUsuario = (int)Session["IdUsuario"];
        var solicitudes = await ObtenerSolicitudesFiltradas(idUsuario, null, null, null);

        var repo = new SolicitudRepository();
        var estadosDrop = repo.ObtenerEstadosSolicitud();
        ViewBag.EstadosSolicitud = new SelectList(estadosDrop, "Id", "Nombre");

        ViewBag.FechaInicio = null;
        ViewBag.FechaFin = null;
        ViewBag.Estado = null;
        ViewBag.FiltrosActivos = null;

        solicitud.ArchivosAdjuntos = repo.ObtenerArchivosPorSolicitud(id);
        ViewBag.DetalleSolicitud = solicitud;

        return View("Index", solicitudes);
    }

    private async Task<List<Solicitud>> ObtenerSolicitudesFiltradas(int idUsuario, string fechaInicio, string fechaFin, string estado)
    {
        var solicitudes = new List<Solicitud>();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(ApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string url = $"api/solicitudes/usuario/{idUsuario}";
            var parametros = new List<string>();

            if (!string.IsNullOrEmpty(fechaInicio)) parametros.Add($"fechaInicio={HttpUtility.UrlEncode(fechaInicio)}");
            if (!string.IsNullOrEmpty(fechaFin)) parametros.Add($"fechaFin={HttpUtility.UrlEncode(fechaFin)}");
            if (!string.IsNullOrEmpty(estado)) parametros.Add($"estado={HttpUtility.UrlEncode(estado)}");

            if (parametros.Count > 0)
                url += "?" + string.Join("&", parametros);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                solicitudes = JsonConvert.DeserializeObject<List<Solicitud>>(json);
            }
        }

        return solicitudes;
    }

    [HttpGet]
    public ActionResult DescargarArchivo(int id)
    {
        if (Session["Usuario"] == null)
            return RedirectToAction("Login", "Account");

        try
        {
            var repo = new SolicitudRepository();
            var archivo = repo.ObtenerArchivoPorId(id);

            if (archivo == null)
                return HttpNotFound("Archivo no encontrado");

            int idUsuario = (int)Session["IdUsuario"];
            var solicitud = repo.ObtenerSolicitudPorId(archivo.IdSolicitud);

            if (solicitud == null || solicitud.IdAsegurado != idUsuario)
                return new HttpStatusCodeResult(403, "No tiene permisos para descargar este archivo");

            if (string.IsNullOrEmpty(archivo.Ruta) || !System.IO.File.Exists(archivo.Ruta))
                return HttpNotFound("Archivo no disponible en el sistema");

            byte[] contenido = System.IO.File.ReadAllBytes(archivo.Ruta);
            string contentType = ObtenerContentType(archivo.NombreOriginal);

            return File(contenido, contentType, archivo.NombreOriginal); // Forza descarga
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al descargar archivo: {ex.Message}");
            return new HttpStatusCodeResult(500, "Error interno del servidor");
        }
    }


    [HttpGet]
    public ActionResult VerArchivo(int id)
    {
        if (Session["Usuario"] == null)
            return RedirectToAction("Login", "Account");

        try
        {
            var repo = new SolicitudRepository();
            var archivo = repo.ObtenerArchivoPorId(id);

            if (archivo == null)
            {
                return HttpNotFound("Archivo no encontrado");
            }

            // Verificar que el usuario tenga permisos para ver este archivo
            int idUsuario = (int)Session["IdUsuario"];
            var solicitud = repo.ObtenerSolicitudPorId(archivo.IdSolicitud);

            if (solicitud == null || solicitud.IdAsegurado != idUsuario)
                return new HttpStatusCodeResult(403, "No tiene permisos para descargar este archivo");


            // Verificar que el archivo sea visualizable
            string extension = Path.GetExtension(archivo.NombreOriginal)?.ToLower();
            var extensionesVisualizables = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

            if (!extensionesVisualizables.Contains(extension))
            {
                return new HttpStatusCodeResult(400, "Este tipo de archivo no se puede visualizar directamente");
            }

            // Obtener ruta física del archivo
            //string rutaBase = Server.MapPath("~/App_Data/Uploads/");
            string rutaCompleta = archivo.Ruta;

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return HttpNotFound("Contenido del archivo no encontrado");
            }

            byte[] contenidoArchivo = System.IO.File.ReadAllBytes(rutaCompleta);
            string contentType = ObtenerContentType(archivo.NombreOriginal);

            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{archivo.NombreOriginal}\"");

            return File(contenidoArchivo, contentType);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al ver archivo: {ex.Message}");
            return new HttpStatusCodeResult(500, "Error interno del servidor");
        }
    }
    private string ObtenerContentType(string nombreArchivo)
    {
        string extension = Path.GetExtension(nombreArchivo).ToLower();

        switch (extension)
        {
            case ".pdf": return "application/pdf";
            case ".jpg":
            case ".jpeg": return "image/jpeg";
            case ".png": return "image/png";
            case ".gif": return "image/gif";
            case ".doc": return "application/msword";
            case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case ".xls": return "application/vnd.ms-excel";
            case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            case ".txt": return "text/plain";
            default: return "application/octet-stream";
        }
    }


    // También necesitarás agregar una acción para anular solicitudes si no la tienes
    [HttpPost]
    public async Task<ActionResult> AnularSolicitud(int id)
    {
        if (Session["Usuario"] == null)
            return Json(new { success = false, message = "Sesión expirada" });

        try
        {
            int idUsuario = (int)Session["IdUsuario"];
            var repo = new SolicitudRepository();
            var solicitud = repo.ObtenerSolicitudPorId(id);

            if (solicitud == null)
            {
                return Json(new { success = false, message = "Solicitud no encontrada" });
            }


            // Verificar que la solicitud se pueda anular
            if (solicitud.IdEstado != 1 && solicitud.IdEstado != 3)
            {
                return Json(new { success = false, message = "Esta solicitud no se puede anular" });
            }

            //// Anular la solicitud via API
            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(ApiUrl);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    var response = await client.PutAsync($"/api/solicitudes/{id}/anular", null);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        return Json(new { success = true, message = "Solicitud anulada correctamente" });
            //    }
            //}

            // Si la API no funciona, intentar con el repositorio local
            bool resultado = repo.AnularSolicitud(id, idUsuario, solicitud.IdEstado);
            if (resultado)
            {
                return Json(new { success = true, message = "Solicitud anulada correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "Error al anular la solicitud" });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al anular solicitud: {ex.Message}");
            return Json(new { success = false, message = "Error interno del servidor" });
        }
    }


}
