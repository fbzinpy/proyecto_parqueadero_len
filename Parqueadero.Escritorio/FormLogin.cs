using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Datos;

namespace Parqueadero.Escritorio;

// Ventana de inicio de sesión. Valida el correo y la contraseña con UserManager (de Identity),
// porque SignInManager depende de las cookies del navegador.
public class FormLogin : Form
{
    private readonly TextBox _txtCorreo = new TextBox();
    private readonly TextBox _txtClave = new TextBox();
    private readonly Button _btnIngresar = new Button();

    public FormLogin()
    {
        Text = "Parqueadero - Iniciar sesión";
        ClientSize = new Size(320, 175);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        Label lblCorreo = new Label { Text = "Correo", Location = new Point(20, 20), AutoSize = true };
        _txtCorreo.Location = new Point(20, 40);
        _txtCorreo.Width = 280;

        Label lblClave = new Label { Text = "Contraseña", Location = new Point(20, 70), AutoSize = true };
        _txtClave.Location = new Point(20, 90);
        _txtClave.Width = 280;
        _txtClave.UseSystemPasswordChar = true; // Muestra puntos en lugar de la contraseña.

        _btnIngresar.Text = "Ingresar";
        _btnIngresar.Location = new Point(20, 125);
        _btnIngresar.Width = 280;
        _btnIngresar.Height = 30;
        _btnIngresar.Click += BtnIngresar_Click;

        // Al presionar Enter en cualquier campo se hace clic en Ingresar.
        AcceptButton = _btnIngresar;

        Controls.Add(lblCorreo);
        Controls.Add(_txtCorreo);
        Controls.Add(lblClave);
        Controls.Add(_txtClave);
        Controls.Add(_btnIngresar);
    }

    // async void solo se usa en eventos (como este clic), porque el evento no puede devolver Task.
    private async void BtnIngresar_Click(object? sender, EventArgs e)
    {
        string correo = _txtCorreo.Text.Trim();
        string clave = _txtClave.Text;
        string? rol = null;

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

        if (rol == null)
        {
            MessageBox.Show("Correo o contraseña incorrectos.", "Iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Se oculta el login, se abre la pantalla principal y, cuando esta se cierra, termina la aplicación.
        Hide();
        using (FormPrincipal principal = new FormPrincipal(correo, rol))
        {
            principal.ShowDialog();
        }
        Close();
    }
}
