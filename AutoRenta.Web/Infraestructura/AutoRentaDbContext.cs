using AutoRenta.Web.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AutoRenta.Web.Infraestructura;

/// <summary>Contexto de Entity Framework Core (base de datos AutoRentaDB).</summary>
public class AutoRentaDbContext : DbContext
{
    public AutoRentaDbContext(DbContextOptions<AutoRentaDbContext> options) : base(options) { }

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Vehiculo>(e =>
        {
            e.Property(v => v.Placa).HasMaxLength(10);
            e.Property(v => v.Marca).HasMaxLength(50);
            e.Property(v => v.Modelo).HasMaxLength(50);
            e.Property(v => v.Categoria).HasMaxLength(30);
            e.Property(v => v.Transmision).HasMaxLength(20);
            e.HasIndex(v => v.Placa).IsUnique();
            e.Property(v => v.TarifaDiaria).HasPrecision(18, 2);
            e.Property(v => v.EstadoActual).HasMaxLength(20);
            e.Ignore(v => v.EstaDisponible);
        });
        mb.Entity<Reserva>(e =>
        {
            e.Property(r => r.CostoEstimado).HasPrecision(18, 2);
            e.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Codigo).HasMaxLength(20);
            e.Ignore(r => r.Dias);
        });
        mb.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nombre).HasMaxLength(120);
            e.Property(c => c.Correo).HasMaxLength(150);
            e.HasIndex(c => c.Correo).IsUnique();
        });
    }
}
