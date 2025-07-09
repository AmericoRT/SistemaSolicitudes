using AccesoDatos.Repositories;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class SolicitudService
    {
        private SolicitudRepository _solicitudRepository;

        public SolicitudService()
        {
            _solicitudRepository = new SolicitudRepository();  
        }


        public List<TipoSolicitud> ObtenerTiposSolicitud()
        {
            return _solicitudRepository.ObtenerTiposSolicitud();  
        }


        public void GuardarSolicitud(Solicitud solicitud)
        {
     
            _solicitudRepository.GuardarSolicitud(solicitud); 
        }

        public List<Solicitud> ObtenerPorIdUsuario(int idUsuario)
        {
            return _solicitudRepository.ObtenerSolicitudesPorUsuario(idUsuario);
        }

        public List<Solicitud> ObtenerPorIdUsuarioYFechas(int idUsuario, DateTime fechaInicio,DateTime fechaFin)
        {
            return _solicitudRepository.ObtenerSolicitudesPorUsuarioYFechas(idUsuario, fechaInicio, fechaFin);
        }

        public Solicitud ObtenerPorId(int id)
        {
            return _solicitudRepository.ObtenerSolicitudesPorId(id);
        }

        public List<Solicitud> ObtenerSolicitudesPorAdministrador(int idAdmin)
        {
            return _solicitudRepository.ObtenerSolicitudesPorAdministrador(idAdmin);
        }

        public List<Solicitud> ObtenerSolicitudesPendientes()
        {
            return _solicitudRepository.ObtenerSolicitudesPendientes();
        }
        public Solicitud ObtenerSolicitudPorId(int id) =>
            _solicitudRepository.ObtenerSolicitudPorId(id);

        public List<EstadoSolicitud> ObtenerEstadosSolicitud()
        {
            return _solicitudRepository.ObtenerEstadosSolicitud();
        }


        public void ActualizarEstadoSolicitud(int idSolicitud, int estadoAnterior, int estadoNuevo, int idAdmin, string comentario) =>
            _solicitudRepository.ActualizarEstadoSolicitud(idSolicitud, estadoAnterior, estadoNuevo, idAdmin, comentario);
        public List<Solicitud> FiltrarSolicitudes(DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            return _solicitudRepository.FiltrarSolicitudes(fecha, dni, nombre, idTipo, idEstado);
        }


        public List<Solicitud> ObtenerSolicitudesPorAdministradorFiltrado(
    int idAdmin, DateTime? fecha, string dni, string nombre, int? idTipo, int? idEstado)
        {
            return _solicitudRepository.ObtenerSolicitudesPorAdministradorFiltrado(idAdmin, fecha, dni, nombre, idTipo, idEstado);
        }
        public List<Solicitud> ObtenerPorIdAdministrador(int idAdministrador)
        {
            return _solicitudRepository.ObtenerSolicitudesPorAdministrador(idAdministrador);
        }



    }
}
