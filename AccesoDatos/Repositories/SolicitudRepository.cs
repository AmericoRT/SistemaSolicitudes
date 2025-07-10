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
                SqlCommand command = new SqlCommand("SELECT id, nombre_tipo AS Nombre FROM Tipos_Solicitud", connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tiposSolicitud.Add(new TipoSolicitud
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Nombre = reader["Nombre"].ToString()
                    });
                }

                connection.Close();
            }

            return tiposSolicitud;
        }

        public void GuardarSolicitud(Solicitud solicitud)
        {
            //string cadenaConexion = ConfigurationManager.ConnectionStrings["TuCadena"].ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("InsertarSolicitud", conexion, transaccion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@idAsegurado", solicitud.IdAsegurado);
                    cmd.Parameters.AddWithValue("@idAdministrador", (object)solicitud.IdAdministrador ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idEstado", 1);
                    cmd.Parameters.AddWithValue("@idTipoSolicitud", solicitud.IdTipoSolicitud);
                    cmd.Parameters.AddWithValue("@cabecera", solicitud.Cabecera);
                    cmd.Parameters.AddWithValue("@detalle", solicitud.Descripcion);

                    int idSolicitud = Convert.ToInt32(cmd.ExecuteScalar());
                    if (solicitud.ArchivosAdjuntos != null)
                    {
                        foreach (var archivo in solicitud.ArchivosAdjuntos)
                        {
                            SqlCommand cmdArchivo = new SqlCommand(@"
                        INSERT INTO Archivos_Adjuntos (IdSolicitud, archivo_Ruta, archivo_nombre,fecha_subida)
                        VALUES (@IdSolicitud, @Ruta, @NombreOriginal,getDate())", conexion, transaccion);

                            cmdArchivo.Parameters.AddWithValue("@IdSolicitud", idSolicitud);
                            cmdArchivo.Parameters.AddWithValue("@Ruta", archivo.Ruta);
                            cmdArchivo.Parameters.AddWithValue("@NombreOriginal", archivo.NombreOriginal);

                            cmdArchivo.ExecuteNonQuery();
                        }
                    }

                    transaccion.Commit();
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    throw new Exception("Error al guardar la solicitud: " + ex.Message);
                }
            }
        }


        public List<Solicitud> ObtenerSolicitudesPorUsuario(int idUsuario)
        {
            List<Solicitud> lista = new List<Solicitud>();

            using (SqlConnection con = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT s.*,tipoEstado=es.estado_nombre,tipoSolicitud=tp.nombre_tipo
                    FROM Solicitudes s inner join Tipos_Solicitud tp on s.idTipoSolicitud=tp.id
                    inner join Estados_Solicitud es on s.idEstado = es.id 
                    WHERE idAsegurado = @idUsuario", con);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Solicitud
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            IdAsegurado = Convert.ToInt32(reader["idAsegurado"]),
                            IdTipoSolicitud = Convert.ToInt32(reader["idTipoSolicitud"]),
                            IdEstado = Convert.ToInt32(reader["idEstado"]),
                            Cabecera = reader["cabecera"].ToString(),
                            Descripcion = reader["detalle"].ToString(),
                            FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                            FechaUltimaModificacion = Convert.ToDateTime(reader["fechaUltimaModificacion"]),
                            TipoSolicitud = reader["tipoSolicitud"].ToString(),
                            EstadoSolicitud = reader["tipoEstado"].ToString(),
                        });
                    }
                }
            }

            return lista;
        }

        public List<ArchivoAdjunto> ObtenerArchivosPorSolicitud(int idSolicitud)
        {
            var lista = new List<ArchivoAdjunto>();
           
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, archivo_ruta, archivo_nombre FROM archivos_adjuntos WHERE IdSolicitud = @IdSolicitud", conexion);
                cmd.Parameters.AddWithValue("@IdSolicitud", idSolicitud);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new ArchivoAdjunto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Ruta = reader["archivo_ruta"].ToString(),
                            NombreOriginal = reader["archivo_nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }


        public List<Solicitud> ObtenerSolicitudesPorUsuarioYFechas(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                List<Solicitud> lista = new List<Solicitud>();

                using (SqlConnection con = new SqlConnection(cadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand(@"SELECT s.*,tipoEstado=es.estado_nombre,tipoSolicitud=tp.nombre_tipo
                    FROM Solicitudes s inner join Tipos_Solicitud tp on s.idTipoSolicitud=tp.id
                    inner join Estados_Solicitud es on s.idEstado = es.id 
                    WHERE idAsegurado = @idUsuario AND fechaSolicitud >= @FechaInicio 
                    AND fechaSolicitud <= @FechaFin
                    ORDER BY fechaSolicitud DESC", con);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Solicitud
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                IdAsegurado = Convert.ToInt32(reader["idAsegurado"]),
                                IdTipoSolicitud = Convert.ToInt32(reader["idTipoSolicitud"]),
                                IdEstado = Convert.ToInt32(reader["idEstado"]),
                                Cabecera = reader["cabecera"].ToString(),
                                Descripcion = reader["detalle"].ToString(),
                                FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                                FechaUltimaModificacion = Convert.ToDateTime(reader["fechaUltimaModificacion"]),
                                TipoSolicitud = reader["tipoSolicitud"].ToString(),
                                EstadoSolicitud = reader["tipoEstado"].ToString(),
                            });
                        }
                    }
                }

                return lista;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error al obtener solicitud: {ex.Message}", ex);
            }
        }

        public Solicitud ObtenerSolicitudesPorId(int id)
        {
            Solicitud solicitud = null;

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
                SELECT top 1 s.id, s.cabecera, s.detalle,
                s.fechaSolicitud,
                s.idAsegurado, s.idAdministrador,
                s.idTipoSolicitud, s.idEstado,
                ts.nombre_tipo, es.estado_nombre,
                fechaUltimaModificacion=ms.fechaModificacion,
                observacion = ms.comentario 
                FROM Solicitudes s
                INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
                INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
                INNER JOIN Modificaciones_Solicitud ms ON s.id = ms.idSolicitud and ms.idEstadoNuevo=s.idEstado
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
                        EstadoSolicitud = reader["estado_nombre"].ToString(),
                        FechaUltimaModificacion = (DateTime)reader["fechaUltimaModificacion"],
                        Observacion = reader["observacion"] != DBNull.Value ? reader["observacion"].ToString() : null,
                    };
                }

                connection.Close();
            }

            return solicitud;
        }


        public List<Solicitud> ObtenerSolicitudesPorAdministrador(int idAdmin)
        {
            var solicitudes = new List<Solicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT 
                s.id, 
                ts.nombre_tipo AS TipoSolicitud,
                es.estado_nombre AS EstadoSolicitud,
                s.cabecera, 
                s.detalle AS Descripcion,
                s.fechaSolicitud,
                u.DNI,
                (a.Nombre + ' ' + a.Apellido) AS NombreAsegurado,
                s.idTipoSolicitud,s.idEstado 
            FROM Solicitudes s
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            INNER JOIN Asegurado a ON s.idAsegurado = a.IdUsuario
            INNER JOIN Usuario u ON a.IdUsuario = u.IdUsuario
            WHERE s.idAdministrador = @idAdmin
                ";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@idAdmin", idAdmin);

                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    solicitudes.Add(new Solicitud
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        DNI = reader["DNI"].ToString(),
                        NombreAsegurado = reader["NombreAsegurado"].ToString(),
                        TipoSolicitud = reader["TipoSolicitud"].ToString(),
                        EstadoSolicitud = reader["EstadoSolicitud"].ToString(),
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["Descripcion"].ToString(),
                        FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                        //DocumentoAdjuntoRuta = "",
                        IdEstado= Convert.ToInt32(reader["idEstado"]),
                        IdTipoSolicitud = Convert.ToInt32(reader["idTipoSolicitud"])
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
            SELECT 
                s.id, 
                ts.nombre_tipo AS TipoSolicitud,
                es.estado_nombre AS EstadoSolicitud,
                s.cabecera, 
                s.detalle AS Descripcion,
                s.fechaSolicitud,
                u.DNI,
               (a.Nombre + ' ' + a.Apellido) AS NombreAsegurado
            FROM Solicitudes s
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            INNER JOIN Asegurado a ON s.idAsegurado = a.IdUsuario
            INNER JOIN Usuario u ON a.IdUsuario = u.IdUsuario
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
                        DNI = reader["DNI"].ToString(),
                        NombreAsegurado = reader["NombreAsegurado"].ToString(),
                        //DocumentoAdjuntoRuta = ""
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
                //string query = "SELECT id, estado_nombre FROM Estados_Solicitud";
                string query = "SELECT id, estado_nombre FROM Estados_Solicitud WHERE estado_nombre!='ANULADA' ";
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

                // 1. Actualizar el estado y asignar el administrador si aún no está asignado
                string updateSql = @"
        UPDATE Solicitudes
        SET idEstado = @nuevoEstado,
            idAdministrador = ISNULL(idAdministrador, @idAdmin), -- asignar solo si está null
            fechaUltimaModificacion = GETDATE()
        WHERE id = @idSolicitud";

                using (var cmdUpdate = new SqlCommand(updateSql, connection))
                {
                    cmdUpdate.Parameters.AddWithValue("@nuevoEstado", estadoNuevo);
                    cmdUpdate.Parameters.AddWithValue("@idSolicitud", idSolicitud);
                    cmdUpdate.Parameters.AddWithValue("@idAdmin", idAdmin);
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


        public List<Solicitud> FiltrarSolicitudes(DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            var lista = new List<Solicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT s.id, s.cabecera, s.detalle, s.fechaSolicitud,
                   ts.nombre_tipo, es.estado_nombre,
                   u.DNI, CONCAT(a.nombre, ' ', a.apellido) AS NombreAsegurado
            FROM Solicitudes s
            INNER JOIN Asegurado a ON s.idAsegurado = a.IdUsuario
            INNER JOIN Usuario u ON a.idUsuario = u.idUsuario
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            WHERE s.idAdministrador IS NULL"; // solo no gestionadas

                // Aplica filtros si se enviaron
                if (fecha.HasValue)
                    query += " AND CAST(s.fechaSolicitud AS DATE) = @fecha";

                if (!string.IsNullOrEmpty(dni))
                    query += " AND u.DNI LIKE @dni";

                if (!string.IsNullOrEmpty(nombre))
                    query += " AND (a.nombre LIKE @nombre OR a.apellido LIKE @nombre)";

                SqlCommand cmd = new SqlCommand(query, connection);

                if (fecha.HasValue)
                    cmd.Parameters.AddWithValue("@fecha", fecha.Value.Date);
                if (!string.IsNullOrEmpty(dni))
                    cmd.Parameters.AddWithValue("@dni", "%" + dni + "%");
                if (!string.IsNullOrEmpty(nombre))
                    cmd.Parameters.AddWithValue("@nombre", "%" + nombre + "%");

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Solicitud
                    {
                        Id = (int)reader["id"],
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["detalle"].ToString(),
                        FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                        TipoSolicitud = reader["nombre_tipo"].ToString(),
                        EstadoSolicitud = reader["estado_nombre"].ToString(),
                        DNI = reader["DNI"].ToString(),
                        NombreAsegurado = reader["NombreAsegurado"].ToString()
                    });
                }

                connection.Close();
            }

            return lista;
        }
        public List<Solicitud> ObtenerSolicitudesPorAdministradorFiltrado(
    int idAdmin, DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            var lista = new List<Solicitud>();

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                string query = @"
            SELECT s.id, s.cabecera, s.detalle, s.fechaSolicitud,
                   ts.nombre_tipo, es.estado_nombre,
                   u.DNI, CONCAT(a.nombre, ' ', a.apellido) AS NombreAsegurado
            FROM Solicitudes s
            INNER JOIN Asegurado a ON s.idAsegurado = a.IdUsuario
            INNER JOIN Usuario u ON a.idUsuario = u.idUsuario
            INNER JOIN Tipos_Solicitud ts ON s.idTipoSolicitud = ts.id
            INNER JOIN Estados_Solicitud es ON s.idEstado = es.id
            WHERE s.idAdministrador = @idAdmin";

                if (fecha.HasValue)
                    query += " AND CAST(s.fechaSolicitud AS DATE) = @fecha";
                if (!string.IsNullOrEmpty(dni))
                    query += " AND u.DNI LIKE @dni";
                if (!string.IsNullOrEmpty(nombre))
                    query += " AND (a.nombre LIKE @nombre OR a.apellido LIKE @nombre)";
                if (idTipo.HasValue)
                    query += " AND s.idTipoSolicitud = @idTipo";
                if (idEstado.HasValue)
                    query += " AND s.idEstado = @idEstado";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idAdmin", idAdmin);
                if (fecha.HasValue) cmd.Parameters.AddWithValue("@fecha", fecha.Value.Date);
                if (!string.IsNullOrEmpty(dni)) cmd.Parameters.AddWithValue("@dni", "%" + dni + "%");
                if (!string.IsNullOrEmpty(nombre)) cmd.Parameters.AddWithValue("@nombre", "%" + nombre + "%");
                if (idTipo.HasValue) cmd.Parameters.AddWithValue("@idTipo", idTipo.Value);
                if (idEstado.HasValue) cmd.Parameters.AddWithValue("@idEstado", idEstado.Value);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Solicitud
                    {
                        Id = (int)reader["id"],
                        Cabecera = reader["cabecera"].ToString(),
                        Descripcion = reader["detalle"].ToString(),
                        FechaSolicitud = Convert.ToDateTime(reader["fechaSolicitud"]),
                        TipoSolicitud = reader["nombre_tipo"].ToString(),
                        EstadoSolicitud = reader["estado_nombre"].ToString(),
                        DNI = reader["DNI"].ToString(),
                        NombreAsegurado = reader["NombreAsegurado"].ToString()
                    });
                }

                connection.Close();
            }

            return lista;
        }

        public ArchivoAdjunto ObtenerArchivoPorId(int id)
        {
            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                var command = new SqlCommand("SELECT IdSolicitud, archivo_Ruta, archivo_nombre,fecha_subida FROM Archivos_Adjuntos WHERE Id = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ArchivoAdjunto
                        {
                            Id = id,
                            NombreOriginal = reader["archivo_nombre"].ToString(),
                            IdSolicitud = (int)reader["IdSolicitud"],
                            Ruta = reader["archivo_Ruta"].ToString(),
                        };
                    }
                }
            }
            return null;
        }

        public bool AnularSolicitud(int id, int idUsuario)
        {
            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var selectCmd = new SqlCommand("SELECT estado=idEstado FROM Solicitudes where id= @i", connection);
                    selectCmd.Parameters.AddWithValue("@id", id);
                    selectCmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                    var estado = selectCmd.ExecuteScalar()?.ToString();

                    if (estado == null)
                        return false;

                    if (estado == "PENDIENTE" || estado == "OBSERVADA")
                    {

                        var updateCmd = new SqlCommand("UPDATE Solicitudes SET idEstado = 6, FechaUltimaModificacion = GETDATE() WHERE Id = @id", connection, transaction);
                        updateCmd.Parameters.AddWithValue("@id", id);
                        updateCmd.ExecuteNonQuery();

                        var insertCmd = new SqlCommand(@"
                        INSERT INTO Modificaciones_Solicitud (idSolicitud, idAdministrador, idEstadoAnterior, idEstadoNuevo, fechaModificacion, comentario)
                        VALUES (@idSolicitud, @idAdministrador, @idEstadoAnterior, @idEstadoNuevo, GETDATE(), @comentario)", connection, transaction);

                        insertCmd.Parameters.AddWithValue("@idSolicitud", id);
                        insertCmd.Parameters.AddWithValue("@idAdministrador", 0);
                        insertCmd.Parameters.AddWithValue("@idEstadoAnterior", estado);
                        insertCmd.Parameters.AddWithValue("@idEstadoNuevo", 6);
                        insertCmd.Parameters.AddWithValue("@comentario", "Anulación realizada por el usuario");

                        insertCmd.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }

                    return false;
                }
                    
            }
            
        }



    }

}
