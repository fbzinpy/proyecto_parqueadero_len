using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Pantalla principal: barra superior con el usuario y "Cerrar sesión", y debajo
// entrada, salida y datos del cupo a la izquierda, y el plano de cupos a la derecha.
// La lógica está en Core (ServicioParqueadero, EstadoParqueadero, CalculadoraCobro);
// aquí solo se leen los campos, se llama a Core y se muestra el resultado.
public class FormPrincipal : Form
{
    private static readonly CultureInfo Colombia = new CultureInfo("es-CO");

    // Entrada
    private readonly TextBox _txtPlacaEntrada = Estilo.Campo(20, 78, 296, true);
    private readonly RadioButton _rbCarro = Estilo.Opcion("Carro", 20, 140, 144, 38);
    private readonly RadioButton _rbMoto = Estilo.Opcion("Moto", 172, 140, 144, 38);
    private readonly Label _lblMensajeEntrada = Estilo.Texto(20, 238, 296, 44, 15, Estilo.Exito);

    // Salida
    private readonly TextBox _txtPlacaSalida = Estilo.Campo(20, 78, 196, true);
    private readonly Label _lblInfoSalida = Estilo.Texto(20, 124, 296, 96, 16, Estilo.Carbon);
    private readonly Button _btnCobrar = Estilo.BotonOscuro("Cobrar y dar salida  →", 20, 226, 296, 46);
    private readonly Label _lblMensajeSalida = Estilo.Texto(20, 280, 296, 44, 15, Estilo.Exito);

    // Placa del ticket que se buscó. "Cobrar" usa esta, no lo que haya escrito después en el campo.
    private string? _placaEncontrada;

    // Cupo seleccionado
    private readonly Label _lblCodigo = Estilo.Titulo("—", 20, 40, 48, Estilo.Carbon);
    private readonly Label _lblDetalleCupo = Estilo.Texto(20, 104, 296, 96, 16, Estilo.Carbon);

