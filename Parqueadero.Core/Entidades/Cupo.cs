namespace Parqueadero.Core.Entidades;

// No tiene "Ocupado": un cupo está ocupado si tiene un ticket abierto.
public class Cupo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty; // Ej: A-01 (carros), M-01 (motos)
    public TipoVehiculo Tipo { get; set; }
}
