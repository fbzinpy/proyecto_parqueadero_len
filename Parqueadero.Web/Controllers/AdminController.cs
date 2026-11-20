using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Web.Controllers;

// Pantalla del rol Administrador: resumen, tarifas y usuarios. Por ahora solo muestra la vista.
// Solo el Administrador puede entrar.
[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Botón "Comparar estructuras": mide los tiempos y vuelve a mostrar la misma pantalla con la tabla.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Comparar()
    {
        ViewData["Comparacion"] = ComparacionEstructuras.Ejecutar();
        return View("Index");
    }
}
