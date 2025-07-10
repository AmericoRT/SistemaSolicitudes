using Entidades;
using LogicaNegocio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClienteWeb.Models;


namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class AdminController : Controller
    {
        //string apiUrl = "https://localhost:44359/";
        string apiUrl = "https://localhost:44300/";

        private SolicitudService _solicitudService = new SolicitudService();
        public async Task<ActionResult> MisSolicitudes(DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];
            var solicitudes = await ObtenerSolicitudesPorAdministradorAPI(idAdmin, fecha, dni, nombre, idTipo, idEstado);

            // Cargar dropdowns
            ViewBag.Tipos = new SelectList(_solicitudService.ObtenerTiposSolicitud(), "Id", "Nombre", idTipo);
            ViewBag.Estados = new SelectList(_solicitudService.ObtenerEstadosSolicitud(), "Id", "Nombre", idEstado);

            
            return View(solicitudes);
        }


        public async Task<ActionResult> RevisarSolicitudes(DateTime? fecha, string dni, 
            string nombre, int? idTipo, int? idEstado)

        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login", "Account");

            var solicitudes = await ObtenerSolicitudesPendientesAdminAPI(fecha, dni, nombre, idTipo, idEstado);

            ViewBag.Tipos = new SelectList(_solicitudService.ObtenerTiposSolicitud(), "Id", "Nombre", idTipo);
            ViewBag.Estados = new SelectList(_solicitudService.ObtenerEstadosSolicitud(), "Id", "Nombre", idEstado);

            return View(solicitudes);
        }

        [HttpPost]
        public async Task<ActionResult> GestionarSolicitud(int id)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];
            int nuevoEstado = 2; // En revisión

            // Primero obtener la solicitud para saber el estado anterior
            Solicitud solicitud = await ObtenerSolicitudPorIdAPI(id);
            if (solicitud == null)
            {
                TempData["MensajeError"] = "No se pudo obtener la solicitud.";
                return RedirectToAction("RevisarSolicitudes");
            }

            var dto = new EstadoSolicitudDto
            {
                IdSolicitud = id,
                IdAdministrador = idAdmin,
                EstadoAnterior = solicitud.IdEstado,
                EstadoNuevo = nuevoEstado,
                Comentario = "Solicitud asignada y puesta en revisión"
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/solicitudes/actualizarEstado", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("MisSolicitudes");
                else
                {
                    TempData["MensajeError"] = "Error al gestionar la solicitud vía API.";
                    return RedirectToAction("RevisarSolicitudes");
                }
            }
        }




        public async Task<ActionResult> Detalle(int id)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            Solicitud solicitud = await ObtenerSolicitudPorIdAPI(id);
            var estados = _solicitudService.ObtenerEstadosSolicitud();
            ViewBag.Estados = new SelectList(estados, "Id", "Nombre", solicitud.IdEstado);

            return View(solicitud);
        }
        [HttpPost]
        public async Task<ActionResult> Detalle(int id, int nuevoEstado, string comentario)
        {
            if (Session["Admin"] == null || Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Account");

            int idAdmin = (int)Session["IdUsuario"];
            Solicitud solicitud = await ObtenerSolicitudPorIdAPI(id);

            if (solicitud.IdEstado == nuevoEstado)
            {
                ViewBag.Mensaje = "Debe seleccionar un estado diferente al actual.";
                ViewBag.Estados = new SelectList(_solicitudService.ObtenerEstadosSolicitud(), "Id", "Nombre", nuevoEstado);
                return View(solicitud);
            }

            await ActualizarEstadoAPI(solicitud.Id, solicitud.IdEstado, nuevoEstado, idAdmin, comentario);

            return RedirectToAction("MisSolicitudes");
        }

        private async Task<List<Solicitud>> ObtenerSolicitudesPorAdministradorAPI(int idAdmin, DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            var solicitudes = new List<Solicitud>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = $"api/solicitudes/administrador/{idAdmin}";
                var queryParams = new List<string>();

                if (fecha.HasValue) queryParams.Add($"fechaInicio={HttpUtility.UrlEncode(fecha.Value.ToString("yyyy-MM-dd"))}");
                if (!string.IsNullOrEmpty(dni)) queryParams.Add($"dni={HttpUtility.UrlEncode(dni)}");
                if (!string.IsNullOrEmpty(nombre)) queryParams.Add($"nombre={HttpUtility.UrlEncode(nombre)}");
                if (idTipo.HasValue) queryParams.Add($"tipo={idTipo.Value}");
                if (idEstado.HasValue) queryParams.Add($"estado={idEstado.Value}");

                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    solicitudes = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                }
            }

            return solicitudes;
        }

        private async Task<Solicitud> ObtenerSolicitudPorIdAPI(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync($"api/solicitudes/solicitudes/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Solicitud>(json);
                }
            }

            return null;
        }

        private async Task<bool> ActualizarEstadoAPI(int idSolicitud, int estadoAnterior, int estadoNuevo, int idAdmin, string comentario)
        {
            var body = new
            {
                IdSolicitud = idSolicitud,
                IdAdministrador = idAdmin,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                Comentario = comentario
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/solicitudes/actualizarEstado", content);
                return response.IsSuccessStatusCode;
            }
        }
        private async Task<List<Solicitud>> ObtenerSolicitudesPendientesAdminAPI(
    DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            var solicitudes = new List<Solicitud>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var queryParams = new List<string>();
                if (fecha.HasValue) queryParams.Add("fechaInicio=" + fecha.Value.ToString("yyyy-MM-dd"));
                if (!string.IsNullOrEmpty(dni)) queryParams.Add("dni=" + HttpUtility.UrlEncode(dni));
                if (!string.IsNullOrEmpty(nombre)) queryParams.Add("nombre=" + HttpUtility.UrlEncode(nombre));
                if (idTipo.HasValue) queryParams.Add("tipo=" + idTipo.Value);
                if (idEstado.HasValue) queryParams.Add("estado=" + idEstado.Value);

                string url = "api/solicitudes/pendientes-admin";
                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    solicitudes = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                }
            }

            return solicitudes;
        }



        public ActionResult Index() => RedirectToAction("MisSolicitudes");
    }
}
