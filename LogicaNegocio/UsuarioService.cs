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

        // Validar si ya existe un usuario por DNI
        public bool ExisteUsuario(string dni)
        {
            return repo.ExisteUsuario(dni);
        }

        // Registrar un usuario
        public bool RegistrarUsuario(string dni, string clave)
        {
            return repo.RegistrarUsuario(dni, clave);
        }

    }
}
