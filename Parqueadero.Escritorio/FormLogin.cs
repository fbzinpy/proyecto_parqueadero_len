using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Datos;

namespace Parqueadero.Escritorio;

// Ventana de inicio de sesión. Valida el correo y la contraseña con UserManager (de Identity),
// porque SignInManager depende de las cookies del navegador.
// Mismo diseño que el acceso de la web: escenario negro a la izquierda y formulario blanco a la derecha.
public class FormLogin : Form
{
    private readonly TextBox _txtCorreo = Estilo.Campo(55, 210, 300, false);
    private readonly TextBox _txtClave = Estilo.Campo(55, 282, 300, false);
    private readonly Label _lblError = Estilo.Parrafo(55, 322, 300, 40, 15, Estilo.Error);
    private readonly Button _btnIngresar = Estilo.BotonAmarillo("Ingresar  →", 55, 372, 300, 48);

    public FormLogin()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Parqueadero USC - Iniciar sesión";
        ClientSize = new Size(880, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Estilo.Fondo;
        Font = Estilo.Fuente(14);

        // Se agrega primero el panel que ocupa el resto (Fill) y después el que se pega a la izquierda.
        Panel formulario = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.Fondo };
        Panel escenario = new Panel { Dock = DockStyle.Left, Width = 470, BackColor = Estilo.Tarjeta };
        Controls.Add(formulario);
        Controls.Add(escenario);

        CrearEscenario(escenario);
        CrearFormulario(formulario);

        // Al presionar Enter en cualquier campo se hace clic en Ingresar.
        AcceptButton = _btnIngresar;
    }

    private void CrearEscenario(Panel escenario)
    {
        Label antetitulo = Estilo.Titulo("Sistema de gestión · Miniproyecto 2", 48, 70, 14, Estilo.Amarillo);
        Label hero = Estilo.Titulo("Cada cupo" + Environment.NewLine + "bajo control", 48, 105, 58, Estilo.Texto);

        Label detalle = Estilo.Parrafo(48, 285, 372, 90, 17, Estilo.Texto2);
        detalle.Text = "Entradas, salidas, cobros y reportes en un solo lugar. Web y escritorio sobre la misma base de datos.";

        // Los dos cuadritos amarillos del acceso de la web.
        Panel punto1 = new Panel { Location = new Point(48, 440), Size = new Size(12, 12), BackColor = Estilo.Amarillo };
        Panel punto2 = new Panel { Location = new Point(68, 440), Size = new Size(12, 12), BackColor = Estilo.BordeCampo };

        escenario.Controls.AddRange(new Control[] { antetitulo, hero, detalle, punto1, punto2 });
    }

    private void CrearFormulario(Panel formulario)
    {
        _txtClave.UseSystemPasswordChar = true; // Muestra puntos en lugar de la contraseña.
        _btnIngresar.Click += BtnIngresar_Click;

        formulario.Controls.AddRange(new Control[]
        {
            Estilo.Etiqueta("Acceso", 55, 95),
            Estilo.Titulo("Iniciar sesión", 55, 115, 38, Estilo.Texto),
            Estilo.Etiqueta("Correo electrónico", 55, 188),
            _txtCorreo,
            Estilo.Etiqueta("Contraseña", 55, 260),
            _txtClave,
            _lblError,
            _btnIngresar
        });
    }

    // async void solo se usa en eventos (como este clic), porque el evento no puede devolver Task.
    private async void BtnIngresar_Click(object? sender, EventArgs e)
    {
        string correo = _txtCorreo.Text.Trim();
        string clave = _txtClave.Text;
        string? rol = null;

        if (correo.Length == 0 || clave.Length == 0)
        {
            _lblError.Text = "Escribe el correo y la contraseña.";
            return;
        }

        _lblError.Text = "";
        _btnIngresar.Enabled = false;
        _btnIngresar.Text = "INGRESANDO...";

        try
        {
            // Un scope nuevo por operación: el UserManager (y su DbContext) solo vive durante este clic.
            using (IServiceScope scope = Program.Servicios.CreateScope())
            {
                UserManager<IdentityUser> usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                IdentityUser? usuario = await usuarios.FindByEmailAsync(correo);
                if (usuario != null && await usuarios.CheckPasswordAsync(usuario, clave))
                {
                    IList<string> roles = await usuarios.GetRolesAsync(usuario);

                    // Solo pueden entrar Administrador y Operador.
                    if (roles.Contains(DatosIniciales.RolAdministrador))
                    {
                        rol = DatosIniciales.RolAdministrador;
                    }
                    else if (roles.Contains(DatosIniciales.RolOperador))
                    {
                        rol = DatosIniciales.RolOperador;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Por ejemplo, la base de datos está bloqueada o no se puede abrir.
            _lblError.Text = "No se pudo conectar con la base de datos: " + ex.Message;
            RestaurarBoton();
            return;
        }

        RestaurarBoton();

        if (rol == null)
        {
            _lblError.Text = "Correo o contraseña incorrectos.";
            _txtClave.Clear();
            _txtClave.Focus();
            return;
        }

        // Se oculta el login y se abre la pantalla principal.
        Hide();
        DialogResult resultado;
        using (FormPrincipal principal = new FormPrincipal(correo, rol))
        {
            resultado = principal.ShowDialog();
        }

        if (resultado == DialogResult.Retry)
        {
            // "Cerrar sesión": se vuelve al login con los campos limpios.
            _txtCorreo.Clear();
            _txtClave.Clear();
            _lblError.Text = "";
            Show();
            _txtCorreo.Focus();
        }
        else
        {
            // Se cerró la ventana principal: termina la aplicación.
            Close();
        }
    }

    private void RestaurarBoton()
    {
        _btnIngresar.Enabled = true;
        _btnIngresar.Text = "INGRESAR  →";
    }
}
