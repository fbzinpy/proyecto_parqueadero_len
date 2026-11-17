using Parqueadero.Core.Entidades;

namespace Parqueadero.Core.Servicios;

public static class AsignadorCupos
{
    // Devuelve el primer cupo libre del tipo pedido, o null si no hay ninguno.
    // cuposOcupados es un HashSet porque Contains es O(1): revisa un cupo sin recorrer la colección.
    // Con una List, Contains es O(n) y se repetiría por cada cupo revisado.
    public static Cupo? AsignarCupo(IEnumerable<Cupo> cupos, HashSet<int> cuposOcupados, TipoVehiculo tipo)
    {
        foreach (Cupo cupo in cupos)
        {
            if (cupo.Tipo == tipo && !cuposOcupados.Contains(cupo.Id))
            {
                return cupo;
            }
        }

        return null;
    }
}
