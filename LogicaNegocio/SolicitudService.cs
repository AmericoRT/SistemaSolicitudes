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

        public List<EstadoSolicitud> ObtenerEstadosSolicitud() =>
            _solicitudRepository.ObtenerEstadosSolicitud();

        public void ActualizarEstadoSolicitud(int idSolicitud, int estadoAnterior, int estadoNuevo, int idAdmin, string comentario) =>
            _solicitudRepository.ActualizarEstadoSolicitud(idSolicitud, estadoAnterior, estadoNuevo, idAdmin, comentario);


    }
}
