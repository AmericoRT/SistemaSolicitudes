using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccesoDatos.Repositories;
using Entidades;

namespace LogicaNegocio
{
    public class UsuarioService
    {
        private readonly UsuarioRepository repo = new UsuarioRepository();

        public Usuario Login(string usuario, string clave)
        {
            return repo.ValidarUsuario(usuario, clave);
        }

    }
}
