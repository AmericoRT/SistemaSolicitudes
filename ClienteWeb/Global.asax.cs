using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace ClienteWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    string[] roles = ticket.UserData.Split(','); // por si hay múltiples roles en el futuro
                    var identity = new FormsIdentity(ticket);
                    var principal = new GenericPrincipal(identity, roles);
                    HttpContext.Current.User = principal;
                }
                catch
                {
                    
                }
            }
        }


    }
}
