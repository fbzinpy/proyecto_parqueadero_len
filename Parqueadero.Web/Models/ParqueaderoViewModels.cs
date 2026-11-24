using Parqueadero.Core.Entidades;

namespace Parqueadero.Web.Models;

// Datos que necesita cada pantalla. El controlador los llena y la vista solo los muestra.

public class PanelViewModel
{
    public int TotalCupos { get; set; }
    public int CuposOcupados { get; set; }
    public List<Cupo> Cupos { get; set; } = new();
    public HashSet<int> IdsOcupados { get; set; } = new();
    public List<Ticket> VehiculosAdentro { get; set; } = new();   // ya filtrados por tipo y búsqueda
    public List<Ticket> TodosAdentro { get; set; } = new();       // sin filtrar: para el mapa y la tarjeta lateral
    public string? FiltroTipo { get; set; }   // null = todos, "Carro" o "Moto"
    public string? Buscar { get; set; }       // texto del buscador de placa
}

public class EntradaViewModel
{
    public List<Cupo> CuposLibres { get; set; } = new();
    public string? Placa { get; set; }
    public TipoVehiculo Tipo { get; set; } = TipoVehiculo.Carro;
    public int? CupoId { get; set; }
    public string? Error { get; set; }
}

public class SalidaViewModel
{
    public string? Placa { get; set; }
    public Ticket? Ticket { get; set; }       // null si todavía no se ha buscado o no se encontró
    public DateTime HoraSalida { get; set; }
    public int HorasCobradas { get; set; }
    public decimal ValorHora { get; set; }
    public decimal Total { get; set; }
    public string? Error { get; set; }
}

public class UsuarioFila
{
    public string Id { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class AdminViewModel
{
    public decimal IngresosDelDia { get; set; }
    public int VehiculosAtendidos { get; set; }
    public int CuposOcupados { get; set; }
    public int TotalCupos { get; set; }
    public int Operadores { get; set; }
    public int CuposCarro { get; set; }
    public int CuposMoto { get; set; }
    public List<Tarifa> Tarifas { get; set; } = new();
    public List<UsuarioFila> Usuarios { get; set; } = new();
}
