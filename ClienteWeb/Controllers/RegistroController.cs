using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using LogicaNegocio;
using Newtonsoft.Json;

namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class RegistroController : Controller
    {
        private readonly UsuarioService servicio = new UsuarioService();

        private async Task<bool> VerificarDniEnApiAsync(string dni)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = $"http://sisdataperu-003-site1.itempurl.com/prueba2/api/asegurados/{dni}"; 
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Api-Key", "12345ABCDEF");
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string DNI, string Codigo, string ConfirmarCodigo, string Nombre, string Apellido)
        {
            if (string.IsNullOrWhiteSpace(DNI) || DNI.Length < 8 || DNI.Length > 12)
            {
                ViewBag.Mensaje = "El DNI debe tener entre 8 y 12 caracteres.";
                return View();
            }

            if (Codigo != ConfirmarCodigo)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                return View();
            }

            bool dniValido = await VerificarDniEnApiAsync(DNI);
            if (!dniValido)
            {
                ViewBag.Mensaje = "El DNI no se encuentra registrado como asegurado.";
                return View();
            }

            if (servicio.ExisteUsuario(DNI))
            {
                ViewBag.Mensaje = "El usuario ya está registrado.";
                return View();
            }

            if (servicio.RegistrarUsuario(DNI, Codigo, Nombre, Apellido))
            {
                TempData["RegistroExitoso"] = "¡Registro exitoso! Ahora puedes iniciar sesión.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Mensaje = "Error al registrar el usuario.";
            return View();
        }
    }
}
