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
}
