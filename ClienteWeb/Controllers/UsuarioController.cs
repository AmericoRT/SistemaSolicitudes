using AccesoDatos.Repositories;
using LogicaNegocio;
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

            return RedirectToAction("Index", "Perfil");
        }


        public ActionResult Tramitar()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            var repo = new SolicitudRepository();
            var estados = repo.ObtenerTiposSolicitud();
            ViewBag.TiposSolicitud = new SelectList(estados, "Id", "Nombre");
            return View();
        }

        public ActionResult Seguimiento()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction("Index", "MisSolicitudes");
        }

        public ActionResult Index() => RedirectToAction("Perfil");
    }
}