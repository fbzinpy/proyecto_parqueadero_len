using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Entidades;

namespace Parqueadero.Core.Datos;

// Hereda de IdentityDbContext para que las tablas de usuarios y roles (ASP.NET Core Identity)
// queden en la misma base de datos que los cupos, tickets y tarifas.
public class ParqueaderoDbContext : IdentityDbContext
{
    public const int CuposCarro = 85;
    public const int CuposMoto = 20;

    public ParqueaderoDbContext(DbContextOptions<ParqueaderoDbContext> options) : base(options)
    {
    }

    public DbSet<Cupo> Cupos => Set<Cupo>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero Identity configura sus tablas (AspNetUsers, AspNetRoles, ...).
        base.OnModelCreating(modelBuilder);

        // Datos iniciales (seed): quedan dentro de la migración.
        modelBuilder.Entity<Cupo>().HasData(CrearCupos());

        modelBuilder.Entity<Tarifa>().HasData(
            new Tarifa { Id = 1, Tipo = TipoVehiculo.Carro, ValorHora = 3000m },
            new Tarifa { Id = 2, Tipo = TipoVehiculo.Moto, ValorHora = 1500m });
    }

    // 105 cupos: A-01 a A-85 para carros y M-01 a M-20 para motos.
    private static List<Cupo> CrearCupos()
    {
        List<Cupo> cupos = new List<Cupo>();
        int id = 1;

        for (int i = 1; i <= CuposCarro; i++)
        {
            cupos.Add(new Cupo { Id = id, Codigo = $"A-{i:00}", Tipo = TipoVehiculo.Carro });
            id++;
        }

        for (int i = 1; i <= CuposMoto; i++)
        {
            cupos.Add(new Cupo { Id = id, Codigo = $"M-{i:00}", Tipo = TipoVehiculo.Moto });
            id++;
        }

        return cupos;
    }
}
