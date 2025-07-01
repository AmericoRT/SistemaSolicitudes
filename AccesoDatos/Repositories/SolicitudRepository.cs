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
        public List<Solicitud> ObtenerSolicitudesPorAdministrador(int idAdmin)
        {
            var solicitudes = new List<Solicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT s.id, ts.nombre_tipo AS TipoSolicitud,
                   es.estado_nombre AS EstadoSolicitud,
                   s.cabecera, s.detalle AS Descripcion,
                   s.fechaSolicitud
            FROM Solicitudes s
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            WHERE s.idAdministrador = @idAdmin";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@idAdmin", idAdmin);

                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    solicitudes.Add(new Solicitud
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TipoSolicitud = reader["TipoSolicitud"].ToString(),
                        EstadoSolicitud = reader["EstadoSolicitud"].ToString(),
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                        DocumentoAdjuntoRuta = ""
                    });
                }

                reader.Close();
                connection.Close();
            }

            return solicitudes;
        }
        public List<Solicitud> ObtenerSolicitudesPendientes()
        {
            var solicitudes = new List<Solicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT s.id, ts.nombre_tipo AS TipoSolicitud,
                   es.estado_nombre AS EstadoSolicitud,
                   s.cabecera, s.detalle AS Descripcion,
                   s.fechaSolicitud
            FROM Solicitudes s
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            WHERE s.idAdministrador IS NULL";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    solicitudes.Add(new Solicitud
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        TipoSolicitud = reader["TipoSolicitud"].ToString(),
                        EstadoSolicitud = reader["EstadoSolicitud"].ToString(),
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                        DocumentoAdjuntoRuta = ""
                    });
                }

                reader.Close();
                connection.Close();
            }

            return solicitudes;
        }



    }

}
