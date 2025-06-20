using System.Collections.Generic;
using System;

namespace ClienteWeb.Models
{
    public class PerfilViewModel
    {
        // Información personal del usuario
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

        // Lista de solicitudes del usuario
        public List<Solicitud> Solicitudes { get; set; }
    }

    // Modelo de Solicitud
    public class Solicitud
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }
}
