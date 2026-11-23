using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Panel: cuántos cupos hay ocupados, el plano del parqueadero y los vehículos que están adentro.
// Pueden verlo los dos roles.
public class VistaPanel : Vista
{
    // Cabecera
    private readonly Label _lblOcupacion = Estilo.Titulo("", 0, 0, 42, Estilo.Carbon);
    private readonly Label _lblLibres = Estilo.Etiqueta("", 0, 62);
    private readonly Panel _barraRelleno = new Panel { Dock = DockStyle.Left, BackColor = Estilo.Amarillo };
    private readonly Panel _barraFondo = new Panel { Dock = DockStyle.Bottom, Height = 6, BackColor = Color.FromArgb(222, 222, 222) };
    private int _porcentajeOcupado;

    // Plano y cupo seleccionado
    private readonly PanelPlano _plano = new PanelPlano();
    private readonly Label _lblCodigo = Estilo.Titulo("—", 20, 40, 46, Estilo.Carbon);
    private readonly Label _lblDetalle = Estilo.Texto(20, 100, 340, 80, 16, Estilo.Grafito);
    private readonly Button _btnEntradaAqui = Estilo.BotonAmarillo("Registrar entrada aquí  →", 20, 186, 340, 44);
    private readonly Button _btnSalidaAqui = Estilo.BotonOscuro("Dar salida  →", 20, 186, 340, 44);

    // Vehículos dentro
    private readonly DataGridView _tabla = Estilo.Tabla();
    private readonly RadioButton _rbTodos = Estilo.Opcion("Todos", 0, 0, 90, 36);
    private readonly RadioButton _rbCarro = Estilo.Opcion("Carro", 0, 0, 90, 36);
    private readonly RadioButton _rbMoto = Estilo.Opcion("Moto", 0, 0, 90, 36);
    private readonly TextBox _txtBuscar = Estilo.Campo(0, 0, 200, true);
    private readonly Label _lblVacio = Estilo.Texto(0, 0, 400, 30, 16, Estilo.Grafito);
    private List<Ticket> _abiertos = new List<Ticket>();

    // Se vuelve a leer la base de datos cada 10 segundos, para ver lo que se registra desde la web.
    private readonly System.Windows.Forms.Timer _temporizador = new System.Windows.Forms.Timer { Interval = 10000 };

    public VistaPanel(FormPrincipal shell) : base(shell)
    {
        // Primero lo que ocupa el resto (Fill) y después la cabecera.
        Panel cuerpo = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro, Padding = new Padding(0, 16, 0, 0) };
        Controls.Add(cuerpo);
        Controls.Add(CrearCabecera());

