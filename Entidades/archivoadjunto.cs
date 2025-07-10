using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class ArchivoAdjunto
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; } 
        public string Ruta { get; set; }
        public string NombreOriginal { get; set; }

        public Solicitud Solicitud { get; set; }
    }
}