    // Plano y ocupación
    private readonly PanelPlano _plano = new PanelPlano();
    private readonly Label _lblOcupacion = Estilo.Titulo("", 0, 46, 22, Estilo.Carbon);
    private readonly Panel _barraRelleno = new Panel { Dock = DockStyle.Left, BackColor = Estilo.Amarillo };
    private readonly Panel _barraFondo = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = Color.FromArgb(222, 222, 222) };
    private int _porcentajeOcupado;

    public FormPrincipal(string correo, string rol)
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Parqueadero";
        BackColor = Estilo.GrisClaro;
        Font = Estilo.Fuente(14);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized; // Abre ocupando toda la pantalla: así caben las tarjetas y el plano se ve más grande.

        // Tamaño inicial: el que quepa en la pantalla (en portátiles pequeños no se sale).
        Rectangle pantalla = Screen.PrimaryScreen!.WorkingArea;
        Size = new Size(Math.Min(1240, pantalla.Width), Math.Min(800, pantalla.Height));
        MinimumSize = new Size(Math.Min(1100, pantalla.Width), Math.Min(640, pantalla.Height));

        // El orden importa al acoplar (Dock): primero lo que ocupa el resto (Fill) y después los bordes.
        Panel cuerpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = Estilo.GrisClaro };
        Controls.Add(cuerpo);
        Controls.Add(CrearBarra(correo, rol));

        Panel derecha = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 0, 0, 0), BackColor = Estilo.GrisClaro };
        cuerpo.Controls.Add(derecha);
        cuerpo.Controls.Add(CrearColumnaIzquierda());

        _plano.Dock = DockStyle.Fill;
        derecha.Controls.Add(_plano);
        derecha.Controls.Add(CrearCabeceraMapa());

        _rbCarro.Checked = true;
        _rbCarro.CheckedChanged += (sender, e) => MostrarSeleccion();
        _plano.SeleccionCambio += (sender, e) => MostrarSeleccion();

        RefrescarMapa();
    }

    // ---------- Construcción de las zonas ----------

    private Panel CrearBarra(string correo, string rol)
    {
        Panel barra = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Estilo.Carbon, Padding = new Padding(24, 0, 24, 0) };

        Label marca = Estilo.Titulo("Parqueadero", 0, 0, 28, Estilo.Blanco);
        marca.Dock = DockStyle.Left;
        marca.AutoSize = false;
        marca.Width = 260;
        marca.TextAlign = ContentAlignment.MiddleLeft;

        // A la derecha: rol, correo y el botón de cerrar sesión (un panel que crece solo según su contenido).
        FlowLayoutPanel derecha = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Estilo.Carbon
        };

        Label lblRol = Estilo.Titulo(rol, 0, 0, 13, Estilo.Amarillo);
        lblRol.Margin = new Padding(0, 12, 0, 0);
        Label lblCorreo = Estilo.Titulo("", 0, 0, 16, Estilo.Blanco);
        lblCorreo.Text = correo; // El correo se muestra tal cual, sin pasarlo a mayúsculas.
        lblCorreo.Margin = new Padding(0);
        FlowLayoutPanel datos = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, 20, 0),
            BackColor = Estilo.Carbon
        };
        datos.Controls.Add(lblRol);
        datos.Controls.Add(lblCorreo);

        Button btnCerrar = Estilo.BotonBorde("Cerrar sesión", 0, 0, 160, 38, true);
        btnCerrar.Margin = new Padding(0, 13, 0, 0);
        btnCerrar.Click += (sender, e) => CerrarSesion();

        derecha.Controls.Add(datos);
        derecha.Controls.Add(btnCerrar);

        barra.Controls.Add(derecha);
        barra.Controls.Add(marca);
        return barra;
    }

    private FlowLayoutPanel CrearColumnaIzquierda()
    {
        FlowLayoutPanel columna = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 372,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Estilo.GrisClaro
        };

        columna.Controls.Add(CrearZonaEntrada());
        columna.Controls.Add(CrearZonaSalida());
        columna.Controls.Add(CrearZonaCupo());
        return columna;
    }

    private Panel CrearZonaEntrada()
    {
        Panel tarjeta = Estilo.Tarjeta(336, 296);

        Button btnRegistrar = Estilo.BotonAmarillo("Registrar entrada  →", 20, 188, 296, 46);
        btnRegistrar.Click += (sender, e) => RegistrarEntrada();

        // Enter en el campo de la placa equivale a hacer clic en el botón.
        _txtPlacaEntrada.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RegistrarEntrada();
            }
        };

        tarjeta.Controls.AddRange(new Control[]
        {
            Estilo.Titulo("Entrada", 20, 14, 28, Estilo.Carbon),
            Estilo.Etiqueta("Placa", 20, 58),
            _txtPlacaEntrada,
            Estilo.Etiqueta("Tipo de vehículo", 20, 120),
            _rbCarro,
            _rbMoto,
            btnRegistrar,
            _lblMensajeEntrada
        });
        return tarjeta;
    }

    private Panel CrearZonaSalida()
    {
        Panel tarjeta = Estilo.Tarjeta(336, 336);

        Button btnBuscar = Estilo.BotonBorde("Buscar", 226, 78, 90, 34, false);
        btnBuscar.Click += (sender, e) => BuscarTicket();

        _btnCobrar.Enabled = false; // Se activa cuando se encuentra un vehículo.
        _btnCobrar.Click += (sender, e) => CobrarYDarSalida();

        _txtPlacaSalida.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuscarTicket();
            }
        };

        // Si cambia la placa después de buscar, hay que buscar de nuevo antes de cobrar.
        _txtPlacaSalida.TextChanged += (sender, e) =>
        {
            if (_placaEncontrada != null && ServicioParqueadero.NormalizarPlaca(_txtPlacaSalida.Text) != _placaEncontrada)
            {
                LimpiarBusqueda();
            }
        };

        tarjeta.Controls.AddRange(new Control[]
        {
            Estilo.Titulo("Salida", 20, 14, 28, Estilo.Carbon),
            Estilo.Etiqueta("Placa", 20, 58),
            _txtPlacaSalida,
            btnBuscar,
            _lblInfoSalida,
            _btnCobrar,
            _lblMensajeSalida
        });
        return tarjeta;
    }

    private Panel CrearZonaCupo()
    {
        Panel tarjeta = Estilo.Tarjeta(336, 210);

        _lblDetalleCupo.Text = "Toca un cupo en el plano para ver su zona y qué tan lejos está de la portería.";
        _lblDetalleCupo.ForeColor = Estilo.Grafito;

        tarjeta.Controls.AddRange(new Control[]
        {
            Estilo.Etiqueta("Cupo seleccionado", 20, 16),
            _lblCodigo,
            _lblDetalleCupo
        });
        return tarjeta;
    }

    private Panel CrearCabeceraMapa()
    {
        Panel cabecera = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = Estilo.GrisClaro };

        // Lado izquierdo: título, cuántos hay ocupados y la barra de ocupación.
        Panel izquierda = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro };
        izquierda.Controls.Add(Estilo.Titulo("Mapa de cupos", 0, 0, 34, Estilo.Carbon));
        izquierda.Controls.Add(_lblOcupacion);

        // Lado derecho: leyenda y botón Actualizar.
        FlowLayoutPanel derecha = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Estilo.GrisClaro
        };
        derecha.Controls.Add(CrearLeyenda("Libre", Estilo.Blanco));
        derecha.Controls.Add(CrearLeyenda("Ocupado", Estilo.Carbon));
        derecha.Controls.Add(CrearLeyenda("Seleccionado", Estilo.Amarillo));

        Button btnActualizar = Estilo.BotonBorde("Actualizar", 0, 0, 130, 38, false);
        btnActualizar.Margin = new Padding(12, 2, 0, 0);
        btnActualizar.Click += (sender, e) => RefrescarMapa();
        derecha.Controls.Add(btnActualizar);

        // La barra se estira con la ventana: el relleno amarillo mide el porcentaje del ancho.
        _barraFondo.Controls.Add(_barraRelleno);
        _barraFondo.Resize += (sender, e) => AjustarBarra();

        // Fill primero, después los bordes.
        cabecera.Controls.Add(izquierda);
        cabecera.Controls.Add(derecha);
        cabecera.Controls.Add(_barraFondo);
        return cabecera;
    }

    // Un cuadrito de color con su nombre (libre, ocupado, seleccionado).
    private static Control CrearLeyenda(string texto, Color color)
    {
        FlowLayoutPanel item = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 10, 12, 0),
            BackColor = Estilo.GrisClaro
        };

        Panel muestra = new Panel { Size = new Size(16, 16), BackColor = color, Margin = new Padding(0, 3, 6, 0) };
        // Con el cuadro blanco hace falta un borde para que se vea sobre el fondo gris.
        muestra.Paint += (sender, e) =>
        {
            using (Pen borde = new Pen(Estilo.GrisBorde))
            {
                e.Graphics.DrawRectangle(borde, 0, 0, muestra.Width - 1, muestra.Height - 1);
            }
        };

        Label etiqueta = Estilo.Titulo(texto, 0, 0, 13, Estilo.Grafito);
        etiqueta.Margin = new Padding(0);

        item.Controls.Add(muestra);
        item.Controls.Add(etiqueta);
        return item;
    }

    // ---------- Acciones ----------

    private void CerrarSesion()
    {
        // Retry es la señal para el login: "volver a mostrarse" (cerrar la ventana con la X termina la aplicación).
        DialogResult = DialogResult.Retry;
    }

    private void RegistrarEntrada()
    {
        TipoVehiculo tipo = _rbCarro.Checked ? TipoVehiculo.Carro : TipoVehiculo.Moto;

        try
        {
            // Un scope nuevo por cada operación: el DbContext lee los datos frescos (también lo que se registró desde la web).
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();

                // Si hay un cupo libre seleccionado en el plano y es del tipo elegido, se usa ese.
                // Si no, null = el cupo se asigna automáticamente.
                Cupo? elegido = _plano.CupoSeleccionado;
                int? cupoId = null;
                if (elegido != null && elegido.Tipo == tipo && !_plano.EstaOcupado(elegido))
                {
                    cupoId = elegido.Id;
                }

                (Ticket? ticket, string? error) = servicio.RegistrarEntrada(_txtPlacaEntrada.Text, tipo, cupoId);

                if (error != null)
                {
                    Estilo.Mensaje(_lblMensajeEntrada, error, true);
                }
                else
                {
                    Estilo.Mensaje(_lblMensajeEntrada, $"Entrada registrada: {ticket!.Placa} en el cupo {ticket.Cupo!.Codigo}", false);
                    _txtPlacaEntrada.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensajeEntrada, "No se pudo registrar: " + ex.Message, true);
        }

        RefrescarMapa();
    }

    private void BuscarTicket()
    {
        LimpiarBusqueda();

        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                Ticket? ticket = servicio.BuscarTicketAbierto(_txtPlacaSalida.Text);

                if (ticket == null)
                {
                    Estilo.Mensaje(_lblMensajeSalida, "Ese vehículo no está en el parqueadero.", true);
                    return;
                }

                // Lo que pagaría si sale ahora: horas cobradas × valor de la hora.
                int horas = CalculadoraCobro.HorasCobradas(ticket.HoraEntrada, DateTime.Now);
                decimal total = horas * servicio.ValorHora(ticket.Tipo);

                _placaEncontrada = ticket.Placa;
                _lblInfoSalida.Text =
                    $"{ticket.Placa} · {ticket.Tipo} · cupo {ticket.Cupo!.Codigo}\n" +
                    $"Entró a las {ticket.HoraEntrada:dd/MM HH:mm}\n" +
                    $"Horas cobradas: {horas}\n" +
                    $"Total si sale ahora: {FormatoPesos(total)}";
                _btnCobrar.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensajeSalida, "No se pudo buscar: " + ex.Message, true);
        }
    }

    private void CobrarYDarSalida()
    {
        // Solo se cobra la placa que se buscó (el botón está desactivado hasta que se encuentra una).
        if (_placaEncontrada == null)
        {
            Estilo.Mensaje(_lblMensajeSalida, "Busca primero la placa.", true);
            return;
        }

        DialogResult respuesta = MessageBox.Show(
            $"¿Dar salida al vehículo {_placaEncontrada}?\n\n{_lblInfoSalida.Text.Split('\n').Last()}",
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

                if (ticket == null)
                {
                    LimpiarBusqueda();
                    Estilo.Mensaje(_lblMensajeSalida, "Ese vehículo ya no está en el parqueadero.", true);
                }
                else
                {
                    LimpiarBusqueda();
                    _txtPlacaSalida.Clear();
                    Estilo.Mensaje(_lblMensajeSalida, $"Salida registrada: {ticket.Placa}. Total cobrado: {FormatoPesos(ticket.Total ?? 0)}", false);
                }
            }
        }
        catch (Exception ex)
        {
            Estilo.Mensaje(_lblMensajeSalida, "No se pudo registrar la salida: " + ex.Message, true);
        }

        RefrescarMapa();
    }

    // Borra el resultado de la última búsqueda y desactiva el botón de cobrar.
    private void LimpiarBusqueda()
    {
        _placaEncontrada = null;
        _lblInfoSalida.Text = "";
        _lblMensajeSalida.Text = "";
        _btnCobrar.Enabled = false;
    }

    // ---------- Plano de cupos ----------

    // Lee los cupos y los ocupados de la base de datos y vuelve a dibujar el plano.
    private void RefrescarMapa()
    {
        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                List<Cupo> cupos = servicio.ObtenerCupos();
                EstadoParqueadero estado = servicio.CargarEstado();

                _plano.Actualizar(cupos, estado);

                _lblOcupacion.Text = $"{estado.CuposOcupados} de {estado.TotalCupos} ocupados".ToUpper();
                _porcentajeOcupado = estado.TotalCupos == 0 ? 0 : estado.CuposOcupados * 100 / estado.TotalCupos;
                AjustarBarra();
            }
        }
        catch (Exception ex)
        {
            _lblOcupacion.Text = "NO SE PUDO LEER LA BASE DE DATOS";
            MessageBox.Show("No se pudo leer el estado del parqueadero:\n" + ex.Message, "Parqueadero", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AjustarBarra()
    {
        _barraRelleno.Width = _barraFondo.Width * _porcentajeOcupado / 100;
    }

    // Muestra los datos del cupo seleccionado (se llama al tocar un cupo, al refrescar y al cambiar el tipo).
    private void MostrarSeleccion()
    {
        Cupo? cupo = _plano.CupoSeleccionado;
        PosicionCupo? posicion = _plano.PosicionSeleccionada;
        if (cupo == null || posicion == null)
        {
            return;
        }

        bool ocupado = _plano.EstaOcupado(cupo);
        TipoVehiculo tipoElegido = _rbCarro.Checked ? TipoVehiculo.Carro : TipoVehiculo.Moto;

        _lblCodigo.Text = cupo.Codigo;
        _lblDetalleCupo.ForeColor = Estilo.Carbon;
        _lblDetalleCupo.Text =
            $"{cupo.Tipo} · {(ocupado ? "Ocupado" : "Libre")}\n" +
            $"{posicion.Zona}\n" +
            $"A unos {PlanoParqueadero.MetrosAPorteria(posicion)} m de la portería";

        // Avisa qué pasará al registrar la entrada.
        if (!ocupado && cupo.Tipo == tipoElegido)
        {
            _lblDetalleCupo.Text += "\nSe usará al registrar la entrada.";
        }
    }

    // ---------- Ayudas de presentación ----------

    // Igual que en la web: "$ 12.000".
    private static string FormatoPesos(decimal valor)
    {
        return "$ " + valor.ToString("N0", Colombia);
    }
}
