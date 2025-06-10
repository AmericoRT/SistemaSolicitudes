using System.Web.Mvc;
using LogicaNegocio;

namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class RegistroController : Controller
    {
        private readonly UsuarioService servicio = new UsuarioService();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string DNI, string Codigo, string ConfirmarCodigo)
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

            if (servicio.ExisteUsuario(DNI))
            {
                ViewBag.Mensaje = "El usuario ya está registrado.";
                return View();
            }

            if (servicio.RegistrarUsuario(DNI, Codigo))
            {
                TempData["RegistroExitoso"] = "¡Registro exitoso! Ahora puedes iniciar sesión.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Mensaje = "Error al registrar el usuario.";
            return View();
        }
    }
}
