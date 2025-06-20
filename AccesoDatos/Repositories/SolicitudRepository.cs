using Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace AccesoDatos.Repositories
{
    public class SolicitudRepository
    {
        private readonly string cadenaConexion;

        public SolicitudRepository()
        {
            cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;
        }

            public List<TipoSolicitud> ObtenerTiposSolicitud()
            {
                var tiposSolicitud = new List<TipoSolicitud>();

                using (SqlConnection connection = new SqlConnection(cadenaConexion))
                {
                    SqlCommand command = new SqlCommand("ObtenerTiposSolicitud", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tiposSolicitud.Add(new TipoSolicitud
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString()
                        });
                    }

                    connection.Close();
                }

                return tiposSolicitud;
            }

        public void GuardarSolicitud(Solicitud solicitud)
        {
            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                SqlCommand command = new SqlCommand("InsertarSolicitud", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@TipoSolicitud", solicitud.TipoSolicitud);
                command.Parameters.AddWithValue("@Descripcion", solicitud.Descripcion);
                command.Parameters.AddWithValue("@FechaSolicitud", solicitud.FechaSolicitud);
                command.Parameters.AddWithValue("@DocumentoAdjuntoRuta", solicitud.DocumentoAdjuntoRuta);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

    }

}
