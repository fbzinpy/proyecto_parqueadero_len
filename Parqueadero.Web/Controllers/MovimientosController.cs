using Microsoft.AspNetCore.Mvc;

namespace Parqueadero.Web.Controllers;

// Entradas y salidas de vehículos. Por ahora solo muestra las vistas.
public class MovimientosController : Controller
{
    public IActionResult Entrada()
    {
        return View();
    }

    public IActionResult Salida()
    {
        return View();
    }
}
