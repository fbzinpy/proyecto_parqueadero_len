using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Datos;

namespace Parqueadero.Escritorio;

public record UsuarioFila(string Id, string Correo, string Rol);

// Usuarios y roles para la pantalla de Administración. Hace lo mismo que AdminController en la web,
// con el mismo UserManager de Identity y la misma base de datos.
public static class GestorUsuarios
{
    public static readonly string[] Roles = { DatosIniciales.RolAdministrador, DatosIniciales.RolOperador };

    public static async Task<List<UsuarioFila>> ListarAsync()
    {
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            UserManager<IdentityUser> usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            List<UsuarioFila> filas = new List<UsuarioFila>();

            foreach (IdentityUser usuario in usuarios.Users.OrderBy(u => u.Email).ToList())
            {
                IList<string> roles = await usuarios.GetRolesAsync(usuario);
                filas.Add(new UsuarioFila(usuario.Id, usuario.Email ?? "", string.Join(", ", roles)));
            }

            return filas;
        }
    }

    public static async Task<int> ContarOperadoresAsync()
    {
        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            UserManager<IdentityUser> usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            return (await usuarios.GetUsersInRoleAsync(DatosIniciales.RolOperador)).Count;
        }
    }

    // Devuelve null si se creó, o el mensaje de error.
    public static async Task<string?> CrearAsync(string? correo, string? clave, string? rol)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave) || rol == null || !Roles.Contains(rol))
        {
            return "Completa el correo, la contraseña y el rol.";
        }

        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            UserManager<IdentityUser> usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            IdentityUser usuario = new IdentityUser { UserName = correo.Trim(), Email = correo.Trim(), EmailConfirmed = true };
            IdentityResult resultado = await usuarios.CreateAsync(usuario, clave);
            if (!resultado.Succeeded)
            {
                return TraducirErrores(resultado);
            }

            await usuarios.AddToRoleAsync(usuario, rol);
            return null;
        }
    }

    // Cambia el rol y, si se escribió, la contraseña. Devuelve null si salió bien, o el mensaje de error.
    public static async Task<string?> EditarAsync(string id, string? rol, string? clave, string correoDeQuienEdita)
    {
        if (rol == null || !Roles.Contains(rol))
        {
            return "Elige un rol.";
        }

        using (IServiceScope scope = Program.Servicios.CreateScope())
        {
            UserManager<IdentityUser> usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            IdentityUser? usuario = await usuarios.FindByIdAsync(id);
            if (usuario == null)
            {
                return "Ese usuario ya no existe.";
            }

            IList<string> rolesActuales = await usuarios.GetRolesAsync(usuario);

            // Si el administrador se quitara su propio rol, quedaría por fuera de la pantalla de Administración.
            bool esElMismo = string.Equals(usuario.Email, correoDeQuienEdita, StringComparison.OrdinalIgnoreCase);
            if (esElMismo && !rolesActuales.Contains(rol))
            {
                return "No puedes cambiar tu propio rol.";
            }

            if (!string.IsNullOrEmpty(clave))
            {
                // Primero se valida la nueva contraseña, para no dejar al usuario sin contraseña si no cumple las reglas.
                foreach (IPasswordValidator<IdentityUser> validador in usuarios.PasswordValidators)
                {
                    IdentityResult validacion = await validador.ValidateAsync(usuarios, usuario, clave);
                    if (!validacion.Succeeded)
                    {
                        return TraducirErrores(validacion);
                    }
                }

                await usuarios.RemovePasswordAsync(usuario);
                await usuarios.AddPasswordAsync(usuario, clave);
            }

            if (!rolesActuales.Contains(rol) || rolesActuales.Count != 1)
            {
                await usuarios.RemoveFromRolesAsync(usuario, rolesActuales);
                await usuarios.AddToRoleAsync(usuario, rol);
            }

            return null;
        }
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
