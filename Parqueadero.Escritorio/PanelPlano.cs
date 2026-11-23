using System.Drawing.Drawing2D;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

// Control propio que dibuja el plano del parqueadero con GDI+ (las posiciones vienen de Core, igual que en la web).
// Dibuja en las coordenadas del plano (820 x 1000) y las escala para que quepa en el panel.
public class PanelPlano : Panel
{
    private static readonly Color Asfalto = ColorTranslator.FromHtml("#3b3d40");
    private static readonly Color Pasto = ColorTranslator.FromHtml("#8fae6a");
    private static readonly Color Anden = ColorTranslator.FromHtml("#d9d6cf");
    private static readonly Color EdificioBorde = ColorTranslator.FromHtml("#9a9a98");
    private static readonly Color EdificioRelleno = ColorTranslator.FromHtml("#b0b0ad");
    private static readonly Color TextoOscuro = ColorTranslator.FromHtml("#1e2a4a");
    private static readonly Color Amarillo = ColorTranslator.FromHtml("#ffc000");

    // (x, y, radio) de cada árbol
    private static readonly (int X, int Y, int R)[] Arboles =
    {
        (40, 40, 26), (80, 100, 24), (770, 34, 26), (190, 745, 24), (245, 772, 28), (295, 745, 22), (190, 805, 20),
        (740, 950, 30), (790, 930, 24),
        (718, 120, 20), (718, 200, 20), (718, 280, 20), (718, 360, 20), (718, 440, 20),
        (718, 520, 20), (718, 600, 20), (718, 680, 20), (718, 756, 20)
    };

    private readonly List<PosicionCupo> _posiciones = PlanoParqueadero.ObtenerPosiciones();
    private Dictionary<string, Cupo> _cuposPorCodigo = new Dictionary<string, Cupo>();
    private HashSet<int> _ocupados = new HashSet<int>();

    // Cupo que el usuario tocó (null si ninguno). Se guarda el código para conservarlo al refrescar.
    private string? _codigoSeleccionado;

    public event EventHandler? SeleccionCambio;

    public PanelPlano()
    {
        DoubleBuffered = true; // Evita el parpadeo al volver a dibujar.
    }

    public PosicionCupo? PosicionSeleccionada => _posiciones.FirstOrDefault(p => p.Codigo == _codigoSeleccionado);

    public Cupo? CupoSeleccionado
    {
        get
        {
            if (_codigoSeleccionado != null && _cuposPorCodigo.TryGetValue(_codigoSeleccionado, out Cupo? cupo))
            {
                return cupo;
            }
            return null;
        }
    }

    public bool EstaOcupado(Cupo cupo)
    {
        return _ocupados.Contains(cupo.Id);
    }

    // Recibe los datos frescos de la base de datos y vuelve a dibujar.
    public void Actualizar(List<Cupo> cupos, EstadoParqueadero estado)
    {
        _cuposPorCodigo = cupos.ToDictionary(c => c.Codigo);
        _ocupados = cupos.Where(c => estado.EstaOcupado(c.Id)).Select(c => c.Id).ToHashSet();
        Invalidate();
        SeleccionCambio?.Invoke(this, EventArgs.Empty);
    }

    // ---------- Escala ----------

    // Escala que hace caber el plano completo conservando la proporción, y el margen para centrarlo.
    private (float Escala, float DesdeX, float DesdeY) Medidas()
    {
        float escala = Math.Min(ClientSize.Width / (float)PlanoParqueadero.Ancho, ClientSize.Height / (float)PlanoParqueadero.Alto);
        float desdeX = (ClientSize.Width - PlanoParqueadero.Ancho * escala) / 2;
        float desdeY = (ClientSize.Height - PlanoParqueadero.Alto * escala) / 2;
        return (escala, desdeX, desdeY);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        Invalidate();
    }

    // ---------- Clic ----------

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        // Del punto del mouse (píxeles) a coordenadas del plano.
        (float escala, float desdeX, float desdeY) = Medidas();
        double px = (e.X - desdeX) / escala;
        double py = (e.Y - desdeY) / escala;

        string? encontrado = null;
        foreach (PosicionCupo p in _posiciones)
        {
            // "Deshacer" la rotación del cupo: se gira el punto al revés alrededor del centro
            // y así basta con comprobar si cae dentro de un rectángulo sin girar.
            double ang = p.Rotacion * Math.PI / 180;
            double dx = px - p.X;
            double dy = py - p.Y;
            double lx = dx * Math.Cos(ang) + dy * Math.Sin(ang);
            double ly = -dx * Math.Sin(ang) + dy * Math.Cos(ang);

            if (Math.Abs(lx) <= p.Ancho / 2 && Math.Abs(ly) <= p.Alto / 2)
            {
                encontrado = p.Codigo;
                break;
            }
        }

