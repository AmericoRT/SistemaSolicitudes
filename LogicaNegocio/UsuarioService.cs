using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccesoDatos.Repositories;
using Entidades;
using Utiles;

namespace LogicaNegocio
{
    public class UsuarioService
    {
        private readonly UsuarioRepository repo = new UsuarioRepository();

        private HashearClave util = new HashearClave();
        //public Usuario Login(string usuario, string clave)
        //{
        //    return repo.ValidarUsuario(usuario, clave);
        //}

        // Validar si ya existe un usuario por DNI
        public bool ExisteUsuario(string dni)
        {
            return repo.ExisteUsuario(dni);
        }

        // Registrar un usuario
        public bool RegistrarUsuario(string dni, string clave, string nombre, string apellido)
        {
            return repo.RegistrarUsuario(dni, clave,nombre,apellido);
        }

        public Usuario Login(string usuario, string clave)
        {
            var user = repo.ObtenerPorUsuario(usuario);
            if (user == null) return null;

            if (util.EsHashSHA256(user.Clave))
            {
                string claveHasheada = util.HashearSHA256(clave);
                return claveHasheada == user.Clave ? user : null;
            }
            else
            {
                // Comparar en plano y actualizar
                if (user.Clave == clave)
                {
                    string nuevaClave = util.HashearSHA256(clave);
                    repo.ActualizarClaveHasheada(user.Id, nuevaClave);
                    return user;
                }
                else return null;
            }
        }

        public List<Usuario> ObtenerTodosLosUsuarios()
        {
            return repo.ObtenerTodos();
        }


    }
}
