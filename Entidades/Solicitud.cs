using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string TipoSolicitud { get; set; }
        public string EstadoSolicitud { get; set; }  
        public string Cabecera { get; set; } 
        public string Descripcion { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string DocumentoAdjuntoRuta { get; set; }

        public int IdAsegurado { get; set; }
        public int? IdAdministrador { get; set; }  // nullable
        public int IdTipoSolicitud { get; set; }
        public int IdEstado { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }

        public string Observacion { get; set; }
    }

}
