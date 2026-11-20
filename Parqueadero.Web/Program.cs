using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Datos;
using Parqueadero.Core.Servicios;
using Parqueadero.Web.Datos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Base de datos SQLite compartida con la aplicación de escritorio.
builder.Services.AddDbContext<ParqueaderoDbContext>(opciones =>
    opciones.UseSqlite(RutaBaseDatos.CadenaConexion()));

// Lógica del parqueadero (de Core). Scoped: uno por cada petición, igual que el DbContext.
builder.Services.AddScoped<ServicioParqueadero>();

// ASP.NET Core Identity: usuarios y roles guardados en la misma base de datos.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ParqueaderoDbContext>();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    // Sin sesión iniciada se va al login.
    opciones.LoginPath = "/Cuenta/Login";
    // Con sesión pero sin el rol necesario (ej: un Operador entrando a Administración) vuelve al panel.
    opciones.AccessDeniedPath = "/Home/Index";
});

var app = builder.Build();

// Al arrancar: aplica las migraciones pendientes (crea el .db si no existe) y crea roles y usuarios.
using (var scope = app.Services.CreateScope())
{
    ParqueaderoDbContext db = scope.ServiceProvider.GetRequiredService<ParqueaderoDbContext>();
    db.Database.Migrate();
    await DatosIniciales.CrearRolesYUsuariosAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Primero se identifica quién es el usuario y después se revisa si tiene permiso.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
