namespace Parqueadero.Core.Servicios;

public static class CalculadoraCobro
{
    // Cobro por hora o fracción, mínimo 1 hora.
    public static decimal CalcularTotal(DateTime entrada, DateTime salida, decimal valorHora)
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

        return horas * valorHora;
    }
}
