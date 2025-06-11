using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClienteWeb.Models
{
    public class PerfilViewModel
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string NumeroPoliza { get; set; }
        public string CentroAtencion { get; set; }
        public DateTime FechaAfiliacion { get; set; }
        public string RutaImagen { get; set; }
    }
}