using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Historial: los vehículos que ya salieron, del más reciente al más antiguo. Pueden verlo los dos roles.
public class VistaHistorial : Vista
{
    private readonly Label _lblCantidad;
    private readonly Label _lblVacio = Estilo.Parrafo(0, 0, 400, 30, 16, Estilo.Texto2);
    private readonly DataGridView _tabla = Estilo.Tabla();

    public VistaHistorial(FormPrincipal shell) : base(shell)
    {
        Panel tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Tarjeta, Padding = new Padding(20) };
        Controls.Add(tarjeta);
        Controls.Add(Estilo.Cabecera("", "Historial", out _lblCantidad));

        Estilo.Columna(_tabla, "placa", "Placa", 16);
        Estilo.Columna(_tabla, "tipo", "Tipo", 10);
        Estilo.Columna(_tabla, "cupo", "Cupo", 10);
        Estilo.Columna(_tabla, "entrada", "Entrada", 16);
        Estilo.Columna(_tabla, "salida", "Salida", 16);
        Estilo.Columna(_tabla, "tiempo", "Tiempo", 14);
        Estilo.Columna(_tabla, "total", "Total", 18, true);

        _lblVacio.Dock = DockStyle.Top;
        _lblVacio.Height = 36;

        tarjeta.Controls.Add(_tabla);
        tarjeta.Controls.Add(_lblVacio);
    }

    public override void AlMostrar()
    {
        try
        {
            List<Ticket> historial;
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                historial = servicio.ObtenerHistorial();
            }

            _tabla.Rows.Clear();
            foreach (Ticket t in historial)
            {
                _tabla.Rows.Add(
                    t.Placa,
                    t.Tipo,
                    t.Cupo?.Codigo ?? "",
                    t.HoraEntrada.ToString("dd/MM HH:mm"),
                    t.HoraSalida?.ToString("dd/MM HH:mm") ?? "",
                    Formato.Duracion(t.HoraSalida!.Value - t.HoraEntrada),
                    Formato.Dinero(t.Total ?? 0));
            }
            _tabla.CurrentCell = null;
            _tabla.ClearSelection();

            _lblCantidad.Text = $"{historial.Count} salidas registradas".ToUpper();
            _lblVacio.Text = historial.Count == 0 ? "Todavía no hay salidas registradas." : "";
            _lblVacio.Visible = _lblVacio.Text.Length > 0;
        }
        catch (Exception ex)
        {
            _lblVacio.Text = "No se pudo leer el historial: " + ex.Message;
        }
    }
}
