using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Pantalla principal con tres zonas: entrada, salida y mapa de cupos.
// La lógica está en Core (ServicioParqueadero, EstadoParqueadero, CalculadoraCobro);
// aquí solo se leen los campos, se llama a Core y se muestra el resultado.
public class FormPrincipal : Form
{
    private static readonly CultureInfo Colombia = new CultureInfo("es-CO");
    private static readonly Color ColorLibre = Color.FromArgb(198, 239, 206);
    private static readonly Color ColorOcupado = Color.FromArgb(255, 199, 206);

    // Entrada
    private readonly TextBox _txtPlacaEntrada = new TextBox();
    private readonly ComboBox _cmbTipo = new ComboBox();
    private readonly Label _lblMensajeEntrada = new Label();

    // Salida
    private readonly TextBox _txtPlacaSalida = new TextBox();
    private readonly Label _lblInfoSalida = new Label();
    private readonly Label _lblMensajeSalida = new Label();

    // Mapa de cupos
    private readonly Label _lblOcupacion = new Label();
    private readonly FlowLayoutPanel _panelCupos = new FlowLayoutPanel();

    public FormPrincipal(string correo, string rol)
    {
        Text = "Parqueadero";
        ClientSize = new Size(1000, 600);
        MinimumSize = new Size(1016, 639);
        StartPosition = FormStartPosition.CenterScreen;

        Label lblUsuario = new Label
        {
            Text = $"Usuario: {correo}   |   Rol: {rol}",
            Location = new Point(10, 10),
            AutoSize = true
        };
        Controls.Add(lblUsuario);

        Controls.Add(CrearZonaEntrada());
        Controls.Add(CrearZonaSalida());
        CrearZonaMapa();

        RefrescarMapa();
    }

    // ---------- Construcción de las zonas ----------

    private GroupBox CrearZonaEntrada()
    {
        GroupBox grupo = new GroupBox { Text = "Entrada", Location = new Point(10, 40), Size = new Size(320, 180) };

        Label lblPlaca = new Label { Text = "Placa", Location = new Point(15, 25), AutoSize = true };
        _txtPlacaEntrada.Location = new Point(15, 45);
        _txtPlacaEntrada.Width = 290;

        Label lblTipo = new Label { Text = "Tipo", Location = new Point(15, 75), AutoSize = true };
        _cmbTipo.Location = new Point(15, 95);
        _cmbTipo.Width = 140;
        _cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList; // Solo se puede elegir, no escribir.
        _cmbTipo.Items.Add(TipoVehiculo.Carro);
        _cmbTipo.Items.Add(TipoVehiculo.Moto);
        _cmbTipo.SelectedIndex = 0;

        Button btnRegistrar = new Button { Text = "Registrar entrada", Location = new Point(165, 93), Width = 140, Height = 30 };
        btnRegistrar.Click += BtnRegistrarEntrada_Click;

        _lblMensajeEntrada.Location = new Point(15, 128);
        _lblMensajeEntrada.Size = new Size(290, 45);

        grupo.Controls.AddRange(new Control[] { lblPlaca, _txtPlacaEntrada, lblTipo, _cmbTipo, btnRegistrar, _lblMensajeEntrada });
        return grupo;
    }

    private GroupBox CrearZonaSalida()
    {
        GroupBox grupo = new GroupBox { Text = "Salida", Location = new Point(10, 230), Size = new Size(320, 270) };

        Label lblPlaca = new Label { Text = "Placa", Location = new Point(15, 25), AutoSize = true };
        _txtPlacaSalida.Location = new Point(15, 45);
        _txtPlacaSalida.Width = 190;

        Button btnBuscar = new Button { Text = "Buscar", Location = new Point(215, 43), Width = 90, Height = 30 };
        btnBuscar.Click += BtnBuscar_Click;

        _lblInfoSalida.Location = new Point(15, 80);
        _lblInfoSalida.Size = new Size(290, 95);

        Button btnCobrar = new Button { Text = "Cobrar y dar salida", Location = new Point(15, 180), Width = 290, Height = 30 };
        btnCobrar.Click += BtnCobrar_Click;

        _lblMensajeSalida.Location = new Point(15, 215);
        _lblMensajeSalida.Size = new Size(290, 45);

        grupo.Controls.AddRange(new Control[] { lblPlaca, _txtPlacaSalida, btnBuscar, _lblInfoSalida, btnCobrar, _lblMensajeSalida });
        return grupo;
    }

