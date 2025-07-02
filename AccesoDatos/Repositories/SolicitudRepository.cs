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

        public Solicitud ObtenerSolicitudPorId(int id)
        {
            Solicitud solicitud = null;

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT s.id, s.cabecera, s.detalle,
                   s.fechaSolicitud,
                   s.idAsegurado, s.idAdministrador,
                   s.idTipoSolicitud, s.idEstado,
                   ts.nombre_tipo, es.estado_nombre
            FROM Solicitudes s
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            WHERE s.id = @id";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    solicitud = new Solicitud
                    {
                        Id = (int)reader["id"],
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["detalle"].ToString(),
                        FechaSolicitud = (DateTime)reader["fechaSolicitud"],
                        //DocumentoAdjuntoRuta = reader["documentoAdjuntoRuta"].ToString(),
                        IdAsegurado = (int)reader["idAsegurado"],
                        IdAdministrador = reader["idAdministrador"] as int?,
                        IdTipoSolicitud = (int)reader["idTipoSolicitud"],
                        IdEstado = (int)reader["idEstado"],
                        TipoSolicitud = reader["nombre_tipo"].ToString(),
                        EstadoSolicitud = reader["estado_nombre"].ToString()
                    };
                }

                connection.Close();
            }

            return solicitud;
        }
        public List<EstadoSolicitud> ObtenerEstadosSolicitud()
        {
            var lista = new List<EstadoSolicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = "SELECT id, estado_nombre FROM Estados_Solicitud";
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new EstadoSolicitud
                    {
                        Id = (int)reader["id"],
                        Nombre = reader["estado_nombre"].ToString()
                    });
                }

                connection.Close();
            }

            return lista;
        }

        public void ActualizarEstadoSolicitud(int idSolicitud, int estadoAnterior, int estadoNuevo, int idAdmin, string comentario)
        {
            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();

                // 1. Actualizar el estado de la solicitud
                string updateSql = @"
            UPDATE Solicitudes
            SET idEstado = @nuevoEstado,
                fechaUltimaModificacion = GETDATE()
            WHERE id = @idSolicitud";

                using (var cmdUpdate = new SqlCommand(updateSql, connection))
                {
                    cmdUpdate.Parameters.AddWithValue("@nuevoEstado", estadoNuevo);
                    cmdUpdate.Parameters.AddWithValue("@idSolicitud", idSolicitud);
                    cmdUpdate.ExecuteNonQuery();
                }

                // 2. Registrar la modificación
                string insertSql = @"
            INSERT INTO Modificaciones_Solicitud
            (idSolicitud, idAdministrador, idEstadoAnterior, idEstadoNuevo, fechaModificacion, comentario)
            VALUES (@idSolicitud, @idAdmin, @estadoAnterior, @estadoNuevo, GETDATE(), @comentario)";

                using (var cmdInsert = new SqlCommand(insertSql, connection))
                {
                    cmdInsert.Parameters.AddWithValue("@idSolicitud", idSolicitud);
                    cmdInsert.Parameters.AddWithValue("@idAdmin", idAdmin);
                    cmdInsert.Parameters.AddWithValue("@estadoAnterior", estadoAnterior);
                    cmdInsert.Parameters.AddWithValue("@estadoNuevo", estadoNuevo);
                    cmdInsert.Parameters.AddWithValue("@comentario", comentario ?? "");
                    cmdInsert.ExecuteNonQuery();
                }

                connection.Close();
            }
        }



    }

}
