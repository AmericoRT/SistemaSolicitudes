using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClienteWeb.ViewModels
{
    public class RegistroViewModel
    {
        [Required]
        [StringLength(10, MinimumLength = 7, ErrorMessage = "El DNI debe tener entre 7 y 10 caracteres.")]
        public string DNI { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Codigo { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Codigo", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarCodigo { get; set; }
    }
}