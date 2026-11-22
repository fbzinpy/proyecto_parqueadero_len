using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Parqueadero.Core.Datos;

// Crea los 2 roles y un usuario de prueba para cada uno, si todavía no existen.
// No va en la migración porque las contraseñas se deben guardar cifradas, y eso lo hace UserManager.
public static class DatosIniciales
{
    public const string RolAdministrador = "Administrador";
    public const string RolOperador = "Operador";

    public static async Task CrearRolesYUsuariosAsync(IServiceProvider servicios)
    {
        RoleManager<IdentityRole> roles = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<IdentityUser> usuarios = servicios.GetRequiredService<UserManager<IdentityUser>>();

        foreach (string rol in new[] { RolAdministrador, RolOperador })
        {
            if (!await roles.RoleExistsAsync(rol))
            {
                await roles.CreateAsync(new IdentityRole(rol));
            }
        }

        await CrearUsuarioAsync(usuarios, "admin@parqueadero.local", "Admin123!", RolAdministrador);
        await CrearUsuarioAsync(usuarios, "operador@parqueadero.local", "Operador123!", RolOperador);
    }

    private static async Task CrearUsuarioAsync(UserManager<IdentityUser> usuarios, string correo, string clave, string rol)
    {
        if (await usuarios.FindByEmailAsync(correo) != null)
        {
            return;
        }

        IdentityUser usuario = new IdentityUser { UserName = correo, Email = correo, EmailConfirmed = true };
        await usuarios.CreateAsync(usuario, clave);
        await usuarios.AddToRoleAsync(usuario, rol);
    }
}
