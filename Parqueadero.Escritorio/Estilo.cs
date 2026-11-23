using System.Drawing.Text;

namespace Parqueadero.Escritorio;

// Colores, letra y controles con el mismo estilo de la web (DESIGN.md): negro carbón, gris claro,
// esquinas rectas, sin sombras, letra condensada en MAYÚSCULAS y amarillo solo en el botón principal.
public static class Estilo
{
    public static readonly Color Amarillo = ColorTranslator.FromHtml("#ffc000");
    public static readonly Color AmarilloOscuro = ColorTranslator.FromHtml("#917300");
    public static readonly Color Carbon = ColorTranslator.FromHtml("#202020");
    public static readonly Color Blanco = Color.White;
    public static readonly Color GrisClaro = ColorTranslator.FromHtml("#f5f5f5");
    public static readonly Color Grafito = ColorTranslator.FromHtml("#494949");
    public static readonly Color GrisMedio = ColorTranslator.FromHtml("#7d7d7d");
    public static readonly Color GrisBorde = ColorTranslator.FromHtml("#969696");
    public static readonly Color Error = ColorTranslator.FromHtml("#b3261e");
    public static readonly Color Exito = ColorTranslator.FromHtml("#1e7a3c");

    // La web usa Barlow Condensed. En Windows se usa la primera de estas letras que esté instalada.
    private static readonly string Familia = ElegirFamilia();

    private static string ElegirFamilia()
    {
        using (InstalledFontCollection instaladas = new InstalledFontCollection())
        {
            foreach (string nombre in new[] { "Barlow Condensed", "Bahnschrift SemiCondensed", "Arial Narrow", "Segoe UI" })
            {
                if (instaladas.Families.Any(f => f.Name == nombre))
                {
                    return nombre;
                }
            }
        }
        return "Segoe UI";
    }

    // El tamaño se da en "píxeles de diseño" y se convierte a puntos, para que crezca con la escala de Windows.
    public static Font Fuente(float tamano)
    {
        return new Font(Familia, tamano * 0.75f, FontStyle.Regular, GraphicsUnit.Point);
    }

    // ---------- Textos ----------

    // Etiqueta pequeña en mayúsculas (como ".etiqueta" de la web).
    public static Label Etiqueta(string texto, int x, int y)
    {
        return new Label
        {
            Text = texto.ToUpper(),
            Location = new Point(x, y),
            AutoSize = true,
            Font = Fuente(13),
            ForeColor = Grafito,
            BackColor = Color.Transparent
        };
    }

    public static Label Titulo(string texto, int x, int y, float tamano, Color color)
    {
        return new Label
        {
            Text = texto.ToUpper(),
            Location = new Point(x - (int)(tamano * 0.08), y),
            AutoSize = true,
            Font = Fuente(tamano),
            ForeColor = color,
            BackColor = Color.Transparent
        };
    }

    // Etiqueta de varias líneas con tamaño fijo (mensajes, datos del cupo).
    public static Label Texto(int x, int y, int ancho, int alto, float tamano, Color color)
    {
        return new Label
        {
            Location = new Point(x, y),
            Size = new Size(ancho, alto),
            Font = Fuente(tamano),
            ForeColor = color,
            BackColor = Color.Transparent
        };
    }

    public static void Mensaje(Label etiqueta, string texto, bool esError)
    {
        etiqueta.Text = texto;
        etiqueta.ForeColor = esError ? Error : Exito;
    }

    // ---------- Campos y botones ----------

    public static TextBox Campo(int x, int y, int ancho, bool mayusculas)
    {
        TextBox campo = new TextBox
        {
            Location = new Point(x, y),
            Width = ancho,
            Font = Fuente(18),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Blanco,
            ForeColor = Carbon
        };
        if (mayusculas)
        {
            campo.CharacterCasing = CharacterCasing.Upper;
        }
        return campo;
    }

    // Botón plano de un solo color, con otro color al pasar el mouse.
    private static Button BotonPlano(string texto, int x, int y, int ancho, int alto, Color fondo, Color letra, Color fondoHover, Color letraHover)
    {
        Button boton = new Button
        {
            Text = texto.ToUpper(),
            Location = new Point(x, y),
            Size = new Size(ancho, alto),
            Font = Fuente(16),
            FlatStyle = FlatStyle.Flat,
            BackColor = fondo,
            ForeColor = letra,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseOverBackColor = fondoHover;
        boton.FlatAppearance.MouseDownBackColor = fondoHover;
        boton.MouseEnter += (sender, e) => { if (boton.Enabled) boton.ForeColor = letraHover; };
        boton.MouseLeave += (sender, e) => boton.ForeColor = letra;
        boton.EnabledChanged += (sender, e) => boton.BackColor = boton.Enabled ? fondo : GrisBorde;
        return boton;
    }

    // Botón principal: amarillo.
    public static Button BotonAmarillo(string texto, int x, int y, int ancho, int alto)
    {
        return BotonPlano(texto, x, y, ancho, alto, Amarillo, Carbon, AmarilloOscuro, Blanco);
    }

    // Botón oscuro, para acciones importantes que no son la principal (cobrar).
    public static Button BotonOscuro(string texto, int x, int y, int ancho, int alto)
    {
        return BotonPlano(texto, x, y, ancho, alto, Carbon, Blanco, Grafito, Blanco);
    }

    // Botón solo con borde. sobreOscuro = va sobre la barra negra (borde y letra claros).
    public static Button BotonBorde(string texto, int x, int y, int ancho, int alto, bool sobreOscuro)
    {
        Color letra = sobreOscuro ? Blanco : Carbon;
        Button boton = BotonPlano(texto, x, y, ancho, alto, sobreOscuro ? Carbon : Blanco, letra,
            sobreOscuro ? Grafito : GrisClaro, letra);
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.BorderColor = sobreOscuro ? GrisMedio : GrisBorde;
        return boton;
    }

    // Opción tipo botón (Carro / Moto): la elegida se pinta de negro.
    public static RadioButton Opcion(string texto, int x, int y, int ancho, int alto)
    {
        RadioButton opcion = new RadioButton
        {
            Text = texto.ToUpper(),
            Location = new Point(x, y),
            Size = new Size(ancho, alto),
            Appearance = Appearance.Button,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Fuente(16),
            FlatStyle = FlatStyle.Flat,
            BackColor = Blanco,
            ForeColor = Carbon,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        opcion.FlatAppearance.BorderColor = GrisBorde;
        opcion.FlatAppearance.BorderSize = 1;
        opcion.FlatAppearance.CheckedBackColor = Carbon;
        opcion.FlatAppearance.MouseOverBackColor = GrisClaro;
        opcion.CheckedChanged += (sender, e) => opcion.ForeColor = opcion.Checked ? Blanco : Carbon;
        return opcion;
    }

    // Panel blanco que hace de tarjeta sobre el fondo gris.
    public static Panel Tarjeta(int ancho, int alto)
    {
        return new Panel
        {
            Size = new Size(ancho, alto),
            BackColor = Blanco,
            Margin = new Padding(0, 0, 0, 16)
        };
    }
}
