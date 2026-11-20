using System.Globalization;

namespace Parqueadero.Web.Models;

// Formatos que se repiten en varias vistas.
public static class Formato
{
    private static readonly CultureInfo Colombia = new CultureInfo("es-CO");

    // 3000 -> "$ 3.000"
    public static string Dinero(decimal valor)
    {
        return "$ " + valor.ToString("N0", Colombia);
    }

    // 2 h 33 min, o solo 45 min si es menos de una hora.
    public static string Duracion(TimeSpan tiempo)
    {
        int horas = (int)tiempo.TotalHours;
        if (horas == 0)
        {
            return $"{tiempo.Minutes} min";
        }
        return $"{horas} h {tiempo.Minutes:00} min";
    }
}