        if (encontrado != null)
        {
            _codigoSeleccionado = encontrado;
            Invalidate();
            SeleccionCambio?.Invoke(this, EventArgs.Empty);
        }
    }

    // ---------- Dibujo ----------

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Anden);

        (float escala, float desdeX, float desdeY) = Medidas();
        g.TranslateTransform(desdeX, desdeY);
        g.ScaleTransform(escala, escala);

        DibujarFondo(g);

        PosicionCupo? seleccionada = null;
        foreach (PosicionCupo p in _posiciones)
        {
            if (p.Codigo == _codigoSeleccionado)
            {
                seleccionada = p; // Se dibuja al final para que su borde quede encima.
                continue;
            }
            DibujarCupo(g, p, false);
        }

        if (seleccionada != null)
        {
            DibujarCupo(g, seleccionada, true);
        }
    }

    private void DibujarCupo(Graphics g, PosicionCupo p, bool seleccionado)
    {
        GraphicsState estado = g.Save();

        // Se mueve el origen al centro del cupo y se gira: así el rectángulo se dibuja centrado y sin cuentas.
        g.TranslateTransform((float)p.X, (float)p.Y);
        g.RotateTransform((float)p.Rotacion);

        float mitadAncho = (float)p.Ancho / 2;
        float mitadAlto = (float)p.Alto / 2;

        using (Pen borde = new Pen(seleccionado ? Amarillo : Color.White, seleccionado ? 3f : 2f))
        {
            g.DrawRectangle(borde, -mitadAncho, -mitadAlto, (float)p.Ancho, (float)p.Alto);
        }

        if (_cuposPorCodigo.TryGetValue(p.Codigo, out Cupo? cupo) && _ocupados.Contains(cupo.Id))
        {
            // Carro visto desde arriba: carrocería y vidrio, un poco más chicos que el cupo.
            bool vertical = p.Alto > p.Ancho;
            float cw = (float)p.Ancho - 6;
            float ch = (float)p.Alto - 6;
            float vw = vertical ? cw - 4 : ch * 0.35f;
            float vh = vertical ? ch * 0.35f : ch - 4;
            float vx = vertical ? -vw / 2 : cw / 2 - vw - 3;
            float vy = vertical ? -ch / 2 + 4 : -vh / 2;

            using (GraphicsPath carroceria = RectanguloRedondeado(-cw / 2, -ch / 2, cw, ch, 3))
            using (SolidBrush blanco = new SolidBrush(ColorTranslator.FromHtml("#f2f2f2")))
            {
                g.FillPath(blanco, carroceria);
            }

            using (GraphicsPath vidrio = RectanguloRedondeado(vx, vy, vw, vh, 2))
            using (SolidBrush gris = new SolidBrush(ColorTranslator.FromHtml("#9aa3ad")))
            {
                g.FillPath(gris, vidrio);
            }
        }

        g.Restore(estado);
    }

    private static GraphicsPath RectanguloRedondeado(float x, float y, float ancho, float alto, float radio)
    {
        float d = radio * 2;
        GraphicsPath ruta = new GraphicsPath();
        ruta.AddArc(x, y, d, d, 180, 90);
        ruta.AddArc(x + ancho - d, y, d, d, 270, 90);
        ruta.AddArc(x + ancho - d, y + alto - d, d, d, 0, 90);
        ruta.AddArc(x, y + alto - d, d, d, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

    // Curva "Q" del SVG (un solo punto de control) convertida a la curva cúbica que usa GDI+.
    private static void Cuadratica(GraphicsPath ruta, PointF p0, PointF p1, PointF p2)
    {
        PointF c1 = new PointF(p0.X + 2f / 3 * (p1.X - p0.X), p0.Y + 2f / 3 * (p1.Y - p0.Y));
        PointF c2 = new PointF(p2.X + 2f / 3 * (p1.X - p2.X), p2.Y + 2f / 3 * (p1.Y - p2.Y));
        ruta.AddBezier(p0, c1, c2, p2);
    }

    private static void Rellenar(Graphics g, string color, float x, float y, float ancho, float alto)
    {
        using (SolidBrush pincel = new SolidBrush(ColorTranslator.FromHtml(color)))
        {
            g.FillRectangle(pincel, x, y, ancho, alto);
        }
    }

    private static void Poligono(Graphics g, string color, params PointF[] puntos)
    {
        using (SolidBrush pincel = new SolidBrush(ColorTranslator.FromHtml(color)))
        {
            g.FillPolygon(pincel, puntos);
        }
    }

    // Línea blanca punteada (carriles).
    private static void Carril(Graphics g, float x1, float y1, float x2, float y2, float raya, float espacio)
    {
        using (Pen lapiz = new Pen(Color.White, 2f))
        {
            lapiz.DashPattern = new[] { raya / 2, espacio / 2 }; // El patrón se mide en múltiplos del grosor (2).
            g.DrawLine(lapiz, x1, y1, x2, y2);
        }
    }

    private static void Edificio(Graphics g, float x, float y, float ancho, float alto, float margen)
    {
        Rellenar(g, "#9a9a98", x, y, ancho, alto);
        Rellenar(g, "#b0b0ad", x + margen, y + margen, ancho - margen * 2, alto - margen * 2);
    }

    // sobreAsfalto: el texto va sobre un recuadro del color del asfalto, para que la línea punteada no lo atraviese.
    private static void Texto(Graphics g, string texto, float x, float y, Color color, bool centrado, bool sobreAsfalto = false)
    {
        using (Font fuente = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (SolidBrush pincel = new SolidBrush(color))
        using (StringFormat formato = new StringFormat())
        {
            formato.Alignment = centrado ? StringAlignment.Center : StringAlignment.Near;
            string mayusculas = texto.ToUpper();

            if (sobreAsfalto)
            {
                SizeF medida = g.MeasureString(mayusculas, fuente);
                using (SolidBrush fondo = new SolidBrush(Asfalto))
                {
                    g.FillRectangle(fondo, x - 2, y - 11, medida.Width - 4, medida.Height);
                }
            }

            g.DrawString(mayusculas, fuente, pincel, x, y - 11, formato);
        }
    }

    private static void DibujarFondo(Graphics g)
    {
        // Calle norte
        Rellenar(g, "#8fae6a", 128, 0, 570, 8);
        Rellenar(g, "#3b3d40", 128, 8, 570, 40);
        Carril(g, 140, 28, 690, 28, 14, 12);
        Rellenar(g, "#8fae6a", 128, 48, 570, 8);

        // Pasto de las esquinas
        Rellenar(g, "#8fae6a", 0, 0, 110, 150);
        Rellenar(g, "#8fae6a", 720, 0, 100, 70);

        // Edificios
        Edificio(g, 4, 170, 114, 240, 8);
        Edificio(g, 4, 440, 114, 340, 8);
        Edificio(g, 744, 110, 76, 300, 6);
        Edificio(g, 744, 460, 76, 280, 6);

        // Lote: borde de pasto y asfalto encima
        Poligono(g, "#8fae6a", new PointF(120, 52), new PointF(698, 52), new PointF(702, 784), new PointF(572, 912), new PointF(250, 934), new PointF(120, 878));
        Poligono(g, "#3b3d40", new PointF(130, 60), new PointF(690, 60), new PointF(692, 780), new PointF(565, 905), new PointF(250, 925), new PointF(130, 870));

        // Portería
        Rellenar(g, "#3b3d40", 0, 840, 136, 64);
        Rellenar(g, "#8fae6a", 0, 832, 132, 8);
        Rellenar(g, "#8fae6a", 0, 904, 128, 8);
        Carril(g, 0, 872, 130, 872, 12, 10);

        // Pasto abajo a la derecha
        Poligono(g, "#8fae6a", new PointF(610, 1000), new PointF(700, 900), new PointF(820, 900), new PointF(820, 1000));

        // Carriles del lote
        Carril(g, 150, 140, 640, 140, 14, 12);
        Carril(g, 312, 230, 312, 700, 14, 12);
        Carril(g, 449, 230, 449, 700, 14, 12);
        Carril(g, 600, 200, 600, 760, 14, 12);
        Carril(g, 350, 820, 560, 820, 14, 12);

        // Zona de motos (rectángulo punteado)
        using (Pen lapiz = new Pen(Color.White, 2f))
        {
            lapiz.DashPattern = new[] { 3f, 3f };
            g.DrawRectangle(lapiz, 148, 228, 144, 466);
        }

        // Isla de pasto
        using (GraphicsPath isla = new GraphicsPath())
        using (SolidBrush pasto = new SolidBrush(Pasto))
        {
            isla.AddLine(150, 720, 300, 720);
            Cuadratica(isla, new PointF(300, 720), new PointF(340, 730), new PointF(330, 770));
            Cuadratica(isla, new PointF(330, 770), new PointF(300, 820), new PointF(200, 830));
            isla.AddLine(200, 830, 150, 830);
            isla.CloseFigure();
            g.FillPath(pasto, isla);
        }

        // Árboles: 3 círculos superpuestos
        foreach ((int x, int y, int r) in Arboles)
        {
            Circulo(g, "#5f8f3e", x, y, r);
            Circulo(g, "#77a64d", x, y - 6, r * 0.65f);
            Circulo(g, "#8fbf5f", x, y - 10, r * 0.3f);
        }

        // Flecha de la portería
        Poligono(g, "#ffc000",
            new PointF(196, 852), new PointF(160, 852), new PointF(160, 842), new PointF(140, 858),
            new PointF(160, 874), new PointF(160, 864), new PointF(196, 864));

        // Textos
        Texto(g, "Edificio", 60, 290, TextoOscuro, true);
        Texto(g, "Coliseo múltiple USC", 60, 610, TextoOscuro, true);
        Texto(g, "Bloques", 785, 260, TextoOscuro, true);
        Texto(g, "Bloques", 785, 600, TextoOscuro, true);
        Texto(g, "Motos", 158, 248, TextoOscuro, true);
        Texto(g, "← Portería · entrada y salida", 8, 928, TextoOscuro, false);
        Texto(g, "Norte · consulado", 330, 32, Color.White, false, true);
        Texto(g, "Fila superior · al fondo", 380, 144, Color.White, false, true);
    }

    private static void Circulo(Graphics g, string color, float x, float y, float r)
    {
        using (SolidBrush pincel = new SolidBrush(ColorTranslator.FromHtml(color)))
        {
            g.FillEllipse(pincel, x - r, y - r, r * 2, r * 2);
        }
    }
}
