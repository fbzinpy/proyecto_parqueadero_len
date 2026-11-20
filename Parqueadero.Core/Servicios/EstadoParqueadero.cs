using Parqueadero.Core.Entidades;

namespace Parqueadero.Core.Servicios;

// Estado del parqueadero en memoria: qué cupos hay, cuáles están ocupados y qué vehículos están adentro.
// Se arma con los datos de la base de datos (cupos y tickets abiertos) y lo pueden usar la web y el escritorio.
public class EstadoParqueadero
{
    // Arreglo: el parqueadero tiene una cantidad fija de cupos (60) que no cambia mientras el programa corre.
    // Un arreglo ocupa justo ese espacio y se accede por posición en O(1).
    // Una List también accede en O(1), pero está pensada para crecer, y aquí no hace falta.
    private readonly Cupo[] _cupos;

    // Dictionary placa -> ticket abierto: al registrar una salida se busca el vehículo por placa en O(1).
    // Con una List habría que recorrerla ticket por ticket (O(n)).
    // Además, la clave es única, así que la misma placa no puede estar adentro dos veces.
    // OrdinalIgnoreCase: "abc123" y "ABC123" se toman como la misma placa.
    private readonly Dictionary<string, Ticket> _vehiculosAdentro;

    // HashSet con los Id de los cupos ocupados: saber si un cupo está ocupado es O(1).
    // Con una List, Contains sería O(n). No se necesitan repetidos ni orden, justo lo que da un HashSet.
    private readonly HashSet<int> _cuposOcupados;

    public EstadoParqueadero(IEnumerable<Cupo> cupos, IEnumerable<Ticket> ticketsAbiertos)
    {
        _cupos = cupos.ToArray();
        _vehiculosAdentro = new Dictionary<string, Ticket>(StringComparer.OrdinalIgnoreCase);
        _cuposOcupados = new HashSet<int>();

        foreach (Ticket ticket in ticketsAbiertos)
        {
            _vehiculosAdentro[ticket.Placa] = ticket;
            _cuposOcupados.Add(ticket.CupoId);
        }
    }

    public int TotalCupos => _cupos.Length;
    public int CuposOcupados => _cuposOcupados.Count;

    public bool EstaAdentro(string placa)
    {
        return _vehiculosAdentro.ContainsKey(placa);
    }

    public bool EstaOcupado(int cupoId)
    {
        return _cuposOcupados.Contains(cupoId);
    }

    // Devuelve el ticket abierto de esa placa, o null si el vehículo no está adentro.
    public Ticket? BuscarTicket(string placa)
    {
        _vehiculosAdentro.TryGetValue(placa, out Ticket? ticket);
        return ticket;
    }

    // Primer cupo libre del tipo pedido, o null si está lleno.
    public Cupo? BuscarCupoLibre(TipoVehiculo tipo)
    {
        return AsignadorCupos.AsignarCupo(_cupos, _cuposOcupados, tipo);
    }

    // Marca el vehículo como adentro y su cupo como ocupado.
    public void RegistrarEntrada(Ticket ticket)
    {
        if (EstaAdentro(ticket.Placa))
        {
            throw new InvalidOperationException($"El vehículo {ticket.Placa} ya está adentro.");
        }

        _vehiculosAdentro.Add(ticket.Placa, ticket);
        _cuposOcupados.Add(ticket.CupoId);
    }

    // Saca el vehículo y libera su cupo. Devuelve el ticket que estaba abierto, o null si no estaba adentro.
    public Ticket? RegistrarSalida(string placa)
    {
        if (!_vehiculosAdentro.Remove(placa, out Ticket? ticket))
        {
            return null;
        }

        _cuposOcupados.Remove(ticket.CupoId);
        return ticket;
    }
}
