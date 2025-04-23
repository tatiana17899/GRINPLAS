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
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly ILogger<CarritoController> _logger;
        
        [ActivatorUtilitiesConstructor]
        public CarritoController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<CarritoController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public CarritoController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : this(context, userManager, NullLogger<CarritoController>.Instance)
        {
        }

        public async Task<IActionResult> Cliente()
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

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            if (cliente == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            var carrito = await _context.Carrito
                .Include(c => c.detalleCarrito)
                .ThenInclude(dc => dc.Producto)
                .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(c => c.ClienteId == cliente.ClienteId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                ClienteId = cliente.ClienteId,
                Cliente = cliente,
                FechaCreacion = DateTime.Now,
                detalleCarrito = new List<DetalleCarrito>()
                };
                _context.Carrito.Add(carrito);
                await _context.SaveChangesAsync();
            }

            // Verificar si el carrito está vacío
            if (carrito.detalleCarrito == null || carrito.detalleCarrito.Count == 0)
            {
                TempData["CarritoVacioError"] = "No hay productos seleccionados en el carrito";
            }

            var viewModel = new CarritoViewModel
            {
                Productos = carrito.detalleCarrito.Select(dc => dc.Producto).Where(producto => producto != null).ToList()!,
                Carrito = carrito.detalleCarrito.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerarPedido(string direccion, string comprobantePago)
        {
            if (_userManager == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles = await _userManager.GetRolesAsync(user);

         

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            if (cliente == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            var carrito = await _context.Carrito
                .Include(c => c.detalleCarrito)
                .ThenInclude(dc => dc.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == cliente.ClienteId);

            if (carrito == null || carrito.detalleCarrito.Count == 0)
            {
                TempData["ErrorMessage"] = "No hay productos en el carrito.";
                return RedirectToAction("Cliente", "Carrito");
            }

            var pedido = new Pedido
            {
                ClienteId = cliente.ClienteId,
                Direccion = direccion,
                FechaEmision = DateTime.Now,
                Status = "Pendiente",
                Total = carrito.detalleCarrito.Sum(dc => dc.Subtotal),
                Pago = comprobantePago,
                BoletaEmitida = $"Boleta_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            foreach (var detalle in carrito.detalleCarrito)
            {
                var producto = detalle.Producto;
                if (producto != null)
                {
                    producto.Stock -= detalle.Cantidad;
                    _context.Productos.Update(producto);

                    var detallePedido = new DetallePedido
                    {
                        PedidoId = pedido.PedidoId,
                        ProductoId = producto.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = (decimal) producto.Precio,
                        PrecioTotal = detalle.Subtotal
                    };
                    _context.DetallePedidos.Add(detallePedido);
                    await _context.SaveChangesAsync();
                }
            }

            _context.DetalleCarrito.RemoveRange(carrito.detalleCarrito);
            await _context.SaveChangesAsync();

            return RedirectToAction("Cliente", "Carrito");
        }

    }
}