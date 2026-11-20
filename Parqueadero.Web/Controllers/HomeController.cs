using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class HomeController : Controller
{
    // Panel del operador: ocupación, vehículos dentro y mapa de cupos. Pueden entrar los dos roles.
    [Authorize(Roles = "Administrador,Operador")]
    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
