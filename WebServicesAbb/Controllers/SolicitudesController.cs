using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Entidades;
using LogicaNegocio;
using WebServicesAbb.Models;

namespace WebServicesAbb.Controllers
{
    [RoutePrefix("api/solicitudes")]
    public class SolicitudesController : ApiController
    {
        private readonly SolicitudService _service;

        public SolicitudesController()
        {
            _service = new SolicitudService(); 
        }

        //// GET api/solicitudes/usuario/9
        [HttpGet]
        [Route("usuario/{idUsuario:int}")]
        public IHttpActionResult ObtenerSolicitudesPorUsuario(int idUsuario, string fechaInicio = null, string fechaFin = null, string estado = null)
        {
            try
            {
                List<Solicitud> solicitudes = _service.ObtenerPorIdUsuario(idUsuario);

                if (solicitudes == null || solicitudes.Count == 0)
                    return NotFound();

                var solicitudesFiltradas = solicitudes.AsQueryable();

                // DEBUG: Agregar logging temporal
                int totalOriginal = solicitudes.Count;
                System.Diagnostics.Debug.WriteLine($"Total solicitudes originales: {totalOriginal}");

                if (!string.IsNullOrEmpty(fechaInicio))
                    System.Diagnostics.Debug.WriteLine($"Filtro fecha inicio: {fechaInicio}");

                if (!string.IsNullOrEmpty(fechaFin))
                    System.Diagnostics.Debug.WriteLine($"Filtro fecha fin: {fechaFin}");

                // Filtrar por fecha inicio
                if (!string.IsNullOrEmpty(fechaInicio))
                {
                    if (DateTime.TryParse(fechaInicio, out DateTime fechaInicioDate))
                    {
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud >= fechaInicioDate);
                        System.Diagnostics.Debug.WriteLine($"Después de filtro inicio: {solicitudesFiltradas.Count()}");
                    }
                }

                // Filtrar por fecha fin
                if (!string.IsNullOrEmpty(fechaFin))
                {
                    if (DateTime.TryParse(fechaFin, out DateTime fechaFinDate))
                    {
                        // Incluir todo el día hasta las 23:59:59
                        fechaFinDate = fechaFinDate.Date.AddDays(1).AddTicks(-1);
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud <= fechaFinDate);
                        System.Diagnostics.Debug.WriteLine($"Después de filtro fin: {solicitudesFiltradas.Count()}");
                    }
                }

                // Filtrar por estado
                if (!string.IsNullOrEmpty(estado))
                {
                    // Si estado es un ID numérico, compara con IdEstado
                    if (int.TryParse(estado, out int estadoId))
                    {
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.IdEstado == estadoId);
                    }
                    else
                    {
                        // Si es texto, compara con el nombre del estado
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.EstadoSolicitud.Equals(estado, StringComparison.OrdinalIgnoreCase));
                    }
                    System.Diagnostics.Debug.WriteLine($"Después de filtro estado: {solicitudesFiltradas.Count()}");
                }

                solicitudes = solicitudesFiltradas.ToList();
                System.Diagnostics.Debug.WriteLine($"Total final: {solicitudes.Count}");

                return Ok(solicitudesFiltradas.ToList());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("solicitudes/{id:int}")]
        public IHttpActionResult ObtenerSolicitudPorId(int id)
        {
            try
            {
                Solicitud solicitud = _service.ObtenerPorId(id);

                if (solicitud == null)
                    return NotFound();

                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("administrador/{idAdministrador:int}")]
        public IHttpActionResult ObtenerSolicitudesPorAdministrador(
    int idAdministrador,
    string fechaInicio = null,
    string fechaFin = null,
    string estado = null,
    int? tipo = null,
    string dni = null,
    string nombre = null)
        {
            try
            {
                var solicitudes = _service.ObtenerPorIdAdministrador(idAdministrador);

                if (solicitudes == null || solicitudes.Count == 0)
                    return NotFound();

                var solicitudesFiltradas = solicitudes.AsQueryable();

                // Filtrar por fecha inicio
                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out DateTime fi))
                {
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud >= fi);
                }

                // Filtrar por fecha fin
                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out DateTime ff))
                {
                    ff = ff.Date.AddDays(1).AddTicks(-1); // incluir todo el día
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.FechaSolicitud <= ff);
                }

                // Filtrar por estado (ID o nombre)
                if (!string.IsNullOrEmpty(estado))
                {
                    if (int.TryParse(estado, out int idEstado))
                    {
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.IdEstado == idEstado);
                    }
                    else
                    {
                        solicitudesFiltradas = solicitudesFiltradas.Where(s => s.EstadoSolicitud != null &&
                                                                               s.EstadoSolicitud.Equals(estado, StringComparison.OrdinalIgnoreCase));
                    }
                }

                // Filtrar por tipo
                if (tipo.HasValue)
                {
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.IdTipoSolicitud == tipo.Value);
                }

                // Filtrar por DNI
                if (!string.IsNullOrEmpty(dni))
                {
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.DNI != null && s.DNI.Contains(dni));
                }

                // Filtrar por nombre del asegurado
                if (!string.IsNullOrEmpty(nombre))
                {
                    solicitudesFiltradas = solicitudesFiltradas.Where(s => s.NombreAsegurado != null &&
                                                                           s.NombreAsegurado.ToLower().Contains(nombre.ToLower()));
                }

                return Ok(solicitudesFiltradas.ToList());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [Route("actualizarEstado")]
        public IHttpActionResult ActualizarEstado([FromBody] EstadoSolicitudDto dto)
        {
            try
            {
                var service = new SolicitudService();
                service.ActualizarEstadoSolicitud(dto.IdSolicitud, dto.EstadoAnterior, dto.EstadoNuevo, dto.IdAdministrador, dto.Comentario);
                return Ok(new { mensaje = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("pendientes-admin")]
        public IHttpActionResult ObtenerSolicitudesPendientesAdmin(
    string fechaInicio = null,
    string fechaFin = null,
    string estado = null,
    int? tipo = null,
    string dni = null,
    string nombre = null)
        {
            try
            {
                var solicitudes = _service.ObtenerSolicitudesPendientes(); // Llama al repository

                var filtradas = solicitudes.AsQueryable();

                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out var fi))
                    filtradas = filtradas.Where(s => s.FechaSolicitud >= fi);

                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out var ff))
                {
                    ff = ff.Date.AddDays(1).AddTicks(-1);
                    filtradas = filtradas.Where(s => s.FechaSolicitud <= ff);
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    if (int.TryParse(estado, out int idEstado))
                        filtradas = filtradas.Where(s => s.IdEstado == idEstado);
                    else
                        filtradas = filtradas.Where(s => s.EstadoSolicitud.Equals(estado, StringComparison.OrdinalIgnoreCase));
                }

                if (tipo.HasValue)
                    filtradas = filtradas.Where(s => s.IdTipoSolicitud == tipo.Value);

                if (!string.IsNullOrEmpty(dni))
                    filtradas = filtradas.Where(s => s.DNI != null && s.DNI.Contains(dni));

                if (!string.IsNullOrEmpty(nombre))
                    filtradas = filtradas.Where(s => s.NombreAsegurado != null && s.NombreAsegurado.ToLower().Contains(nombre.ToLower()));

                return Ok(filtradas.ToList());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



    }
}
