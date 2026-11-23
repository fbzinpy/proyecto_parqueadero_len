using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Administración: resumen del día, tarifas y usuarios con sus roles. Solo la ve el Administrador.
public class VistaAdmin : Vista
{
    private readonly FlowLayoutPanel _columna = new FlowLayoutPanel
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        BackColor = Estilo.GrisClaro
    };

    // Resumen del día
    private readonly Label _lblIngresos = Estilo.Titulo("", 20, 40, 40, Estilo.Carbon);
    private readonly Label _lblAtendidos = Estilo.Titulo("", 20, 40, 40, Estilo.Carbon);
    private readonly Label _lblOcupacion = Estilo.Titulo("", 20, 40, 40, Estilo.Carbon);
    private readonly Label _lblOperadores = Estilo.Titulo("", 20, 40, 40, Estilo.Carbon);

    // Tarifas
    private readonly Label _lblHoraCarro = Estilo.Texto(170, 86, 200, 26, 17, Estilo.Carbon);
    private readonly Label _lblCuposCarro = Estilo.Texto(170, 118, 200, 26, 17, Estilo.Carbon);
    private readonly Label _lblHoraMoto = Estilo.Texto(170, 86, 200, 26, 17, Estilo.Carbon);
    private readonly Label _lblCuposMoto = Estilo.Texto(170, 118, 200, 26, 17, Estilo.Carbon);
    private decimal _valorCarro;
    private decimal _valorMoto;

    // Usuarios
    private readonly DataGridView _tabla = Estilo.Tabla();

    public VistaAdmin(FormPrincipal shell) : base(shell)
    {
        Controls.Add(_columna);
        Controls.Add(Estilo.Cabecera("Resumen del día", "Administración", out _));

        _columna.Controls.Add(CrearResumen());
        _columna.Controls.Add(CrearSeccionTarifas());
        _columna.Controls.Add(CrearSeccionUsuarios());

        // Cada bloque ocupa todo el ancho disponible.
        _columna.Resize += (sender, e) =>
        {
            foreach (Control bloque in _columna.Controls)
            {
                bloque.Width = Math.Max(300, _columna.ClientSize.Width - bloque.Margin.Horizontal);
            }
        };
    }

    // ---------- Construcción ----------

    private Control CrearResumen()
    {
        TableLayoutPanel resumen = new TableLayoutPanel
        {
            Height = 112,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Estilo.GrisClaro,
            Margin = new Padding(0, 0, 0, 16)
        };
        for (int i = 0; i < 4; i++)
        {
            resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        resumen.Controls.Add(CrearDato("Ingresos del día", _lblIngresos), 0, 0);
        resumen.Controls.Add(CrearDato("Vehículos atendidos hoy", _lblAtendidos), 1, 0);
        resumen.Controls.Add(CrearDato("Ocupación actual", _lblOcupacion), 2, 0);
        resumen.Controls.Add(CrearDato("Operadores registrados", _lblOperadores), 3, 0);
        return resumen;
    }

    private static Panel CrearDato(string titulo, Label valor)
    {
        Panel dato = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Blanco, Margin = new Padding(0, 0, 12, 0) };
        dato.Controls.Add(Estilo.Etiqueta(titulo, 20, 16));
        dato.Controls.Add(valor);
        return dato;
    }

    private Control CrearSeccionTarifas()
    {
        Panel seccion = new Panel { Height = 238, BackColor = Estilo.GrisClaro, Margin = new Padding(0, 0, 0, 16) };

        Button btnEditar = Estilo.BotonBorde("Editar tarifas  →", 0, 0, 200, 40, false);
        btnEditar.Click += (sender, e) => EditarTarifas();

        TableLayoutPanel tarjetas = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 168,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Estilo.GrisClaro
        };
        tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tarjetas.Controls.Add(CrearTarjetaTarifa("Carro", _lblHoraCarro, _lblCuposCarro), 0, 0);
        tarjetas.Controls.Add(CrearTarjetaTarifa("Moto", _lblHoraMoto, _lblCuposMoto), 1, 0);

        seccion.Controls.Add(tarjetas);
        seccion.Controls.Add(Estilo.Titulo("Tarifas", 0, 4, 30, Estilo.Carbon));
        seccion.Controls.Add(btnEditar);
        seccion.Resize += (sender, e) => btnEditar.Left = seccion.ClientSize.Width - btnEditar.Width;
        return seccion;
    }

    private static Panel CrearTarjetaTarifa(string vehiculo, Label porHora, Label cupos)
    {
        Panel tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Blanco, Margin = new Padding(0, 0, 12, 0) };
        tarjeta.Controls.Add(Estilo.Etiqueta("Vehículo", 20, 16));
        tarjeta.Controls.Add(Estilo.Titulo(vehiculo, 20, 34, 32, Estilo.Carbon));
        tarjeta.Controls.Add(Estilo.Etiqueta("Por hora", 20, 90));
        tarjeta.Controls.Add(porHora);
        tarjeta.Controls.Add(Estilo.Etiqueta("Cupos", 20, 122));
        tarjeta.Controls.Add(cupos);
        return tarjeta;
    }

    private Control CrearSeccionUsuarios()
    {
        Panel seccion = new Panel { Height = 380, BackColor = Estilo.GrisClaro, Padding = new Padding(0, 56, 0, 0) };

        Button btnNuevo = Estilo.BotonAmarillo("Nuevo usuario  +", 0, 0, 220, 40);
        btnNuevo.Click += (sender, e) => NuevoUsuario();

        Estilo.Columna(_tabla, "correo", "Correo", 50);
        Estilo.Columna(_tabla, "rol", "Rol", 25);
        Estilo.Columna(_tabla, "accion", "", 25, true);
        _tabla.Columns["accion"]!.DefaultCellStyle.ForeColor = Estilo.AmarilloOscuro;
        _tabla.CellClick += (sender, e) =>
        {
            if (e.RowIndex >= 0 && _tabla.Columns[e.ColumnIndex].Name == "accion" && _tabla.Rows[e.RowIndex].Tag is UsuarioFila usuario)
            {
                EditarUsuario(usuario);
            }
        };
        _tabla.CellMouseEnter += (sender, e) => _tabla.Cursor = e.RowIndex >= 0 && _tabla.Columns[e.ColumnIndex].Name == "accion" ? Cursors.Hand : Cursors.Default;

        // La tabla va en una tarjeta blanca debajo del título.
        Panel tarjetaTabla = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Blanco, Padding = new Padding(12) };
        tarjetaTabla.Controls.Add(_tabla);

        seccion.Controls.Add(tarjetaTabla);
        seccion.Controls.Add(Estilo.Titulo("Usuarios y roles", 0, 4, 30, Estilo.Carbon));
        seccion.Controls.Add(btnNuevo);
        seccion.Resize += (sender, e) => btnNuevo.Left = seccion.ClientSize.Width - btnNuevo.Width;
        return seccion;
    }

    // ---------- Datos ----------

    public override async void AlMostrar()
    {
        await RefrescarAsync();
    }

    private async Task RefrescarAsync()
    {
        // Solo el Administrador puede ver esta pantalla (el menú ya no se la ofrece al operador).
        if (!Shell.EsAdministrador)
        {
            return;
        }

        try
        {
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
                EstadoParqueadero estado = servicio.CargarEstado();
                List<Cupo> cupos = servicio.ObtenerCupos();
                (decimal ingresos, int atendidos) = servicio.ResumenDelDia();
                List<Tarifa> tarifas = servicio.ObtenerTarifas();

                _valorCarro = tarifas.FirstOrDefault(t => t.Tipo == TipoVehiculo.Carro)?.ValorHora ?? 0;
                _valorMoto = tarifas.FirstOrDefault(t => t.Tipo == TipoVehiculo.Moto)?.ValorHora ?? 0;

                _lblIngresos.Text = Formato.Dinero(ingresos);
                _lblAtendidos.Text = atendidos.ToString();
                _lblOcupacion.Text = $"{estado.CuposOcupados} / {estado.TotalCupos}";
                _lblHoraCarro.Text = Formato.Dinero(_valorCarro);
                _lblCuposCarro.Text = cupos.Count(c => c.Tipo == TipoVehiculo.Carro).ToString();
                _lblHoraMoto.Text = Formato.Dinero(_valorMoto);
                _lblCuposMoto.Text = cupos.Count(c => c.Tipo == TipoVehiculo.Moto).ToString();
            }

            _lblOperadores.Text = (await GestorUsuarios.ContarOperadoresAsync()).ToString();
            await LlenarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo leer la información de administración:\n" + ex.Message, "Administración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LlenarUsuariosAsync()
    {
        List<UsuarioFila> usuarios = await GestorUsuarios.ListarAsync();

        _tabla.Rows.Clear();
        foreach (UsuarioFila usuario in usuarios)
        {
            int fila = _tabla.Rows.Add(usuario.Correo, usuario.Rol, "EDITAR →");
            _tabla.Rows[fila].Tag = usuario;
        }
        _tabla.CurrentCell = null;
        _tabla.ClearSelection();
    }

    // ---------- Acciones ----------

    private async void EditarTarifas()
    {
        if (!Shell.EsAdministrador)
        {
            return;
        }

        using (FormTarifas ventana = new FormTarifas(_valorCarro, _valorMoto, GuardarTarifas))
        {
            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await RefrescarAsync();
            }
        }
    }

    private string? GuardarTarifas(decimal valorCarro, decimal valorMoto)
    {
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            ServicioParqueadero servicio = scope.ServiceProvider.GetRequiredService<ServicioParqueadero>();
            return servicio.ActualizarTarifas(valorCarro, valorMoto);
        }
    }

    private async void NuevoUsuario()
    {
        if (!Shell.EsAdministrador)
        {
            return;
        }

        using (FormUsuario ventana = new FormUsuario(null, Shell.Correo))
        {
            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await RefrescarAsync();
            }
        }
    }

    private async void EditarUsuario(UsuarioFila usuario)
    {
        if (!Shell.EsAdministrador)
        {
            return;
        }

        using (FormUsuario ventana = new FormUsuario(usuario, Shell.Correo))
        {
            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await RefrescarAsync();
            }
        }
    }
}
