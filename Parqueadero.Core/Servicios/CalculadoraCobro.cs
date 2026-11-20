namespace Parqueadero.Core.Servicios;

public static class CalculadoraCobro
{
    // Horas que se cobran: por hora o fracción, mínimo 1 hora.
    // Ej: 10 min -> 1 h, 60 min -> 1 h, 61 min -> 2 h.
    public static int HorasCobradas(DateTime entrada, DateTime salida)
    {
        if (salida < entrada)
        {
            throw new ArgumentException("La hora de salida no puede ser anterior a la de entrada.");
        }

        double minutos = (salida - entrada).TotalMinutes;
        int horas = (int)Math.Ceiling(minutos / 60);

        if (horas == 0)
        {
            horas = 1;
        }

        return horas;
    }

    public static decimal CalcularTotal(DateTime entrada, DateTime salida, decimal valorHora)
    {
        return HorasCobradas(entrada, salida) * valorHora;
    }
}
