using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AccesoDatos.Repositories;
using Entidades;
using Newtonsoft.Json;
using Utiles;


public class MisSolicitudesController : Controller
{
    public async Task<ActionResult> Index(string fechaInicio = null, string fechaFin = null, string estado = null)
    {
        if (Session["Usuario"] == null)
            return RedirectToAction("Login", "Account");

        if (!DateUtils.ValidarRango(fechaInicio, fechaFin, out string mensajeError))
        {
            ViewBag.Error = mensajeError;
            fechaInicio = fechaFin = null;
        }

        // Cargar estados
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

        // Obtener detalle desde API
        Solicitud solicitud = null;
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://localhost:44300/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"/api/solicitudes/solicitudes/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                solicitud = JsonConvert.DeserializeObject<Solicitud>(json);
            }
        }

        // Obtener todas las solicitudes del usuario sin filtros
        int idUsuario = (int)Session["IdUsuario"];
        var solicitudes = await ObtenerSolicitudesFiltradas(idUsuario, null, null, null);

        // Cargar estados
        var repo = new SolicitudRepository();
        var estadosDrop = repo.ObtenerEstadosSolicitud();
        ViewBag.EstadosSolicitud = new SelectList(estadosDrop, "Id", "Nombre");

        ViewBag.FechaInicio = null;
        ViewBag.FechaFin = null;
        ViewBag.Estado = null;
        ViewBag.FiltrosActivos = null;

        // Detalle para modal
        ViewBag.DetalleSolicitud = solicitud;

        return View("Index", solicitudes);
    }

    private async Task<List<Solicitud>> ObtenerSolicitudesFiltradas(int idUsuario, string fechaInicio, string fechaFin, string estado)
    {
        var solicitudes = new List<Solicitud>();

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://localhost:44300/");
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
}
