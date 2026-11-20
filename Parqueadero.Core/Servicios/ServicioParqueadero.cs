using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Datos;
using Parqueadero.Core.Entidades;

namespace Parqueadero.Core.Servicios;

// Lógica del parqueadero que usa la base de datos: entradas, salidas, historial, tarifas y resumen.
// Está en Core para que la web y el escritorio usen exactamente las mismas reglas.
public class ServicioParqueadero
{
    private readonly ParqueaderoDbContext _db;

    public ServicioParqueadero(ParqueaderoDbContext db)
    {
        _db = db;
    }

    // ---------- Placas ----------

    // "abc 123" o "abc-123" -> "ABC123"
    public static string NormalizarPlaca(string? placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return string.Empty;
        }

        return placa.Replace(" ", "").Replace("-", "").Trim().ToUpperInvariant();
    }

    // Formatos de Colombia: carro ABC123, moto ABC12 o ABC12D.
    public static bool PlacaValida(string placa, TipoVehiculo tipo)
    {
        string patron = tipo == TipoVehiculo.Carro ? "^[A-Z]{3}[0-9]{3}$" : "^[A-Z]{3}[0-9]{2}[A-Z]?$";
        return Regex.IsMatch(placa, patron);
    }

    // ---------- Consultas ----------

    // Arma el estado en memoria (arreglo de cupos, HashSet de ocupados, Dictionary por placa).
    public EstadoParqueadero CargarEstado()
    {
        List<Cupo> cupos = _db.Cupos.OrderBy(c => c.Id).ToList();
        List<Ticket> abiertos = _db.Tickets.Where(t => t.HoraSalida == null).ToList();
        return new EstadoParqueadero(cupos, abiertos);
    }

    public List<Cupo> ObtenerCupos()
    {
        return _db.Cupos.OrderBy(c => c.Id).ToList();
    }

    // Vehículos que están adentro, del que entró primero al último.
    public List<Ticket> ObtenerTicketsAbiertos()
    {
        return _db.Tickets
            .Include(t => t.Cupo)
            .Where(t => t.HoraSalida == null)
            .OrderBy(t => t.HoraEntrada)
            .ToList();
    }

    public Ticket? BuscarTicketAbierto(string placa)
    {
        string placaNormalizada = NormalizarPlaca(placa);
        return _db.Tickets
            .Include(t => t.Cupo)
            .FirstOrDefault(t => t.Placa == placaNormalizada && t.HoraSalida == null);
    }

    // Tickets cerrados, del más reciente al más antiguo.
    public List<Ticket> ObtenerHistorial()
    {
        return _db.Tickets
            .Include(t => t.Cupo)
            .Where(t => t.HoraSalida != null)
            .OrderByDescending(t => t.HoraSalida)
            .ToList();
    }

    // ---------- Entrada ----------

    // Devuelve el ticket creado, o un mensaje de error si no se pudo registrar.
    // cupoId null = se asigna automáticamente el primer cupo libre del tipo.
    public (Ticket? Ticket, string? Error) RegistrarEntrada(string? placa, TipoVehiculo tipo, int? cupoId)
    {
        string placaNormalizada = NormalizarPlaca(placa);

        if (!Enum.IsDefined(tipo))
        {
            return (null, "Elige un tipo de vehículo válido.");
        }

        if (placaNormalizada.Length == 0)
        {
            return (null, "Escribe la placa del vehículo.");
        }

        if (!PlacaValida(placaNormalizada, tipo))
        {
            string ejemplo = tipo == TipoVehiculo.Carro ? "ABC123" : "ABC12D";
            return (null, $"La placa {placaNormalizada} no es válida para {tipo.ToString().ToLower()}. Ejemplo: {ejemplo}.");
        }

        EstadoParqueadero estado = CargarEstado();

        if (estado.EstaAdentro(placaNormalizada))
        {
            return (null, $"El vehículo {placaNormalizada} ya está adentro.");
        }

        Cupo? cupo;
        if (cupoId == null)
        {
            cupo = estado.BuscarCupoLibre(tipo);
            if (cupo == null)
            {
                return (null, $"No hay cupos libres para {tipo.ToString().ToLower()}.");
            }
        }
        else
        {
            cupo = _db.Cupos.Find(cupoId.Value);
            if (cupo == null)
            {
                return (null, "El cupo elegido no existe.");
            }
            if (cupo.Tipo != tipo)
            {
                return (null, $"El cupo {cupo.Codigo} no es para {tipo.ToString().ToLower()}.");
            }
            if (estado.EstaOcupado(cupo.Id))
            {
                return (null, $"El cupo {cupo.Codigo} ya está ocupado.");
            }
        }

        Ticket ticket = new Ticket
        {
            Placa = placaNormalizada,
            Tipo = tipo,
            CupoId = cupo.Id,
            Cupo = cupo,
            HoraEntrada = DateTime.Now
        };

        _db.Tickets.Add(ticket);
        _db.SaveChanges();
        return (ticket, null);
    }

    // ---------- Salida ----------

    // Cierra el ticket: guarda la hora de salida y el total. Devuelve null si la placa no está adentro.
    public Ticket? RegistrarSalida(string? placa)
    {
        Ticket? ticket = BuscarTicketAbierto(placa ?? "");
        if (ticket == null)
        {
            return null;
        }

        DateTime salida = DateTime.Now;
        ticket.HoraSalida = salida;
        ticket.Total = CalculadoraCobro.CalcularTotal(ticket.HoraEntrada, salida, ValorHora(ticket.Tipo));
        _db.SaveChanges();
        return ticket;
    }

    // ---------- Tarifas ----------

    public List<Tarifa> ObtenerTarifas()
    {
        return _db.Tarifas.OrderBy(t => t.Tipo).ToList();
    }

    public decimal ValorHora(TipoVehiculo tipo)
    {
        Tarifa? tarifa = _db.Tarifas.FirstOrDefault(t => t.Tipo == tipo);
        if (tarifa == null)
        {
            throw new InvalidOperationException($"No hay tarifa para {tipo}.");
        }
        return tarifa.ValorHora;
    }

    public string? ActualizarTarifas(decimal valorCarro, decimal valorMoto)
    {
        if (valorCarro <= 0 || valorMoto <= 0)
        {
            return "Las tarifas deben ser mayores que 0.";
        }

        foreach (Tarifa tarifa in _db.Tarifas)
        {
            tarifa.ValorHora = tarifa.Tipo == TipoVehiculo.Carro ? valorCarro : valorMoto;
        }

        _db.SaveChanges();
        return null;
    }

    // ---------- Resumen del día ----------

    // Salidas de hoy y lo que se cobró en ellas.
    public (decimal Ingresos, int VehiculosAtendidos) ResumenDelDia()
    {
        DateTime hoy = DateTime.Today;
        DateTime manana = hoy.AddDays(1);

        // SQLite no puede sumar decimal dentro de la consulta, por eso se suma en memoria (con LINQ sobre la lista).
        List<Ticket> salidasDeHoy = _db.Tickets
            .Where(t => t.HoraSalida >= hoy && t.HoraSalida < manana)
            .ToList();

        decimal ingresos = salidasDeHoy.Sum(t => t.Total ?? 0);
        return (ingresos, salidasDeHoy.Count);
    }
}
