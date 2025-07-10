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

        public Usuario ObtenerPorUsuario(string usuario)
        {
            using (var conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Usuario u inner join rol r on u.idRol=r.idRol WHERE DNI = @usuario ", conn);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Usuario
                    {
                        Id = (int)reader["IdUsuario"],
                        NombreUsuario = reader["DNI"].ToString(),
                        Clave = reader["Clave"].ToString(),
                        Rol = reader["DesRol"].ToString()
                    };
                }
                return null;
            }
        }

        public void ActualizarClaveHasheada(int id, string nuevaClave)
        {
            using (var conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Usuario SET Clave = @clave WHERE IdUsuario = @id", conn);
                cmd.Parameters.AddWithValue("@clave", nuevaClave);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Usuario u inner join rol r on u.idRol=r.idRol", conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Usuario
                    {
                        Id = (int)reader["IdUsuario"],
                        NombreUsuario = reader["DNI"].ToString(),
                        Clave = reader["Clave"].ToString(),
                        Rol = reader["DesRol"].ToString()
                    });
                }
            }

            return lista;
        }

    }
}
