using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Entidades;
using System.Data.SqlClient;
using System.Data;

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



        public bool ExisteUsuario(string dni)
        {
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand("sp_ExisteUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DNI", dni);
                conexion.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public bool RegistrarUsuario(string dni, string clave, string nombre, string apellido)
        {
            using (var conexion = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DNI", dni);
                cmd.Parameters.AddWithValue("@Clave", clave);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@Apellido", apellido);

                conexion.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }





    }
}
