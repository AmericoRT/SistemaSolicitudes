using Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AccesoDatos;
using Utiles;
using AccesoDatos.Repositories;

namespace ClienteWeb.Controllers
{
    public class MisSolicitudesController : Controller
    {
        private Dictionary<string, string> _estadosMap;

        private Dictionary<string, string> ObtenerEstadosMap()
        {
            if (_estadosMap == null)
            {
                try
                {
                    var repo = new SolicitudRepository();
                    var estados = repo.ObtenerEstadosSolicitud();
                    _estadosMap = estados.ToDictionary(e => e.Id.ToString(), e => e.Nombre);
                }
                catch
                {
                    // Fallback en caso de error
                    _estadosMap = new Dictionary<string, string>
                    {
                        { "1", "Pendiente" },
                        { "2", "En Proceso" },
                        { "3", "Completada" },
                        { "4", "Rechazada" }
                    };
                }
            }
            return _estadosMap;
        }

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
            catch (Exception ex)
            {
                var estadosDefault = new List<dynamic>
                {
                    new { Id = "Pendiente", Nombre = "Pendiente" },
                    new { Id = "En Proceso", Nombre = "En Proceso" },
                    new { Id = "Completada", Nombre = "Completada" },
                    new { Id = "Rechazada", Nombre = "Rechazada" }
                };
                ViewBag.EstadosSolicitud = new SelectList(estadosDefault, "Id", "Nombre", estado);
                System.Diagnostics.Debug.WriteLine($"Error cargando estados: {ex.Message}");
            }

            var solicitudes = new List<Solicitud>();
            int idUsuario = (int)Session["IdUsuario"];

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44300/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = $"api/solicitudes/usuario/{idUsuario}";
                List<string> parametros = new List<string>();

                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out DateTime fi))
                    parametros.Add($"fechaInicio={HttpUtility.UrlEncode(fi.ToString("yyyy-MM-dd"))}");

                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out DateTime ff))
                    parametros.Add($"fechaFin={HttpUtility.UrlEncode(ff.ToString("yyyy-MM-dd"))}");

                if (!string.IsNullOrEmpty(estado))
                    parametros.Add($"estado={HttpUtility.UrlEncode(estado)}");

                if (parametros.Count > 0)
                    url += "?" + string.Join("&", parametros);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    solicitudes = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener las solicitudes desde el servicio.";
                }
            }

            if (solicitudes != null && solicitudes.Count > 0)
            {
                var solicitudesFiltradas = solicitudes.AsQueryable();

                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out DateTime inicioDate))
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud >= inicioDate);

                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out DateTime finDate))
                {
                    finDate = finDate.Date.AddDays(1).AddTicks(-1);
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud <= finDate);
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    var estadosMap = ObtenerEstadosMap();
                    if (estadosMap.ContainsKey(estado))
                    {
                        string nombreEstado = estadosMap[estado];
                        solicitudesFiltradas = solicitudesFiltradas.Where(s =>
                            s.EstadoSolicitud.Equals(nombreEstado, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        solicitudesFiltradas = solicitudesFiltradas.Where(s =>
                            s.EstadoSolicitud.Equals(estado, StringComparison.OrdinalIgnoreCase));
                    }
                }

                solicitudes = solicitudesFiltradas.ToList();
            }

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

        public async Task<ActionResult> Detalle(int id)
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            Solicitud solicitud = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44300/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync($"api/solicitudes/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    solicitud = JsonConvert.DeserializeObject<Solicitud>(json);
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener el detalle de la solicitud.";
                }
            }

            if (solicitud == null)
                return HttpNotFound("Solicitud no encontrada");

            return View(solicitud);
        }

        public ActionResult Seguimiento()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction("Index", "MisSolicitudes");
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session["Usuario"] = null;
            return RedirectToAction("Login", "Account");
        }
    }
}
