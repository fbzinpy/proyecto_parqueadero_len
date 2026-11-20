namespace Parqueadero.Core.Datos;

// La web y el escritorio deben usar el mismo archivo .db.
// Por eso el archivo se guarda en la carpeta de la solución (donde está Parqueadero.slnx),
// sin importar desde qué proyecto se ejecute el programa.
public static class RutaBaseDatos
{
    public const string NombreArchivo = "parqueadero.db";

    public static string Obtener()
    {
        // Se empieza en la carpeta del ejecutable (bin/Debug/...) y se sube hasta encontrar la solución.
        DirectoryInfo? carpeta = new DirectoryInfo(AppContext.BaseDirectory);

        while (carpeta != null && !File.Exists(Path.Combine(carpeta.FullName, "Parqueadero.slnx")))
        {
            carpeta = carpeta.Parent;
        }

        if (carpeta == null)
        {
            throw new InvalidOperationException("No se encontró la carpeta de la solución (Parqueadero.slnx).");
        }

        return Path.Combine(carpeta.FullName, NombreArchivo);
    }

    public static string CadenaConexion()
    {
        return $"Data Source={Obtener()}";
    }
}
