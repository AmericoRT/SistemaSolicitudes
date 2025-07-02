using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Asegurado
    {
        public int IdAsegurado { get; set; }
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public Usuario Usuario { get; set; } // opcional
    }
}
