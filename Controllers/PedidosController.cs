using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Identity;
using GRINPLAS.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Build.Framework;

namespace GRINPLAS.Controllers
{
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly ILogger<PedidosController> _logger;
        
        [ActivatorUtilitiesConstructor]
        public PedidosController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<PedidosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public PedidosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : this(context, userManager, NullLogger<PedidosController>.Instance)
        {
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Administrador(DateTime? fechaInicio, DateTime? fechaFin)
        {
           if (_userManager == null)
           {
               return RedirectToPage("/Account/AccessDenied");
           }
           var user = await _userManager.GetUserAsync(User);

            if(user == null){
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles= await _userManager.GetRolesAsync(user);

            if(!userRoles.Contains("Administrador")){
                return RedirectToPage("/Account/AccessDenied");
            }
            var viewModel = new PedidoViewModel{
                Pedidos = await _context.Pedidos.Include (p => p.Cliente).ToListAsync(),
                nuevoPedido = new Pedido(),
                Clientes = await _context.Clientes.ToListAsync(),
                 Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync()
            };
             ViewBag.TotalMangas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Mangas")
                .SumAsync(dp => dp.PrecioTotal);

            ViewBag.TotalBolsas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Bolsas")
                .SumAsync(dp => dp.PrecioTotal);


            return View(viewModel);
        }
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> GerenteGeneral(DateTime? fechaInicio, DateTime? fechaFin)
        {
           if (_userManager == null)
           {
               return RedirectToPage("/Account/AccessDenied");
           }
           var user = await _userManager.GetUserAsync(User);

            if(user == null){
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles= await _userManager.GetRolesAsync(user);

            if(!userRoles.Contains("Vendedor")){
                return RedirectToPage("/Account/AccessDenied");
            }
            var viewModel = new PedidoViewModel{
                Pedidos = await _context.Pedidos.Include (p => p.Cliente).ToListAsync(),
                nuevoPedido = new Pedido(),
                Clientes = await _context.Clientes.ToListAsync(),
                 Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync()
            };
             ViewBag.TotalMangas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Mangas")
                .SumAsync(dp => dp.PrecioTotal);

            ViewBag.TotalBolsas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Bolsas")
                .SumAsync(dp => dp.PrecioTotal);


            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> ActualizarPedido([FromForm] int pedidoId, [FromForm] string status, [FromForm] string pago, [FromForm] DateTime? fechaEntrega)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                pedido.Status = status;
                pedido.Pago = pago;
                
                if (fechaEntrega.HasValue)
                {
                    pedido.FechaEntrega = DateTime.SpecifyKind(fechaEntrega.Value, DateTimeKind.Utc);
                }
                else
                {
                    pedido.FechaEntrega = null;
                }

                _context.Update(pedido);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDireccion([FromForm] int pedidoId, [FromForm] string nuevaDireccion)
        {
            try
            {
                // Busca el pedido en la base de datos
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                // Actualiza la dirección
                pedido.Direccion = nuevaDireccion;
                _context.Update(pedido);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la dirección del pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }

        //cancelar pedido
        [HttpPost]
        public async Task<IActionResult> CancelarPedido([FromForm] int pedidoId)
        {
            try
            {
                // Busca el pedido en la base de datos
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                // Cambia el estado del pedido a "Cancelado"
                pedido.Status = "Cancelado";
                _context.Update(pedido);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar el pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }
    }
}
