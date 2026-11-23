namespace Parqueadero.Core.Servicios;

// Posición de un cupo en el plano. X e Y son el CENTRO del cupo; la rotación va en grados.
public record PosicionCupo(string Codigo, double X, double Y, double Ancho, double Alto, double Rotacion, string Zona);

// Posiciones del plano ilustrado del parqueadero (coordenadas de un plano de 820 x 1000).
// Son dibujo, no datos del negocio: por eso se calculan aquí en código y no se guardan en la base de datos.
public static class PlanoParqueadero
{
    public const int Ancho = 820, Alto = 1000;

    public static List<PosicionCupo> ObtenerPosiciones()
    {
        List<PosicionCupo> lista = new List<PosicionCupo>();

        // A-01 a A-10: fila superior, vertical.
        for (int i = 0; i < 10; i++)
        {
            lista.Add(CarroVertical(175 + i * 51, 95, "Fila superior · al fondo"));
        }

        // A-11 a A-15: junto a la zona de motos, vertical.
        for (int i = 0; i < 5; i++)
        {
            lista.Add(CarroVertical(165 + i * 30, 192, "Junto a la zona de motos"));
        }

        // A-16 a A-35: centro, fila doble 1 (columnas X = 354 y 398).
        foreach (double x in new[] { 354.0, 398.0 })
        {
            for (int k = 0; k < 10; k++)
            {
                lista.Add(CarroHorizontal(x, 255 + k * 46, "Centro · fila doble 1"));
            }
        }

        // A-36 a A-55: centro, fila doble 2 (columnas X = 500 y 544).
        foreach (double x in new[] { 500.0, 544.0 })
        {
            for (int k = 0; k < 10; k++)
            {
                lista.Add(CarroHorizontal(x, 255 + k * 46, "Centro · fila doble 2"));
            }
        }

        // A-56 a A-80: costado de los árboles.
        for (int k = 0; k < 25; k++)
        {
            lista.Add(CarroHorizontal(655, 140 + k * 25, "Costado de los árboles · sombra"));
        }

        // A-81 a A-85: junto a la portería, vertical.
        for (int i = 0; i < 5; i++)
        {
            lista.Add(CarroVertical(280 + i * 34, 885, "Junto a la portería · salida rápida"));
        }

        // M-01 a M-20: zona de motos (columnas X = 190 y 250).
        int numero = 1;
        foreach (double x in new[] { 190.0, 250.0 })
        {
            for (int k = 0; k < 10; k++)
            {
                lista.Add(new PosicionCupo($"M-{numero:00}", x, 268 + k * 44, 34, 16, -22, "Zona de motos"));
                numero++;
            }
        }

        // Los códigos A-01 a A-85 se asignan en el orden en que se agregaron.
        for (int i = 0; i < 85; i++)
        {
            lista[i] = lista[i] with { Codigo = $"A-{i + 1:00}" };
        }

        return lista;
    }

    // Distancia aproximada en metros hasta la portería (el plano está a escala: 9.6 y 5.8 unidades por metro).
    public static int MetrosAPorteria(PosicionCupo p)
    {
        return (int)Math.Round(Math.Sqrt(Math.Pow((p.X - 130) / 9.6, 2) + Math.Pow((p.Y - 872) / 5.8, 2)));
    }

    private static PosicionCupo CarroVertical(double x, double y, string zona)
    {
        return new PosicionCupo("", x, y, 20, 40, 22, zona);
    }

    private static PosicionCupo CarroHorizontal(double x, double y, string zona)
    {
        return new PosicionCupo("", x, y, 40, 20, -22, zona);
    }
}
