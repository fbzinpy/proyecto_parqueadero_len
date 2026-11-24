namespace Parqueadero.Escritorio;

// Ventana para cambiar el valor por hora de carros y motos.
public class FormTarifas : Form
{
    private readonly Func<decimal, decimal, string?> _guardar;

    private readonly TextBox _txtCarro = Estilo.Campo(28, 92, 364, false);
    private readonly TextBox _txtMoto = Estilo.Campo(28, 164, 364, false);
    private readonly Label _lblError = Estilo.Parrafo(28, 214, 364, 44, 15, Estilo.Error);

    // guardar recibe (valorCarro, valorMoto) y devuelve null si salió bien, o el mensaje de error.
    public FormTarifas(decimal valorCarro, decimal valorMoto, Func<decimal, decimal, string?> guardar)
    {
        _guardar = guardar;

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Editar tarifas";
        ClientSize = new Size(420, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Estilo.Tarjeta;
        Font = Estilo.Fuente(14);

        _txtCarro.Text = valorCarro.ToString("0");
        _txtMoto.Text = valorMoto.ToString("0");

        Button btnCancelar = Estilo.BotonBorde("Cancelar", 28, 268, 130, 48, false);
        btnCancelar.Click += (sender, e) => DialogResult = DialogResult.Cancel;

        Button btnGuardar = Estilo.BotonAmarillo("Guardar  →", 170, 268, 222, 48);
        btnGuardar.Click += (sender, e) => Guardar();

        Controls.Add(Estilo.Etiqueta("Administración", 28, 20));
        Controls.Add(Estilo.Titulo("Editar tarifas", 28, 38, 30, Estilo.Texto));
        Controls.Add(Estilo.Etiqueta("Carro · valor por hora ($)", 28, 70));
        Controls.Add(_txtCarro);
        Controls.Add(Estilo.Etiqueta("Moto · valor por hora ($)", 28, 142));
        Controls.Add(_txtMoto);
        Controls.Add(_lblError);
        Controls.Add(btnCancelar);
        Controls.Add(btnGuardar);

        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
    }

    private void Guardar()
    {
        // Valores en pesos enteros.
        if (!decimal.TryParse(_txtCarro.Text.Trim(), out decimal carro) || !decimal.TryParse(_txtMoto.Text.Trim(), out decimal moto))
        {
            _lblError.Text = "Escribe los valores como números, por ejemplo 3000.";
            return;
        }

        try
        {
            string? error = _guardar(carro, moto);
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
    }
}
