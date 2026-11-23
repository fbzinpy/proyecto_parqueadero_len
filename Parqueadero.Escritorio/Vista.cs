namespace Parqueadero.Escritorio;

// Base de cada pantalla del menú (Panel, Entrada, Salida, Historial, Administración).
// FormPrincipal las pone en el mismo espacio y muestra una a la vez.
public abstract class Vista : UserControl
{
    protected readonly FormPrincipal Shell;

    protected Vista(FormPrincipal shell)
    {
        Shell = shell;
        Dock = DockStyle.Fill;
        BackColor = Estilo.GrisClaro;
        Padding = new Padding(32, 24, 32, 24);
    }

    // Se llama cada vez que la pantalla se muestra: aquí se leen los datos frescos de la base de datos.
    public virtual void AlMostrar()
    {
    }
}
