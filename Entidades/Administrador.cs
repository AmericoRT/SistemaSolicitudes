﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Administrador
    {
        public int IdAdministrador { get; set; }
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public Usuario Usuario { get; set; } // opcional
    }
}
