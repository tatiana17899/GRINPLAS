using GRINPLAS.Data;
using GRINPLAS.Models;
using GRINPLAS.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GRINPLAS.Controllers
{
    [Authorize]
    public class HistorialPedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HistorialPedidosController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Obtener todos los pedidos para el gerente
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.FechaEmision)
                .ToListAsync();

            // Calcular ganancias totales
            decimal totalBolsas = 0;
            decimal totalMangas = 0;

            foreach (var pedido in pedidos)
            {
                var detalles = await _context.DetallePedidos
                    .Include(dp => dp.Producto)
                    .ThenInclude(p => p.Categoria)
                    .Where(dp => dp.PedidoId == pedido.PedidoId)
                    .ToListAsync();

                foreach (var detalle in detalles)
                {
                    if (detalle.Producto?.Categoria?.Nombre?.ToLower().Contains("bolsa") == true)
                    {
                        totalBolsas += detalle.PrecioTotal;
                    }
                    else if (detalle.Producto?.Categoria?.Nombre?.ToLower().Contains("manga") == true)
                    {
                        totalMangas += detalle.PrecioTotal;
                    }
                }
            }

            ViewBag.TotalBolsas = totalBolsas;
            ViewBag.TotalMangas = totalMangas;

            var viewModel = new PedidoViewModel
            {
                Pedidos = pedidos
            };

            return View("GerenteGeneral", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarPedido(int pedidoId, string status, string pago, DateTime? fechaEntrega)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                
                if (pedido == null)
                {
                    return Json(new { success = false, error = "Pedido no encontrado" });
                }

                pedido.Status = status;
                pedido.Pago = pago;
                pedido.FechaEntrega = fechaEntrega;

                _context.Update(pedido);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Método para mostrar el comprobante de pago
        public IActionResult VerComprobante(int id)
        {
            var pedido = _context.Pedidos.Find(id);
            
            if (pedido == null || string.IsNullOrEmpty(pedido.ComprobantePago))
            {
                return NotFound();
            }

            // Quitar la barra inicial si existe
            var rutaRelativa = pedido.ComprobantePago.TrimStart('/');
            
            // Obtener la ruta completa del archivo
            var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, rutaRelativa);
            
            if (!System.IO.File.Exists(rutaCompleta))
            {
                return NotFound();
            }

            // Determinar el tipo de contenido según la extensión
            var extension = Path.GetExtension(rutaCompleta).ToLowerInvariant();
            string contentType;
            
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            // Devolver el archivo físico
            return PhysicalFile(rutaCompleta, contentType);
        }
    }
}