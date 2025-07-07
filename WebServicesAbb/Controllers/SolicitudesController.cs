using System;
using System.Collections.Generic;
using System.Web.Http;
using Entidades;
using LogicaNegocio;

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

        // GET api/solicitudes/usuario/9
        [HttpGet]
        [Route("usuario/{idUsuario:int}")]
        public IHttpActionResult ObtenerSolicitudesPorUsuario(int idUsuario)
        {
            List<Solicitud> solicitudes = _service.ObtenerPorIdUsuario(idUsuario);

            if (solicitudes == null || solicitudes.Count == 0)
                return NotFound();

            return Ok(solicitudes);
        }

        [HttpGet]
        [Route("usuario/{idUsuario:int}/fechas")]
        public IHttpActionResult ObtenerSolicitudesPorUsuarioYFechas(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Validar fechas
                if (fechaInicio > fechaFin)
                    return BadRequest("La fecha de inicio no puede ser mayor a la fecha fin");

                List<Solicitud> solicitudes = _service.ObtenerPorIdUsuarioYFechas(idUsuario, fechaInicio, fechaFin);

                if (solicitudes == null || solicitudes.Count == 0)
                    return NotFound();

                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
