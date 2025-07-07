using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClienteWeb.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace ClienteWeb.Controllers
{
    public class PerfilController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string dni = ObtenerDniDesdeSesion();

            if (string.IsNullOrEmpty(dni))
                return RedirectToAction("Login", "Account");

            var perfil = await ObtenerPerfilDesdeApi(dni);

            if (perfil == null)
            {
                ViewBag.Mensaje = "No se encontraron datos del asegurado.";
                return View("Error");
            }

            return View(perfil);
        }

        private string ObtenerDniDesdeSesion()
        {
            return Session["DNI"]?.ToString(); 
        }

        private async Task<PerfilViewModel> ObtenerPerfilDesdeApi(string dni)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = $"http://sisdataperu-003-site1.itempurl.com/prueba2/api/asegurados/{dni}";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Api-Key", "12345ABCDEF");

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        var settings = new JsonSerializerSettings
                        {
                            DateFormatString = "dd/MM/yyyy hh:mm:ss tt",
                            Culture = new CultureInfo("es-PE") // o "es-ES"
                        };

                        var perfil = JsonConvert.DeserializeObject<PerfilViewModel>(json, settings);
                        return perfil;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al obtener datos: " + ex.Message);
                }
            }

            return null;
        }
    }
}
