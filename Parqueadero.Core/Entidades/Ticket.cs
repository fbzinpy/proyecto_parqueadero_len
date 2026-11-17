namespace Parqueadero.Core.Entidades;

// Un ticket está abierto mientras HoraSalida sea null.
public class Ticket
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public TipoVehiculo Tipo { get; set; }
    public int CupoId { get; set; }
    public Cupo? Cupo { get; set; }
    public DateTime HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; } // null mientras el vehículo está adentro
    public decimal? Total { get; set; }       // null hasta que sale
}
