using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClienteWeb.Controllers
{
    public class UsuarioController : Controller
    {
        public ActionResult Perfil()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult Tramitar()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult Seguimiento()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult Index() => RedirectToAction("Perfil");
    }
}