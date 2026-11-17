using Microsoft.AspNetCore.Mvc;

namespace Parqueadero.Web.Controllers;

// Pantalla del rol Administrador: resumen, tarifas y usuarios. Por ahora solo muestra la vista.
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
