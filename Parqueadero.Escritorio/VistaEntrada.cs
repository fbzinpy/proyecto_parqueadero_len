using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Registrar entrada: placa, tipo y cupo (automático o uno de los libres). Pueden usarla los dos roles.
public class VistaEntrada : Vista
{
    // Un elemento de la lista de cupos: "Automático" (Cupo null) o un cupo libre.
    private class OpcionCupo
    {
        public Cupo? Cupo { get; init; }
        public string Texto { get; init; } = "";
        public override string ToString() => Texto;
    }

    private readonly Label _lblLibres;
    private readonly TextBox _txtPlaca = Estilo.Campo(28, 124, 504, true);
    private readonly RadioButton _rbCarro = Estilo.Opcion("Carro", 28, 200, 246, 42);
    private readonly RadioButton _rbMoto = Estilo.Opcion("Moto", 286, 200, 246, 42);
    private readonly ComboBox _cmbCupo = Estilo.Lista(28, 276, 504);
    private readonly Label _lblHora = Estilo.Texto(300, 328, 232, 26, 16, Estilo.Carbon);
    private readonly Label _lblMensaje = Estilo.Texto(28, 440, 504, 50, 15, Estilo.Exito);
    private List<Cupo> _libres = new List<Cupo>();

    public VistaEntrada(FormPrincipal shell) : base(shell)
    {
        Panel anfitrion = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro, AutoScroll = true };
        Controls.Add(anfitrion);
        Controls.Add(Estilo.Cabecera("Nuevo ingreso", "Registrar entrada", out _lblLibres));

        Panel tarjeta = Estilo.Tarjeta(560, 510);
        tarjeta.Location = new Point(0, 8);
        anfitrion.Controls.Add(tarjeta);

        _txtPlaca.Font = Estilo.Fuente(26);
        _txtPlaca.MaxLength = 8;
        _lblHora.TextAlign = ContentAlignment.MiddleRight;

        Button btnRegistrar = Estilo.BotonAmarillo("Registrar entrada  →", 28, 372, 504, 52);
        btnRegistrar.Click += (sender, e) => Registrar();

        // Enter en la placa equivale a hacer clic en el botón.
        _txtPlaca.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Registrar();
            }
        };

        _rbCarro.Checked = true;
        _rbCarro.CheckedChanged += (sender, e) => LlenarCupos(null);

        tarjeta.Controls.AddRange(new Control[]
        {
            Estilo.Titulo("Datos del vehículo", 28, 22, 30, Estilo.Carbon),
            Estilo.Etiqueta("Placa", 28, 100),
            _txtPlaca,
            Estilo.Etiqueta("Tipo de vehículo", 28, 176),
            _rbCarro,
            _rbMoto,
            Estilo.Etiqueta("Cupo asignado", 28, 252),
            _cmbCupo,
            Estilo.Etiqueta("Hora de entrada", 28, 330),
            _lblHora,
            btnRegistrar,
            _lblMensaje
        });

        Estilo.Centrar(anfitrion, tarjeta);
    }

    // Al abrir desde el menú: formulario limpio. Al venir del plano ("Registrar entrada aquí"): con ese cupo elegido.
    public override void AlMostrar()
    {
        Preparar(null);
    }

    public void Preparar(int? cupoId)
    {
        _lblMensaje.Text = "";
        _lblHora.Text = DateTime.Now.ToString("HH:mm");

        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                EstadoParqueadero estado = servicio.CargarEstado();
                _libres = servicio.ObtenerCupos().Where(c => !estado.EstaOcupado(c.Id)).ToList();
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensaje, "No se pudo leer la base de datos: " + ex.Message, true);
            return;
        }

        // Si viene un cupo libre, se elige también el tipo que le corresponde.
        Cupo? elegido = cupoId == null ? null : _libres.FirstOrDefault(c => c.Id == cupoId);
        if (elegido != null)
        {
            _rbCarro.Checked = elegido.Tipo == TipoVehiculo.Carro;
            _rbMoto.Checked = elegido.Tipo == TipoVehiculo.Moto;
        }

        LlenarCupos(elegido?.Id);
        _txtPlaca.Focus();
    }

    // Muestra en la lista solo los cupos libres del tipo elegido.
    private void LlenarCupos(int? cupoIdElegido)
    {
        int libresCarro = _libres.Count(c => c.Tipo == TipoVehiculo.Carro);
        int libresMoto = _libres.Count(c => c.Tipo == TipoVehiculo.Moto);
        _lblLibres.Text = $"{libresCarro} cupos libres para carro · {libresMoto} para moto".ToUpper();

        TipoVehiculo tipo = _rbCarro.Checked ? TipoVehiculo.Carro : TipoVehiculo.Moto;

        _cmbCupo.Items.Clear();
        _cmbCupo.Items.Add(new OpcionCupo { Cupo = null, Texto = "Automático · primer cupo libre" });
        foreach (Cupo cupo in _libres.Where(c => c.Tipo == tipo))
        {
            _cmbCupo.Items.Add(new OpcionCupo { Cupo = cupo, Texto = cupo.Codigo });
        }

        _cmbCupo.SelectedIndex = 0;
        if (cupoIdElegido != null)
        {
            for (int i = 1; i < _cmbCupo.Items.Count; i++)
            {
                if (((OpcionCupo)_cmbCupo.Items[i]!).Cupo?.Id == cupoIdElegido)
                {
                    _cmbCupo.SelectedIndex = i;
                }
            }
        }
    }

    private void Registrar()
    {
        TipoVehiculo tipo = _rbCarro.Checked ? TipoVehiculo.Carro : TipoVehiculo.Moto;
        int? cupoId = (_cmbCupo.SelectedItem as OpcionCupo)?.Cupo?.Id;

        try
        {
            // Un scope nuevo por operación: el DbContext lee datos frescos (también lo que se registró desde la web).
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                (Ticket? ticket, string? error) = servicio.RegistrarEntrada(_txtPlaca.Text, tipo, cupoId);

                if (error != null)
                {
                    Estilo.Mensaje(_lblMensaje, error, true);
                    return;
                }

                string mensaje = $"Entrada registrada: {ticket!.Placa} en el cupo {ticket.Cupo!.Codigo} a las {ticket.HoraEntrada:HH:mm}.";
                _txtPlaca.Clear();
                Preparar(null);
                Estilo.Mensaje(_lblMensaje, mensaje, false);
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensaje, "No se pudo registrar: " + ex.Message, true);
        }
    }
}
