namespace Parqueadero.Escritorio;

// Ventana para crear un usuario nuevo o editar el rol y la contraseña de uno existente.
public class FormUsuario : Form
{
    private readonly UsuarioFila? _editando;
    private readonly string _correoDeQuienEdita;

    private readonly TextBox _txtCorreo = Estilo.Campo(28, 92, 404, false);
    private readonly TextBox _txtClave = Estilo.Campo(28, 164, 404, false);
    private readonly RadioButton _rbAdministrador = Estilo.Opcion("Administrador", 28, 236, 196, 40);
    private readonly RadioButton _rbOperador = Estilo.Opcion("Operador", 236, 236, 196, 40);
    private readonly Label _lblError = Estilo.Parrafo(28, 290, 404, 44, 15, Estilo.Error);
    private readonly Button _btnGuardar;

    // editando = null: usuario nuevo.
    public FormUsuario(UsuarioFila? editando, string correoDeQuienEdita)
    {
        _editando = editando;
        _correoDeQuienEdita = correoDeQuienEdita;

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = editando == null ? "Nuevo usuario" : "Editar usuario";
        ClientSize = new Size(460, 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Estilo.Tarjeta;
        Font = Estilo.Fuente(14);

        _txtClave.UseSystemPasswordChar = true;

        Label correoFijo = Estilo.Parrafo(28, 92, 404, 30, 18, Estilo.Texto);
        if (editando != null)
        {
            correoFijo.Text = editando.Correo;
            _txtClave.PlaceholderText = "Déjala vacía para no cambiarla";
            _rbAdministrador.Checked = editando.Rol.Contains("Administrador");
            _rbOperador.Checked = !_rbAdministrador.Checked;
        }
        else
        {
            _rbOperador.Checked = true;
        }

        Button btnCancelar = Estilo.BotonBorde("Cancelar", 28, 360, 150, 48, false);
        btnCancelar.Click += (sender, e) => DialogResult = DialogResult.Cancel;

        _btnGuardar = Estilo.BotonAmarillo(editando == null ? "Crear usuario  →" : "Guardar  →", 190, 360, 242, 48);
        _btnGuardar.Click += BtnGuardar_Click;

        Controls.Add(Estilo.Etiqueta(editando == null ? "Administración" : "Usuario", 28, 20));
        Controls.Add(Estilo.Titulo(editando == null ? "Nuevo usuario" : "Editar usuario", 28, 38, 30, Estilo.Texto));
        Controls.Add(Estilo.Etiqueta(editando == null ? "Correo electrónico" : "Correo", 28, 70));
        Controls.Add(editando == null ? _txtCorreo : correoFijo);
        Controls.Add(Estilo.Etiqueta(editando == null ? "Contraseña" : "Nueva contraseña (opcional)", 28, 142));
        Controls.Add(_txtClave);
        Controls.Add(Estilo.Etiqueta("Rol", 28, 214));
        Controls.Add(_rbAdministrador);
        Controls.Add(_rbOperador);
        Controls.Add(_lblError);
        Controls.Add(btnCancelar);
        Controls.Add(_btnGuardar);

        AcceptButton = _btnGuardar;
        CancelButton = btnCancelar;
    }

    private async void BtnGuardar_Click(object? sender, EventArgs e)
    {
        string rol = _rbAdministrador.Checked ? "Administrador" : "Operador";
        _lblError.Text = "";
        _btnGuardar.Enabled = false;

        try
        {
            string? error = _editando == null
                ? await GestorUsuarios.CrearAsync(_txtCorreo.Text, _txtClave.Text, rol)
                : await GestorUsuarios.EditarAsync(_editando.Id, rol, _txtClave.Text, _correoDeQuienEdita);

            if (error != null)
            {
                _lblError.Text = error;
                return;
            }

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            _lblError.Text = "No se pudo guardar: " + ex.Message;
        }
        finally
        {
            _btnGuardar.Enabled = true;
        }
    }
}
