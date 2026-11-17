using Microsoft.AspNetCore.Mvc;

namespace Parqueadero.Web.Controllers;

// Por ahora solo muestra la vista. El inicio de sesión real se hará con ASP.NET Core Identity.
public class CuentaController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}
