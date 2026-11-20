using System.Diagnostics;
using Parqueadero.Core.Entidades;

namespace Parqueadero.Core.Servicios;

// Resultado de una comparación: cuánto tardó la estructura elegida y cuánto la alternativa.
public record ResultadoComparacion(
    string Operacion,
    string Elegida,
    double MilisegundosElegida,
    string Alternativa,
    double MilisegundosAlternativa);

// Mide con Stopwatch las estructuras que usa EstadoParqueadero contra una alternativa, para el reporte.
// Con solo 60 cupos todo tarda casi 0 ms, así que se usan muchos más elementos para que la diferencia se vea.
public static class ComparacionEstructuras
{
    public const int CantidadElementos = 10_000;
    public const int CantidadBusquedas = 10_000;

    public static List<ResultadoComparacion> Ejecutar()
    {
        return new List<ResultadoComparacion>
        {
            CompararBusquedaPorPlaca(),
            CompararCupoOcupado(),
            CompararAccesoPorPosicion()
        };
    }

    // Buscar un vehículo por su placa: Dictionary vs List.
    private static ResultadoComparacion CompararBusquedaPorPlaca()
    {
        List<Ticket> lista = new List<Ticket>();
        Dictionary<string, Ticket> diccionario = new Dictionary<string, Ticket>();

        for (int i = 0; i < CantidadElementos; i++)
        {
            Ticket ticket = new Ticket { Id = i, Placa = $"PLA{i:00000}" };
            lista.Add(ticket);
            diccionario.Add(ticket.Placa, ticket);
        }

        string[] placasBuscadas = CrearPlacasBuscadas();

        double msDiccionario = Medir(() =>
        {
            foreach (string placa in placasBuscadas)
            {
                diccionario.TryGetValue(placa, out _);
            }
        });

        double msLista = Medir(() =>
        {
            foreach (string placa in placasBuscadas)
            {
                lista.Find(t => t.Placa == placa);
            }
        });

        return new ResultadoComparacion("Buscar vehículo por placa", "Dictionary", msDiccionario, "List", msLista);
    }

    // Saber si un cupo está ocupado: HashSet vs List.
    private static ResultadoComparacion CompararCupoOcupado()
    {
        List<int> lista = new List<int>();
        HashSet<int> conjunto = new HashSet<int>();

        for (int i = 0; i < CantidadElementos; i++)
        {
            lista.Add(i);
            conjunto.Add(i);
        }

        int[] cuposBuscados = CrearPosiciones();

        double msHashSet = Medir(() =>
        {
            foreach (int id in cuposBuscados)
            {
                conjunto.Contains(id);
            }
        });

        double msLista = Medir(() =>
        {
            foreach (int id in cuposBuscados)
            {
                lista.Contains(id);
            }
        });

        return new ResultadoComparacion("Saber si un cupo está ocupado", "HashSet", msHashSet, "List", msLista);
    }

    // Tomar el cupo de una posición: arreglo vs LinkedList.
    // La LinkedList no tiene índice: para llegar a la posición i hay que recorrer i nodos (O(n)).
    private static ResultadoComparacion CompararAccesoPorPosicion()
    {
        Cupo[] arreglo = new Cupo[CantidadElementos];
        LinkedList<Cupo> enlazada = new LinkedList<Cupo>();

        for (int i = 0; i < CantidadElementos; i++)
        {
            Cupo cupo = new Cupo { Id = i, Codigo = $"C-{i}" };
            arreglo[i] = cupo;
            enlazada.AddLast(cupo);
        }

        int[] posiciones = CrearPosiciones();

        double msArreglo = Medir(() =>
        {
            foreach (int i in posiciones)
            {
                _ = arreglo[i];
            }
        });

        double msEnlazada = Medir(() =>
        {
            foreach (int i in posiciones)
            {
                _ = enlazada.ElementAt(i);
            }
        });

        return new ResultadoComparacion("Tomar el cupo de una posición", "Arreglo", msArreglo, "LinkedList", msEnlazada);
    }

    private static double Medir(Action accion)
    {
        Stopwatch reloj = Stopwatch.StartNew();
        accion();
        reloj.Stop();
        return reloj.Elapsed.TotalMilliseconds;
    }

    // Placas repartidas por toda la colección (no siempre la primera, que sería el mejor caso para la List).
    private static string[] CrearPlacasBuscadas()
    {
        string[] placas = new string[CantidadBusquedas];
        for (int i = 0; i < CantidadBusquedas; i++)
        {
            placas[i] = $"PLA{(i * 7919) % CantidadElementos:00000}";
        }
        return placas;
    }

    private static int[] CrearPosiciones()
    {
        int[] posiciones = new int[CantidadBusquedas];
        for (int i = 0; i < CantidadBusquedas; i++)
        {
            posiciones[i] = (i * 7919) % CantidadElementos;
        }
        return posiciones;
    }
}
