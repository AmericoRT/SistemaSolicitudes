using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Entidades;
using System.Data.SqlClient;

namespace AccesoDatos.Repositories
{
    public class UsuarioRepository
    {

        private readonly string cadenaConexion;

        public UsuarioRepository()
        {
            cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
        }
        public Usuario ValidarUsuario(string usuario, string clave)
        {
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                var comando = new SqlCommand("SP_ValidarUsuario", conexion);
                comando.CommandType = System.Data.CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@Usuario", usuario);
                comando.Parameters.AddWithValue("@Clave", clave);

                conexion.Open();
                var lector = comando.ExecuteReader();
                if (lector.Read())
                {
                    return new Usuario
                    {
                        Id = (int)lector["Id"],
                        NombreUsuario = lector["NombreUsuario"].ToString(),
                        Clave = lector["Clave"].ToString(),
                        Rol = lector["Rol"].ToString()
                    };
                }
                return null;
            }
        }

    }
}
