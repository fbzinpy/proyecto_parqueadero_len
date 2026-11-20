using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Parqueadero.Web.Controllers;

// Inicio y cierre de sesión con ASP.NET Core Identity.
public class CuentaController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public CuentaController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    // returnUrl: la página que el usuario quería ver antes de que lo mandaran al login.
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string correo, string clave, bool recordarme, string? returnUrl = null)
    {
        // El usuario se creó con UserName = correo, por eso se inicia sesión con el correo.
        var resultado = await _signInManager.PasswordSignInAsync(correo, clave, recordarme, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            // Url.IsLocalUrl evita que nos redirijan a otro sitio web.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Correo"] = correo;
        ViewData["Error"] = "Correo o contraseña incorrectos.";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
