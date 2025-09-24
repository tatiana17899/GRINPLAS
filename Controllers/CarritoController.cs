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
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GRINPLAS.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly ILogger<CarritoController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        
        [ActivatorUtilitiesConstructor]
        public CarritoController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<CarritoController> logger,
        IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public CarritoController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : this(context, userManager, NullLogger<CarritoController>.Instance, null!)
        {
        }

        public async Task<IActionResult> Cliente()
        {
            if (_userManager == null)
            {
                //verificar que usermanager esta disponible
                return View("NoAutorizado");
            }
            var user = await _userManager.GetUserAsync(User);

            if(user == null){
                return View("NoAutorizado");
            }
            var userRoles= await _userManager.GetRolesAsync(user);

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            if (cliente == null)
            {
                return View("NoAutorizado");
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
            //Mensaje cuando el carrito está vacío
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto([FromBody] DeleteProductRequest request)
        {
            if (_userManager == null)
            {
                return Json(new { success = false, message = "Acceso denegado." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente no encontrado." });
            }

            var carrito = await _context.Carrito
                .Include(c => c.detalleCarrito)
                .FirstOrDefaultAsync(c => c.ClienteId == cliente.ClienteId);

            if (carrito == null)
            {
                return Json(new { success = false, message = "Carrito no encontrado." });
            }

            var detalleCarrito = carrito.detalleCarrito
                .FirstOrDefault(dc => dc.ProductoId == request.ProductoId);

            if (detalleCarrito == null)
            {
                return Json(new { success = false, message = "Producto no encontrado en el carrito." });
            }

            _context.DetalleCarrito.Remove(detalleCarrito);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public class DeleteProductRequest
        {
            public int ProductoId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> GenerarPedido(string direccion, IFormFile comprobantePago)
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

            // Guardar el comprobante de pago
            string comprobanteFileName = string.Empty;
            
            if (comprobantePago != null && comprobantePago.Length > 0)
            {
                // Crear directorio de uploads si no existe
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                // Generar nombre único para el archivo
                comprobanteFileName = $"comprobante_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetExtension(comprobantePago.FileName)}";
                string filePath = Path.Combine(uploadsFolder, comprobanteFileName);
                
                // Guardar el archivo
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await comprobantePago.CopyToAsync(fileStream);
                }
            }

            string boletaFileName = $"Boleta_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string rutaArchivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "boletas");
            
            // Asegurarse de que exista el directorio
            if (!Directory.Exists(rutaArchivo))
            {
                Directory.CreateDirectory(rutaArchivo);
            }
            
            string rutaCompleta = Path.Combine(rutaArchivo, boletaFileName);
            string rutaRelativa = $"/documentos/boletas/{boletaFileName}";


            var pedido = new Pedido
            {
                ClienteId = cliente.ClienteId,
                Direccion = direccion,
                FechaEmision = DateTime.Now,
                Status = "Pendiente",
                Total = carrito.detalleCarrito.Sum(dc => dc.Subtotal),
                Pago = "Pediente",
                ComprobantePago = comprobanteFileName.Length > 0 ? $"/uploads/{comprobanteFileName}" : null,
                BoletaEmitida = rutaRelativa
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

            var detallesPedido = await _context.DetallePedidos
            .Where(dp => dp.PedidoId == pedido.PedidoId)
            .Include(dp => dp.Producto)
            .ThenInclude(p => p.Categoria)
            .ToListAsync();

        // Crear modelo para la vista de boleta
        var modelBoleta = new BoletaViewModel
        {
            Pedido = pedido,
            Cliente = cliente,
            DetallesPedido = detallesPedido
        };

        try
        {
            // Generar PDF con Rotativa
            var pdf = new ViewAsPdf("Boleta", modelBoleta)
            {
                FileName = boletaFileName,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };

            // Guardar físicamente el PDF
            var pdfData = await pdf.BuildFile(ControllerContext);
            await System.IO.File.WriteAllBytesAsync(rutaCompleta, pdfData);

            _context.DetalleCarrito.RemoveRange(carrito.detalleCarrito);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pedido generado correctamente.";
            TempData.Remove("SuccessMessage");
            return RedirectToAction("Cliente", "Carrito");
        }
        catch (Exception ex)
        {
            // En caso de error al generar el PDF
            TempData["ErrorMessage"] = $"Error al generar la boleta: {ex.Message}";
            // Podrías eliminar el pedido o marcarlo como fallido
            return RedirectToAction("Cliente", "Carrito");
         }
        }
        
        public IActionResult VerBoleta(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                .ThenInclude(dp => dp.Producto)
                .ThenInclude(p => p.Categoria)
                .FirstOrDefault(p => p.PedidoId == id);

            if (pedido == null)
            {
                return NotFound();
            }

            var modelBoleta = new BoletaViewModel
            {
                Pedido = pedido,
                Cliente = pedido.Cliente,
                DetallesPedido = pedido.DetallePedidos.ToList()
            };

            return new ViewAsPdf("Boleta", modelBoleta);
        }
    }
}
