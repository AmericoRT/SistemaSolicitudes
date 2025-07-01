using System.Web;
using System;
using System.Web.Mvc;
using System.Web.Security;
using LogicaNegocio;

namespace SistemaSolicitudes.ClienteWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioService servicio = new UsuarioService();

        //public ActionResult Login() => View();

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string usuario, string clave)
        {
            var user = servicio.Login(usuario, clave);
            if (user != null)
            {
                Session["IdUsuario"] = user.Id;
                Session["DNI"] = usuario;
                if (user.Rol == "Admin")
                    Session["Admin"] = user;
                else
                    Session["Usuario"] = user;


                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.NombreUsuario, 
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    user.Rol 
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                return RedirectToAction("Index", user.Rol == "Admin" ? "Admin" : "Usuario");
            }

            ViewBag.Mensaje = "Credenciales inválidas";
            return View();
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
