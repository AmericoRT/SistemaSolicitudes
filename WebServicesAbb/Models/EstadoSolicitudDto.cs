using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServicesAbb.Models
{
    public class EstadoSolicitudDto
    {
        public int IdSolicitud { get; set; }
        public int IdAdministrador { get; set; }
        public int EstadoAnterior { get; set; }
        public int EstadoNuevo { get; set; }
        public string Comentario { get; set; }
    }
}
