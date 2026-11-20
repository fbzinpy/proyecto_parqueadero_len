using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Parqueadero.Web.Controllers;

// Entradas y salidas de vehículos. Por ahora solo muestra las vistas.
// Pueden entrar los dos roles.
[Authorize(Roles = "Administrador,Operador")]
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
