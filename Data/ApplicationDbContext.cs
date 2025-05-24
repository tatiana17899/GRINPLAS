using GRINPLAS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GRINPLAS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<GRINPLAS.Models.Categoria> Categorias { get; set; }
    public DbSet<GRINPLAS.Models.Cliente> Clientes { get; set; }
    public DbSet<GRINPLAS.Models.DetallePedido> DetallePedidos { get; set; }
    public DbSet<GRINPLAS.Models.Pedido> Pedidos { get; set; }
    public DbSet<GRINPLAS.Models.Producto> Productos { get; set; }
    public DbSet<GRINPLAS.Models.Carrito> Carrito { get; set; }
    public DbSet<GRINPLAS.Models.DetalleCarrito> DetalleCarrito { get; set; }
    public DbSet<GRINPLAS.Models.Trabajadores> Trabajadores { get; set; }
    public DbSet<GRINPLAS.Models.Gasto> Gastos { get; set; }
    public DbSet<GRINPLAS.Models.Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Trabajador)
            .WithOne(t => t.User)
            .HasForeignKey<Trabajadores>(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.FecCre)
            .HasColumnType("timestamp without time zone");
    }
}