        Panel derecha = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro, Padding = new Padding(24, 0, 0, 0) };
        Panel izquierda = new Panel { Dock = DockStyle.Left, BackColor = Estilo.GrisClaro };
        cuerpo.Controls.Add(derecha);
        cuerpo.Controls.Add(izquierda);

        // El plano conserva su proporción: su ancho depende del alto disponible.
        cuerpo.Resize += (sender, e) =>
            izquierda.Width = Math.Max(300, Math.Min(cuerpo.ClientSize.Width * 55 / 100, (int)((cuerpo.ClientSize.Height - 60) * 0.82)));

        CrearZonaPlano(izquierda);
        CrearZonaDerecha(derecha);

        _plano.SeleccionCambio += (sender, e) => MostrarSeleccion();
        _temporizador.Tick += (sender, e) =>
        {
            if (Visible)
            {
                Refrescar();
            }
        };
        _temporizador.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _temporizador.Dispose();
        }
        base.Dispose(disposing);
    }

    // ---------- Construcción ----------

    private Panel CrearCabecera()
    {
        Panel cabecera = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = Estilo.GrisClaro };

        Panel izquierda = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro };
        izquierda.Controls.Add(_lblOcupacion);
        izquierda.Controls.Add(_lblLibres);

        FlowLayoutPanel acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Estilo.GrisClaro
        };
        Button btnEntrada = Estilo.BotonAmarillo("Registrar entrada  →", 0, 0, 230, 46);
        Button btnSalida = Estilo.BotonBorde("Registrar salida  →", 0, 0, 230, 46, false);
        btnEntrada.Margin = new Padding(0, 6, 12, 0);
        btnSalida.Margin = new Padding(0, 6, 0, 0);
        btnEntrada.Click += (sender, e) => Shell.IrAEntrada(null);
        btnSalida.Click += (sender, e) => Shell.IrASalida(null);
        acciones.Controls.Add(btnEntrada);
        acciones.Controls.Add(btnSalida);

        // La barra de ocupación se estira con la ventana: el relleno amarillo mide el porcentaje del ancho.
        _barraFondo.Controls.Add(_barraRelleno);
        _barraFondo.Resize += (sender, e) => AjustarBarra();

        cabecera.Controls.Add(izquierda);
        cabecera.Controls.Add(acciones);
        cabecera.Controls.Add(_barraFondo);
        return cabecera;
    }

    private void CrearZonaPlano(Panel izquierda)
    {
        _plano.Dock = DockStyle.Fill;
        izquierda.Controls.Add(_plano);

        // Leyenda y botón Actualizar, encima del plano.
        FlowLayoutPanel barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Estilo.GrisClaro
        };
        barra.Controls.Add(Estilo.Leyenda("Libre", Estilo.Blanco));
        barra.Controls.Add(Estilo.Leyenda("Ocupado", Estilo.Carbon));
        barra.Controls.Add(Estilo.Leyenda("Seleccionado", Estilo.Amarillo));

        Button btnActualizar = Estilo.BotonBorde("Actualizar", 0, 0, 120, 34, false);
        btnActualizar.Margin = new Padding(12, 4, 0, 0);
        btnActualizar.Click += (sender, e) => Refrescar();
        barra.Controls.Add(btnActualizar);

        izquierda.Controls.Add(barra);
    }

    private void CrearZonaDerecha(Panel derecha)
    {
        // Vehículos dentro (ocupa el resto)
        Panel tarjetaTabla = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Blanco, Padding = new Padding(20) };
        derecha.Controls.Add(tarjetaTabla);

        // Con Dock.Top, lo último que se agrega queda más arriba: primero el espacio entre las tarjetas
        // y después la tarjeta del cupo seleccionado.
        derecha.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Estilo.GrisClaro });

        Panel tarjetaCupo = new Panel { Dock = DockStyle.Top, Height = 246, BackColor = Estilo.Blanco };
        derecha.Controls.Add(tarjetaCupo);

        tarjetaCupo.Controls.Add(Estilo.Etiqueta("Cupo seleccionado", 20, 16));
        tarjetaCupo.Controls.Add(_lblCodigo);
        tarjetaCupo.Controls.Add(_lblDetalle);
        tarjetaCupo.Controls.Add(_btnEntradaAqui);
        tarjetaCupo.Controls.Add(_btnSalidaAqui);
        _lblDetalle.Text = "Toca un cupo en el plano para ver su código, su zona y qué tan lejos está de la portería.";
        _btnEntradaAqui.Visible = false;
        _btnSalidaAqui.Visible = false;
        _btnEntradaAqui.Click += (sender, e) =>
        {
            Cupo? cupo = _plano.CupoSeleccionado;
            if (cupo != null)
            {
                Shell.IrAEntrada(cupo.Id);
            }
        };
        _btnSalidaAqui.Click += (sender, e) =>
        {
            Cupo? cupo = _plano.CupoSeleccionado;
            Ticket? ticket = cupo == null ? null : _abiertos.FirstOrDefault(t => t.CupoId == cupo.Id);
            if (ticket != null)
            {
                Shell.IrASalida(ticket.Placa);
            }
        };

        // Dentro de la tarjeta de la tabla: título, filtros y la tabla.
        Estilo.Columna(_tabla, "placa", "Placa", 22);
        Estilo.Columna(_tabla, "tipo", "Tipo", 14);
        Estilo.Columna(_tabla, "cupo", "Cupo", 14);
        Estilo.Columna(_tabla, "entrada", "Entrada", 16);
        Estilo.Columna(_tabla, "tiempo", "Tiempo", 18);
        Estilo.Columna(_tabla, "accion", "", 26, true);
        _tabla.Columns["accion"]!.DefaultCellStyle.ForeColor = Estilo.AmarilloOscuro;
        _tabla.CellClick += Tabla_CellClick;
        _tabla.CellMouseEnter += (sender, e) => _tabla.Cursor = e.RowIndex >= 0 && _tabla.Columns[e.ColumnIndex].Name == "accion" ? Cursors.Hand : Cursors.Default;

        FlowLayoutPanel filtros = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Estilo.Blanco
        };
        foreach (RadioButton opcion in new[] { _rbTodos, _rbCarro, _rbMoto })
        {
            opcion.Margin = new Padding(0, 6, 8, 0);
            opcion.CheckedChanged += (sender, e) => LlenarTabla();
            filtros.Controls.Add(opcion);
        }
        _rbTodos.Checked = true;
        _txtBuscar.PlaceholderText = "BUSCAR PLACA";
        _txtBuscar.Margin = new Padding(8, 7, 0, 0);
        _txtBuscar.TextChanged += (sender, e) => LlenarTabla();
        filtros.Controls.Add(_txtBuscar);

        Label titulo = Estilo.Titulo("Vehículos dentro", 0, 0, 26, Estilo.Carbon);
        titulo.Dock = DockStyle.Top;
        titulo.AutoSize = false;
        titulo.Height = 40;

        _lblVacio.Dock = DockStyle.Top;
        _lblVacio.Height = 36;

        // Fill primero y luego lo que va arriba.
        tarjetaTabla.Controls.Add(_tabla);
        tarjetaTabla.Controls.Add(_lblVacio);
        tarjetaTabla.Controls.Add(filtros);
        tarjetaTabla.Controls.Add(titulo);
    }

    // ---------- Datos ----------

    public override void AlMostrar()
    {
        Refrescar();
    }

    private void Refrescar()
    {
        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                List<Cupo> cupos = servicio.ObtenerCupos();
                EstadoParqueadero estado = servicio.CargarEstado();
                _abiertos = servicio.ObtenerTicketsAbiertos();

                _lblOcupacion.Text = $"{estado.CuposOcupados} de {estado.TotalCupos} cupos ocupados".ToUpper();
                _lblLibres.Text = $"{estado.TotalCupos - estado.CuposOcupados} cupos libres".ToUpper();
                _porcentajeOcupado = estado.TotalCupos == 0 ? 0 : estado.CuposOcupados * 100 / estado.TotalCupos;
                AjustarBarra();

                _plano.Actualizar(cupos, estado);
            }
            LlenarTabla();
        }
        catch (Exception ex)
        {
            _lblOcupacion.Text = "NO SE PUDO LEER LA BASE DE DATOS";
            _lblLibres.Text = ex.Message;
        }
    }

    private void AjustarBarra()
    {
        _barraRelleno.Width = _barraFondo.Width * _porcentajeOcupado / 100;
    }

    // Aplica el filtro de tipo y el buscador a la lista de vehículos que están adentro.
    private void LlenarTabla()
    {
        IEnumerable<Ticket> lista = _abiertos;

        if (_rbCarro.Checked)
        {
            lista = lista.Where(t => t.Tipo == TipoVehiculo.Carro);
        }
        else if (_rbMoto.Checked)
        {
            lista = lista.Where(t => t.Tipo == TipoVehiculo.Moto);
        }

        string buscado = ServicioParqueadero.NormalizarPlaca(_txtBuscar.Text);
        if (buscado.Length > 0)
        {
            lista = lista.Where(t => t.Placa.Contains(buscado));
        }

        _tabla.Rows.Clear();
        DateTime ahora = DateTime.Now;
        foreach (Ticket t in lista)
        {
            int fila = _tabla.Rows.Add(t.Placa, t.Tipo, t.Cupo?.Codigo ?? "", t.HoraEntrada.ToString("HH:mm"), Formato.Duracion(ahora - t.HoraEntrada), "DAR SALIDA →");
            _tabla.Rows[fila].Tag = t.Placa;
        }
        _tabla.CurrentCell = null; // Sin ninguna fila resaltada hasta que se toque una.
        _tabla.ClearSelection();

        _lblVacio.Text = _tabla.Rows.Count > 0 ? "" : (_abiertos.Count == 0 ? "No hay vehículos adentro." : "Ningún vehículo coincide con el filtro.");
        _lblVacio.Visible = _lblVacio.Text.Length > 0;
    }

    private void Tabla_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && _tabla.Columns[e.ColumnIndex].Name == "accion" && _tabla.Rows[e.RowIndex].Tag is string placa)
        {
            Shell.IrASalida(placa);
        }
    }

    // ---------- Cupo seleccionado ----------

    private void MostrarSeleccion()
    {
        Cupo? cupo = _plano.CupoSeleccionado;
        PosicionCupo? posicion = _plano.PosicionSeleccionada;
        if (cupo == null || posicion == null)
        {
            return;
        }

        bool ocupado = _plano.EstaOcupado(cupo);
        Ticket? ticket = ocupado ? _abiertos.FirstOrDefault(t => t.CupoId == cupo.Id) : null;

        _lblCodigo.Text = cupo.Codigo;
        _lblDetalle.ForeColor = Estilo.Carbon;
        _lblDetalle.Text =
            $"{cupo.Tipo} · {(ocupado ? "Ocupado" : "Libre")}\n" +
            $"{posicion.Zona}\n" +
            $"A unos {PlanoParqueadero.MetrosAPorteria(posicion)} m de la portería";

        if (ticket != null)
        {
            _lblDetalle.Text += $"\nVehículo {ticket.Placa} desde las {ticket.HoraEntrada:HH:mm}";
        }

        // Libre: se puede registrar una entrada ahí. Ocupado: se puede dar salida a ese vehículo.
        _btnEntradaAqui.Visible = !ocupado;
        _btnSalidaAqui.Visible = ticket != null;
    }
}
