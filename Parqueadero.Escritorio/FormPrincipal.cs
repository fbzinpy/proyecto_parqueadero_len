using Parqueadero.Core.Datos;

namespace Parqueadero.Escritorio;

// Ventana principal: barra negra con el menú (como el de la web) y, debajo, la pantalla elegida.
// Las opciones del menú dependen del rol: el Operador ve Panel, Entrada, Salida e Historial;
// el Administrador ve además Administración.
public class FormPrincipal : Form, IMessageFilter
{
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_NCLBUTTONDOWN = 0x00A1;

    public const string NombrePanel = "Panel";
    public const string NombreEntrada = "Registrar entrada";
    public const string NombreSalida = "Registrar salida";
    public const string NombreHistorial = "Historial";
    public const string NombreAdmin = "Administración";

    public string Correo { get; }
    public string Rol { get; }
    public bool EsAdministrador => Rol == DatosIniciales.RolAdministrador;

    private readonly Panel _contenido = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.GrisClaro };
    private readonly Panel _menu = new Panel { Width = 320, BackColor = ColorTranslator.FromHtml("#181818"), Visible = false };
    private readonly Button _btnMenu;
    private readonly Panel _barra;

    // Las pantallas y los botones del menú, por nombre (en el orden en que aparecen).
    private readonly Dictionary<string, Vista> _vistas = new Dictionary<string, Vista>();
    private readonly Dictionary<string, Button> _botonesMenu = new Dictionary<string, Button>();
    private string _actual = "";

    public FormPrincipal(string correo, string rol)
    {
        Correo = correo;
        Rol = rol;

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Parqueadero";
        BackColor = Estilo.GrisClaro;
        Font = Estilo.Fuente(14);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized; // Abre ocupando toda la pantalla.

        // Tamaño al restaurar la ventana: el que quepa en la pantalla.
        Rectangle pantalla = Screen.PrimaryScreen!.WorkingArea;
        Size = new Size(Math.Min(1240, pantalla.Width), Math.Min(800, pantalla.Height));
        MinimumSize = new Size(Math.Min(1100, pantalla.Width), Math.Min(640, pantalla.Height));

        _btnMenu = CrearBotonMenu();
        _barra = CrearBarra();

        // El orden importa al acoplar (Dock): primero lo que ocupa el resto (Fill) y después los bordes.
        Controls.Add(_contenido);
        Controls.Add(_barra);

        CrearPantallas();
        CrearMenu();
        Controls.Add(_menu); // El menú flota encima del contenido (no está acoplado).

        Ir(NombrePanel);
    }

    public void IrAEntrada(int? cupoId)
    {
        ((VistaEntrada)_vistas[NombreEntrada]).Preparar(cupoId);
        Ir(NombreEntrada, false);
    }

    public void IrASalida(string? placa)
    {
        ((VistaSalida)_vistas[NombreSalida]).Preparar(placa);
        Ir(NombreSalida, false);
    }

    // ---------- Construcción ----------

    private Panel CrearBarra()
    {
        Panel barra = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Estilo.Carbon, Padding = new Padding(8, 0, 24, 0) };

        Label marca = Estilo.Titulo("Parqueadero", 0, 0, 28, Estilo.Blanco);
        marca.Dock = DockStyle.Left;
        marca.AutoSize = false;
        marca.Width = 260;
        marca.TextAlign = ContentAlignment.MiddleLeft;

        // A la derecha: rol y correo de quien inició sesión.
        Label lblRol = Estilo.Titulo(Rol, 0, 0, 13, Estilo.Amarillo);
        lblRol.Margin = new Padding(0, 12, 0, 0);
        Label lblCorreo = Estilo.Titulo("", 0, 0, 16, Estilo.Blanco);
        lblCorreo.Text = Correo; // El correo se muestra tal cual, sin pasarlo a mayúsculas.
        lblCorreo.Margin = new Padding(0);
        FlowLayoutPanel datos = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Estilo.Carbon
        };
        datos.Controls.Add(lblRol);
        datos.Controls.Add(lblCorreo);

        barra.Controls.Add(datos);
        barra.Controls.Add(marca);
        barra.Controls.Add(_btnMenu); // Último: queda pegado a la izquierda.
        return barra;
    }

    // Botón "☰ MENÚ" (las tres rayas se dibujan, para no depender de la letra).
    private Button CrearBotonMenu()
    {
        Bitmap icono = new Bitmap(24, 24);
        using (Graphics g = Graphics.FromImage(icono))
        using (Pen lapiz = new Pen(Color.White, 2f))
        {
            g.DrawLine(lapiz, 2, 6, 22, 6);
            g.DrawLine(lapiz, 2, 12, 22, 12);
            g.DrawLine(lapiz, 2, 18, 22, 18);
        }

        Button boton = new Button
        {
            Text = "MENÚ",
            Image = icono,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Dock = DockStyle.Left,
            Width = 130,
            Font = Estilo.Fuente(15),
            FlatStyle = FlatStyle.Flat,
            BackColor = Estilo.Carbon,
            ForeColor = Estilo.Blanco,
            Cursor = Cursors.Hand,
            Padding = new Padding(12, 0, 0, 0),
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseOverBackColor = Estilo.Grafito;
        boton.Click += (sender, e) => AlternarMenu();
        return boton;
    }

    // Crea cada pantalla una sola vez; la de Administración solo existe para el Administrador.
    private void CrearPantallas()
    {
        _vistas[NombrePanel] = new VistaPanel(this);
        _vistas[NombreEntrada] = new VistaEntrada(this);
        _vistas[NombreSalida] = new VistaSalida(this);
        _vistas[NombreHistorial] = new VistaHistorial(this);
        if (EsAdministrador)
        {
            _vistas[NombreAdmin] = new VistaAdmin(this);
        }

        foreach (Vista vista in _vistas.Values)
        {
            vista.Visible = false;
            _contenido.Controls.Add(vista);
        }
    }

    // Lista del menú: una opción por pantalla y, al final, "Cerrar sesión".
    private void CrearMenu()
    {
        foreach (string nombre in _vistas.Keys)
        {
            string destino = nombre;
            Button boton = CrearOpcionMenu(nombre);
            boton.Click += (sender, e) => Ir(destino);
            _botonesMenu[nombre] = boton;
        }

        Button cerrar = CrearOpcionMenu("Cerrar sesión");
        cerrar.Click += (sender, e) => CerrarSesion();

        // Con Dock.Top lo último que se agrega queda arriba: se agregan de abajo hacia arriba.
        _menu.Controls.Add(cerrar);
        foreach (string nombre in _vistas.Keys.Reverse())
        {
            _menu.Controls.Add(_botonesMenu[nombre]);
        }

        _menu.Height = (_vistas.Count + 1) * 56 + 16;
        _menu.Padding = new Padding(0, 8, 0, 8);
    }

    private static Button CrearOpcionMenu(string texto)
    {
        Button boton = new Button
        {
            Text = texto.ToUpper(),
            Dock = DockStyle.Top,
            Height = 56,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(24, 0, 0, 0),
            Font = Estilo.Fuente(20),
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorTranslator.FromHtml("#181818"),
            ForeColor = Estilo.Blanco,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseOverBackColor = Estilo.Grafito;
        return boton;
    }

    // ---------- Navegación ----------

    // Muestra una pantalla. alMostrar = false cuando la pantalla ya se preparó con datos (cupo o placa elegidos).
    private void Ir(string nombre, bool alMostrar = true)
    {
        // Cada pantalla se pide por su nombre; si el rol no la tiene (Administración), no existe en el diccionario.
        if (!_vistas.TryGetValue(nombre, out Vista? vista))
        {
            return;
        }

        foreach (Vista otra in _vistas.Values)
        {
            otra.Visible = otra == vista;
        }
        _actual = nombre;

        // La opción activa se pinta en amarillo, como en la web.
        foreach (KeyValuePair<string, Button> opcion in _botonesMenu)
        {
            opcion.Value.ForeColor = opcion.Key == nombre ? Estilo.Amarillo : Estilo.Blanco;
        }

        _menu.Visible = false;
        Text = "Parqueadero - " + nombre;

        if (alMostrar)
        {
            vista.AlMostrar();
        }
    }

    private void AlternarMenu()
    {
        _menu.Location = new Point(0, _barra.Bottom);
        _menu.Visible = !_menu.Visible;
        if (_menu.Visible)
        {
            _menu.BringToFront();
        }
    }

    private void CerrarSesion()
    {
        // Retry es la señal para el login: "volver a mostrarse" (cerrar la ventana con la X termina la aplicación).
        DialogResult = DialogResult.Retry;
    }

    // ---------- Cerrar el menú al hacer clic fuera ----------

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Application.AddMessageFilter(this);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.RemoveMessageFilter(this);
        base.OnFormClosed(e);
    }

    // Se llama con cada mensaje de la aplicación: si hay un clic fuera del menú, se cierra.
    public bool PreFilterMessage(ref Message m)
    {
        if (_menu.Visible && (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_NCLBUTTONDOWN))
        {
            Control? control = Control.FromHandle(m.HWnd);
            bool dentro = control != null && (control == _menu || _menu.Contains(control) || control == _btnMenu);
            if (!dentro)
            {
                _menu.Visible = false;
            }
        }
        return false; // El clic sigue su camino normal.
    }
}
