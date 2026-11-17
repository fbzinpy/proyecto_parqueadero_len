namespace Parqueadero.Core.Entidades;

public class Tarifa
{
    public int Id { get; set; }
    public TipoVehiculo Tipo { get; set; }
    public decimal ValorHora { get; set; } // dinero siempre en decimal
}
