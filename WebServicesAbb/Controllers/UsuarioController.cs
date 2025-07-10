using System.Collections.Generic;
using System.Web.Http;
using Entidades;
using LogicaNegocio;

namespace WebServicesAbb.Controllers
{
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        private readonly UsuarioService _usuarioService;

        public UsuariosController()
        {
            _usuarioService = new UsuarioService();
        }

        // GET: api/usuarios
        [HttpGet]
        [Route("")]
        public IHttpActionResult ObtenerTodos()
        {
            List<Usuario> usuarios = _usuarioService.ObtenerTodosLosUsuarios();

            if (usuarios == null || usuarios.Count == 0)
                return NotFound();

            return Ok(usuarios);
        }
    }
}
