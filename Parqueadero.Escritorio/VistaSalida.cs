using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Registrar salida: se busca la placa, se ve cuánto debe pagar y se cobra. Pueden usarla los dos roles.
public class VistaSalida : Vista
{
    private readonly TextBox _txtPlaca = Estilo.Campo(28, 100, 392, true);
    private readonly Label _lblMensaje = Estilo.Texto(28, 148, 504, 50, 15, Estilo.Error);

    // Detalle del ticket (solo se ve cuando se encontró el vehículo)
    private readonly Panel _detalle = new Panel { Location = new Point(0, 150), Size = new Size(560, 480), BackColor = Estilo.Blanco, Visible = false };
    private readonly Dictionary<string, Label> _valores = new Dictionary<string, Label>();
    private readonly Label _lblTotal = Estilo.Titulo("", 28, 310, 56, Estilo.Carbon);
    private readonly Panel _tarjeta;

    // Placa del ticket que se buscó. "Cobrar" usa esta, no lo que se haya escrito después en el campo.
    private string? _placaEncontrada;

    public VistaSalida(FormPrincipal shell) : base(shell)
    {
        Panel anfitrion = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro, AutoScroll = true };
        Controls.Add(anfitrion);
        Controls.Add(Estilo.Cabecera("Cobro y salida", "Registrar salida", out _));

        _tarjeta = Estilo.Tarjeta(560, 700);
        _tarjeta.Location = new Point(0, 8);
        anfitrion.Controls.Add(_tarjeta);

        Button btnBuscar = Estilo.BotonBorde("Buscar", 432, 100, 100, 38, false);
        btnBuscar.Click += (sender, e) => Buscar();

        _txtPlaca.Font = Estilo.Fuente(24);
        _txtPlaca.MaxLength = 8;
        _txtPlaca.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Buscar();
            }
        };

        // Si cambia la placa después de buscar, hay que buscar de nuevo antes de cobrar.
        _txtPlaca.TextChanged += (sender, e) =>
        {
            if (_placaEncontrada != null && ServicioParqueadero.NormalizarPlaca(_txtPlaca.Text) != _placaEncontrada)
            {
                OcultarDetalle();
            }
        };

        _tarjeta.Controls.AddRange(new Control[]
        {
            Estilo.Titulo("Buscar vehículo", 28, 22, 30, Estilo.Carbon),
            _lblMensaje,
            Estilo.Etiqueta("Placa", 28, 78),
            _txtPlaca,
            btnBuscar,
            _detalle
        });

        CrearDetalle();
        Estilo.Centrar(anfitrion, _tarjeta);
    }

    private void CrearDetalle()
    {
        _detalle.Controls.Add(new Panel { Location = new Point(28, 0), Size = new Size(504, 1), BackColor = Estilo.GrisBorde });

        string[] filas = { "Placa", "Tipo", "Cupo", "Entrada", "Salida", "Horas cobradas", "Valor por hora" };
        int y = 16;
        foreach (string nombre in filas)
        {
            Label valor = Estilo.Texto(260, y, 272, 26, 17, Estilo.Carbon);
            valor.TextAlign = ContentAlignment.TopRight;
            _valores[nombre] = valor;
            _detalle.Controls.Add(Estilo.Etiqueta(nombre, 28, y + 3));
            _detalle.Controls.Add(valor);
            y += 32;
        }

        _detalle.Controls.Add(Estilo.Etiqueta("Total a pagar", 28, 288));
        _detalle.Controls.Add(_lblTotal);

        Button btnCobrar = Estilo.BotonOscuro("Cobrar y dar salida  →", 28, 404, 504, 52);
        btnCobrar.Click += (sender, e) => Cobrar();
        _detalle.Controls.Add(btnCobrar);
    }

    // Al abrir desde el menú: vacía. Al venir del panel ("Dar salida"): busca esa placa.
    public override void AlMostrar()
    {
        Preparar(null);
    }

    public void Preparar(string? placa)
    {
        OcultarDetalle();
        _lblMensaje.Text = "";
        _txtPlaca.Text = placa ?? "";

        if (!string.IsNullOrWhiteSpace(placa))
        {
            Buscar();
        }
        else
        {
            _txtPlaca.Focus();
        }
    }

    private void OcultarDetalle()
    {
        _placaEncontrada = null;
        _detalle.Visible = false;
    }

    private void Buscar()
    {
        OcultarDetalle();
        _lblMensaje.Text = "";

        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                Ticket? ticket = servicio.BuscarTicketAbierto(_txtPlaca.Text);

                if (ticket == null)
                {
                    string placa = ServicioParqueadero.NormalizarPlaca(_txtPlaca.Text);
                    Estilo.Mensaje(_lblMensaje, placa.Length == 0
                        ? "Escribe la placa del vehículo."
                        : $"No hay ningún vehículo adentro con la placa {placa}.", true);
                    return;
                }

                // Lo que pagaría si sale ahora: horas cobradas × valor de la hora.
                DateTime ahora = DateTime.Now;
                decimal valorHora = servicio.ValorHora(ticket.Tipo);
                int horas = CalculadoraCobro.HorasCobradas(ticket.HoraEntrada, ahora);

                _placaEncontrada = ticket.Placa;
                _valores["Placa"].Text = ticket.Placa;
                _valores["Tipo"].Text = ticket.Tipo.ToString();
                _valores["Cupo"].Text = ticket.Cupo!.Codigo;
                _valores["Entrada"].Text = ticket.HoraEntrada.ToString("dd/MM HH:mm");
                _valores["Salida"].Text = ahora.ToString("dd/MM HH:mm");
                _valores["Horas cobradas"].Text = $"{horas} ({Formato.Duracion(ahora - ticket.HoraEntrada)})";
                _valores["Valor por hora"].Text = Formato.Dinero(valorHora);
                _lblTotal.Text = Formato.Dinero(horas * valorHora);
                _detalle.Visible = true;
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensaje, "No se pudo buscar: " + ex.Message, true);
        }
    }

    private void Cobrar()
    {
        // Solo se cobra la placa que se buscó.
        if (_placaEncontrada == null)
        {
            Estilo.Mensaje(_lblMensaje, "Busca primero la placa.", true);
            return;
        }

        DialogResult respuesta = MessageBox.Show(
            $"¿Cobrar {_lblTotal.Text} y dar salida al vehículo {_placaEncontrada}?",
            "Cobrar y dar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (respuesta != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                Ticket? ticket = servicio.RegistrarSalida(_placaEncontrada);

                OcultarDetalle();
                if (ticket == null)
                {
                    Estilo.Mensaje(_lblMensaje, "Ese vehículo ya no está en el parqueadero.", true);
                    return;
                }

                _txtPlaca.Clear();
                Estilo.Mensaje(_lblMensaje, $"Salida registrada: {ticket.Placa} pagó {Formato.Dinero(ticket.Total ?? 0)}. El cupo {ticket.Cupo!.Codigo} quedó libre.", false);
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensaje, "No se pudo registrar la salida: " + ex.Message, true);
        }
    }
}