    private void CrearZonaMapa()
    {
        _lblOcupacion.Location = new Point(350, 45);
        _lblOcupacion.AutoSize = true;

        Button btnActualizar = new Button { Text = "Actualizar", Location = new Point(890, 38), Width = 100, Height = 30 };
        btnActualizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnActualizar.Click += (sender, e) => RefrescarMapa();

        // FlowLayoutPanel acomoda los botones uno tras otro y pasa a la siguiente fila cuando no caben.
        _panelCupos.Location = new Point(350, 75);
        _panelCupos.Size = new Size(640, 515);
        _panelCupos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _panelCupos.AutoScroll = true;
        _panelCupos.BorderStyle = BorderStyle.FixedSingle;

        Controls.Add(_lblOcupacion);
        Controls.Add(btnActualizar);
        Controls.Add(_panelCupos);
    }

    // ---------- Eventos ----------

    private void BtnRegistrarEntrada_Click(object? sender, EventArgs e)
    {
        TipoVehiculo tipo = (TipoVehiculo)_cmbTipo.SelectedItem!;

        // Un scope nuevo por cada clic: el DbContext lee los datos frescos (también lo que se registró desde la web).
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();

            // null = el cupo se asigna automáticamente.
            (Ticket? ticket, string? error) = servicio.RegistrarEntrada(_txtPlacaEntrada.Text, tipo, null);

            if (error != null)
            {
                MostrarMensaje(_lblMensajeEntrada, error, true);
            }
            else
            {
                MostrarMensaje(_lblMensajeEntrada, $"Entrada registrada: {ticket!.Placa} en el cupo {ticket.Cupo!.Codigo}", false);
            }
        }

        RefrescarMapa();
    }

    private void BtnBuscar_Click(object? sender, EventArgs e)
    {
        _lblMensajeSalida.Text = "";

        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
            Ticket? ticket = servicio.BuscarTicketAbierto(_txtPlacaSalida.Text);

            if (ticket == null)
            {
                _lblInfoSalida.Text = "";
                MostrarMensaje(_lblMensajeSalida, "Ese vehículo no está en el parqueadero.", true);
                return;
            }

            // Lo que pagaría si sale ahora: horas cobradas × valor de la hora.
            int horas = CalculadoraCobro.HorasCobradas(ticket.HoraEntrada, DateTime.Now);
            decimal total = horas * servicio.ValorHora(ticket.Tipo);

            _lblInfoSalida.Text =
                $"Placa: {ticket.Placa} ({ticket.Tipo}, cupo {ticket.Cupo!.Codigo})\n" +
                $"Hora de entrada: {ticket.HoraEntrada:dd/MM HH:mm}\n" +
                $"Horas cobradas: {horas}\n" +
                $"Total a pagar si sale ahora: {FormatoPesos(total)}";
        }
    }

    private void BtnCobrar_Click(object? sender, EventArgs e)
    {
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
            Ticket? ticket = servicio.RegistrarSalida(_txtPlacaSalida.Text);

            if (ticket == null)
            {
                MostrarMensaje(_lblMensajeSalida, "Ese vehículo no está en el parqueadero.", true);
                return;
            }

            _lblInfoSalida.Text = "";
            MostrarMensaje(_lblMensajeSalida, $"Salida registrada: {ticket.Placa}. Total cobrado: {FormatoPesos(ticket.Total ?? 0)}", false);
        }

        RefrescarMapa();
    }

    // ---------- Mapa de cupos ----------

    // Vuelve a crear un botón por cupo con el color según si está libre u ocupado.
    private void RefrescarMapa()
    {
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
            List<Cupo> cupos = servicio.ObtenerCupos();
            EstadoParqueadero estado = servicio.CargarEstado();

            // SuspendLayout/ResumeLayout: acomoda los 60 botones una sola vez al final, no uno por uno.
            _panelCupos.SuspendLayout();

            // Se liberan los botones anteriores. Dispose también los quita del panel.
            while (_panelCupos.Controls.Count > 0)
            {
                _panelCupos.Controls[0].Dispose();
            }

            foreach (Cupo cupo in cupos)
            {
                Button boton = new Button
                {
                    Text = cupo.Codigo,
                    Size = new Size(70, 40),
                    BackColor = estado.EstaOcupado(cupo.Id) ? ColorOcupado : ColorLibre
                };
                _panelCupos.Controls.Add(boton);
            }

            _panelCupos.ResumeLayout();

            _lblOcupacion.Text = $"Ocupados: {estado.CuposOcupados} de {estado.TotalCupos}";
        }
    }

    // ---------- Ayudas de presentación ----------

    private static void MostrarMensaje(Label etiqueta, string texto, bool esError)
    {
        etiqueta.Text = texto;
        etiqueta.ForeColor = esError ? Color.DarkRed : Color.DarkGreen;
    }

    // Igual que en la web: "$ 12.000".
    private static string FormatoPesos(decimal valor)
    {
        return "$ " + valor.ToString("N0", Colombia);
    }
}
