using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parqueadero.Core.Datos;
using Parqueadero.Core.Servicios;

namespace Parqueadero.Escritorio;

static class Program
{
    // Contenedor de servicios de la aplicación. Los formularios lo usan para crear
    // un scope nuevo por cada operación (cada clic), y así leer siempre datos frescos.
    public static ServiceProvider Servicios { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Mismos servicios que la web, sin lo que depende del navegador.
        ServiceCollection servicios = new ServiceCollection();

        // Base de datos SQLite: el mismo archivo .db que usa la web.
        servicios.AddDbContext<ParqueaderoDbContext>(opciones =>
            opciones.UseSqlite(RutaBaseDatos.CadenaConexion()));

        // Identity sin SignInManager (ese usa cookies del navegador): solo usuarios y roles.
        servicios.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ParqueaderoDbContext>();

        // Lógica del parqueadero (de Core). UserManager necesita el logging.
        servicios.AddScoped<ServicioParqueadero>();
        servicios.AddLogging();

        Servicios = servicios.BuildServiceProvider();

        // Al arrancar, igual que la web: aplica las migraciones y crea roles y usuarios.
        // Main no puede ser async en WinForms (perdería el [STAThread]), por eso se espera con GetAwaiter().GetResult().
        using (IServiceScope scope = Servicios.CreateScope())
        {
            ParqueaderoDbContext db = scope.ServiceProvider.GetRequiredService<ParqueaderoDbContext>();
            db.Database.Migrate();
            DatosIniciales.CrearRolesYUsuariosAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }

        // La aplicación empieza por el login.
        Application.Run(new FormLogin());
    }
}
