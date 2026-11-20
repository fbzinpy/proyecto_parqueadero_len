using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;
using Parqueadero.Web.Datos;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

// Pantalla del rol Administrador: resumen del día, tarifas y usuarios.
// Solo el Administrador puede entrar.
[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private static readonly string[] Roles = { DatosIniciales.RolAdministrador, DatosIniciales.RolOperador };

    private readonly ServicioParqueadero _servicio;
    private readonly UserManager<IdentityUser> _usuarios;

    public AdminController(ServicioParqueadero servicio, UserManager<IdentityUser> usuarios)
    {
        _servicio = servicio;
        _usuarios = usuarios;
    }

    public async Task<IActionResult> Index()
    {
        return View(await CrearModeloAsync());
    }

    private async Task<AdminViewModel> CrearModeloAsync()
    {
        EstadoParqueadero estado = _servicio.CargarEstado();
        List<Cupo> cupos = _servicio.ObtenerCupos();
        var (ingresos, atendidos) = _servicio.ResumenDelDia();

        List<UsuarioFila> filas = new List<UsuarioFila>();
        foreach (IdentityUser usuario in _usuarios.Users.OrderBy(u => u.Email).ToList())
        {
            IList<string> rolesDelUsuario = await _usuarios.GetRolesAsync(usuario);
            filas.Add(new UsuarioFila { Id = usuario.Id, Correo = usuario.Email ?? "", Rol = string.Join(", ", rolesDelUsuario) });
        }

        return new AdminViewModel
        {
            IngresosDelDia = ingresos,
            VehiculosAtendidos = atendidos,
            CuposOcupados = estado.CuposOcupados,
            TotalCupos = estado.TotalCupos,
            Operadores = (await _usuarios.GetUsersInRoleAsync(DatosIniciales.RolOperador)).Count,
            CuposCarro = cupos.Count(c => c.Tipo == TipoVehiculo.Carro),
            CuposMoto = cupos.Count(c => c.Tipo == TipoVehiculo.Moto),
            Tarifas = _servicio.ObtenerTarifas(),
            Usuarios = filas
        };
    }

    // ---------- Tarifas ----------

    [HttpGet]
    public IActionResult EditarTarifas()
    {
        return View(_servicio.ObtenerTarifas());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarTarifas(decimal valorCarro, decimal valorMoto)
    {
        string? error = _servicio.ActualizarTarifas(valorCarro, valorMoto);
        if (error != null)
        {
            ViewData["Error"] = error;
            return View(_servicio.ObtenerTarifas());
        }

        TempData["Mensaje"] = "Tarifas actualizadas.";
        return RedirectToAction("Index");
    }

    // ---------- Usuarios ----------

    [HttpGet]
    public IActionResult NuevoUsuario()
    {
        ViewData["Roles"] = Roles;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoUsuario(string? correo, string? clave, string? rol)
    {
        ViewData["Roles"] = Roles;
        ViewData["Correo"] = correo;
        ViewData["Rol"] = rol;

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave) || rol == null || !Roles.Contains(rol))
        {
            ViewData["Error"] = "Completa el correo, la contraseña y el rol.";
            return View();
        }

        IdentityUser usuario = new IdentityUser { UserName = correo.Trim(), Email = correo.Trim(), EmailConfirmed = true };
        IdentityResult resultado = await _usuarios.CreateAsync(usuario, clave);
        if (!resultado.Succeeded)
        {
            ViewData["Error"] = TraducirErrores(resultado);
            return View();
        }

        await _usuarios.AddToRoleAsync(usuario, rol);
        TempData["Mensaje"] = $"Usuario {usuario.Email} creado con rol {rol}.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditarUsuario(string id)
    {
        IdentityUser? usuario = await _usuarios.FindByIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        ViewData["Roles"] = Roles;
        ViewData["Rol"] = (await _usuarios.GetRolesAsync(usuario)).FirstOrDefault();
        return View(usuario);
    }

    // Cambia el rol y, si se escribió, la contraseña.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUsuario(string id, string? rol, string? clave)
    {
        IdentityUser? usuario = await _usuarios.FindByIdAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        ViewData["Roles"] = Roles;
        ViewData["Rol"] = rol;

        if (rol == null || !Roles.Contains(rol))
        {
            ViewData["Error"] = "Elige un rol.";
            return View(usuario);
        }

        IList<string> rolesActuales = await _usuarios.GetRolesAsync(usuario);

        // Si el administrador se quitara su propio rol, quedaría por fuera de esta pantalla.
        bool esElMismo = usuario.Id == _usuarios.GetUserId(User);
        if (esElMismo && !rolesActuales.Contains(rol))
        {
            ViewData["Error"] = "No puedes cambiar tu propio rol.";
            return View(usuario);
        }

        if (!string.IsNullOrEmpty(clave))
        {
            // Primero se valida la nueva contraseña, para no dejar al usuario sin contraseña si no cumple las reglas.
            foreach (IPasswordValidator<IdentityUser> validador in _usuarios.PasswordValidators)
            {
                IdentityResult validacion = await validador.ValidateAsync(_usuarios, usuario, clave);
                if (!validacion.Succeeded)
                {
                    ViewData["Error"] = TraducirErrores(validacion);
                    return View(usuario);
                }
            }

            await _usuarios.RemovePasswordAsync(usuario);
            await _usuarios.AddPasswordAsync(usuario, clave);
        }

        if (!rolesActuales.Contains(rol) || rolesActuales.Count != 1)
        {
            await _usuarios.RemoveFromRolesAsync(usuario, rolesActuales);
            await _usuarios.AddToRoleAsync(usuario, rol);
        }

        TempData["Mensaje"] = $"Usuario {usuario.Email} actualizado.";
        return RedirectToAction("Index");
    }

    // Identity devuelve los errores en inglés; aquí se pasan a un mensaje en español.
    private static string TraducirErrores(IdentityResult resultado)
    {
        if (resultado.Errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
        {
            return "Ya existe un usuario con ese correo.";
        }
        if (resultado.Errors.Any(e => e.Code == "InvalidEmail" || e.Code == "InvalidUserName"))
        {
            return "El correo no es válido.";
        }
        if (resultado.Errors.Any(e => e.Code.StartsWith("Password")))
        {
            return "La contraseña debe tener mínimo 6 caracteres, una mayúscula, una minúscula, un número y un símbolo.";
        }
        return "No se pudo guardar el usuario.";
    }
}
