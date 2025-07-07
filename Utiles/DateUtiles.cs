using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utiles
{
    public static class DateUtils
    {
        public static bool ValidarRango(string fechaInicio, string fechaFin, out string mensajeError, int diasMax = 365, bool permitirFuturas = false)
        {
            mensajeError = "";

            if (!string.IsNullOrEmpty(fechaInicio) && !DateTime.TryParse(fechaInicio, out _))
            {
                mensajeError = "El formato de la fecha de inicio es inválido.";
                return false;
            }

            if (!string.IsNullOrEmpty(fechaFin) && !DateTime.TryParse(fechaFin, out _))
            {
                mensajeError = "El formato de la fecha fin es inválido.";
                return false;
            }

            if (!string.IsNullOrEmpty(fechaInicio) && !string.IsNullOrEmpty(fechaFin))
            {
                var inicio = DateTime.Parse(fechaInicio);
                var fin = DateTime.Parse(fechaFin);

                if (inicio > fin)
                {
                    mensajeError = "La fecha de inicio no puede ser mayor a la fecha fin.";
                    return false;
                }

                if ((fin - inicio).TotalDays > diasMax)
                {
                    mensajeError = $"El rango de fechas no puede ser mayor a {diasMax} días.";
                    return false;
                }
            }

            if (!permitirFuturas)
            {
                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.Parse(fechaInicio) > DateTime.Now ||
                    !string.IsNullOrEmpty(fechaFin) && DateTime.Parse(fechaFin) > DateTime.Now)
                {
                    mensajeError = "Las fechas no pueden ser futuras.";
                    return false;
                }
            }

            return true;
        }
    }
}
