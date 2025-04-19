using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization; 
using GRINPLAS.Data;
using GRINPLAS.Models;
using GRINPLAS.ViewModel;
using Microsoft.Build.Framework;

namespace GRINPLAS.Controllers
{
    [Authorize(Roles = "GerenteGeneral")]
    public class AnalisisVentasController : Controller
    {
        private readonly ILogger<AnalisisVentasController> _logger;
        private readonly ApplicationDbContext _context;

        public AnalisisVentasController(ILogger<AnalisisVentasController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AnalisisViewModel();
            try
            {
                // Obtener datos base sin filtros para la vista inicial
                await LoadBaseData(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el análisis de ventas");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string period, string category, DateTime? customDate)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
                
                // Consulta base de pedidos filtrados por período
                var pedidosQuery = _context.Pedidos
                    .Include(p => p.DetallePedidos)
                        .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                    .AsQueryable();

                // Aplicar filtro de fecha
                switch (period)
                {
                    case "semana":
                        var startOfWeek = todayLocal.AddDays(-(int)todayLocal.DayOfWeek);
                        pedidosQuery = pedidosQuery.Where(p => p.FechaEmision >= startOfWeek && p.FechaEmision <= todayLocal);
                        break;
                    case "mes":
                        pedidosQuery = pedidosQuery.Where(p => p.FechaEmision.Month == todayLocal.Month && p.FechaEmision.Year == todayLocal.Year);
                        break;
                    case "año":
                        pedidosQuery = pedidosQuery.Where(p => p.FechaEmision.Year == todayLocal.Year);
                        break;
                    case "custom":
                        if (!customDate.HasValue) return Json(new { success = false, error = "Fecha no especificada" });
                        pedidosQuery = pedidosQuery.Where(p => p.FechaEmision.Date == customDate.Value.Date);
                        break;
                    default:
                        pedidosQuery = pedidosQuery.Where(p => p.FechaEmision >= todayLocal.AddDays(-7) && p.FechaEmision <= todayLocal);
                        break;
                }

                var pedidosFiltrados = await pedidosQuery.ToListAsync();

                // Obtener productos según categoría seleccionada
                var productosQuery = _context.Productos
                    .Include(p => p.Categoria)
                    .AsQueryable();

                if (category != "todos")
                {
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre == category);
                }

                var productos = await productosQuery.ToListAsync();

                // Contar cantidad total vendida por producto (no pedidos)
                var resultado = productos.Select(p => new
                {
                    NombreProducto = p.Nombre,
                    CantidadVendida = pedidosFiltrados
                        .SelectMany(ped => ped.DetallePedidos)
                        .Where(d => d.ProductoId == p.ProductoId)
                        .Sum(d => d.Cantidad)
                })
                .Where(x => x.CantidadVendida > 0) // Solo productos con ventas
                .OrderByDescending(x => x.CantidadVendida)
                .ToList();

                return Json(new
                {
                    labels = resultado.Select(x => x.NombreProducto).ToArray(),
                    values = resultado.Select(x => x.CantidadVendida).ToArray(),
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos para el gráfico");
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProductos(string categoria)
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Categoria.Nombre == categoria)
                    .Select(p => new { 
                        id = p.ProductoId,
                        nombre = p.Nombre 
                    })
                    .ToListAsync();

                return Json(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo productos");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVentasProducto(string producto, string fecha)
        {
            try
            {
                // Parsear fecha sin conversión de zona horaria
                if (!DateTime.TryParse(fecha, out var fechaDate))
                {
                    return Json(new { success = false, error = "Formato de fecha inválido" });
                }

                var cantidad = await _context.DetallePedidos
                    .Include(d => d.Producto)
                    .Include(d => d.Pedido)
                    .Where(d => d.Producto.Nombre == producto && 
                        d.Pedido.FechaEmision.Date == fechaDate.Date)
                    .SumAsync(d => (int?)d.Cantidad) ?? 0; // Manejo de valores nulos

                return Json(new { 
                    success = true,
                    cantidad 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ventas por producto");
                return Json(new { 
                    success = false, 
                    error = ex.Message,
                    cantidad = 0 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStockProducto(string producto)
        {
            try
            {
                var cantidad = await _context.Productos
                    .Where(p => p.Nombre == producto)
                    .Select(p => p.Stock)
                    .FirstOrDefaultAsync();

                return Json(new { cantidad });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo stock por producto");
                return Json(new { success = false, error = ex.Message });
            }
        }
        private async Task LoadBaseData(AnalisisViewModel viewModel)
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.Producto)
                .ThenInclude(pr => pr.Categoria)
                .ToListAsync();

            viewModel.VentasBolsas = pedidos
                .SelectMany(p => p.DetallePedidos)
                .Where(d => d.Producto?.Categoria?.Nombre == "Bolsas")
                .Sum(d => d.Cantidad);

            viewModel.VentasMangas = pedidos
                .SelectMany(p => p.DetallePedidos)
                .Where(d => d.Producto?.Categoria?.Nombre == "Mangas")
                .Sum(d => d.Cantidad);

            viewModel.TotalClientes = await _context.Clientes.CountAsync();
            viewModel.GananciaTotal = pedidos.Sum(p => p.Total);
            viewModel.Pedidos = pedidos;
            viewModel.Clientes = await _context.Clientes.ToListAsync();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}