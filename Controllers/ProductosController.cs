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
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly ILogger<ProductosController> _logger;

        [ActivatorUtilitiesConstructor]
        public ProductosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ProductosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public ProductosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : this(context, userManager, NullLogger<ProductosController>.Instance)
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

            if (user == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("Administrador"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            var viewModel = new ProductoViewModel
            {
                nuevoProducto = new Producto(),
                Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync(),
                Categorias = await _context.Categorias.ToListAsync(),
            };


            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> BuscarProductos(string nombreFiltro, int? categoriaFiltro)
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

            if (!userRoles.Contains("Administrador"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            var productosQuery = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrEmpty(nombreFiltro))
            {
                productosQuery = productosQuery.Where(p => p.Nombre.ToLower().Contains(nombreFiltro.ToLower()));
            }

            if (categoriaFiltro.HasValue && categoriaFiltro.Value != 0)
            {
                productosQuery = productosQuery.Where(p => p.CategoriaId == categoriaFiltro.Value);
            }

            var viewModel = new ProductoViewModel
            {
                nuevoProducto = new Producto(),
                Productos = await productosQuery.ToListAsync(),
                Categorias = await _context.Categorias.ToListAsync(),
            };

            ViewData["nombreFiltro"] = nombreFiltro;
            ViewData["categoriaFiltro"] = categoriaFiltro;

            return View("Administrador", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarProducto(Producto producto)
        {
            if (ModelState.IsValid)
            {
                if (producto.ProductoId == 0)
                {
                    // Inserción: no asignar ProductoId, que lo genere la BD
                    _context.Add(producto);
                }
                else
                {
                    // Actualización
                    _context.Update(producto);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto guardado exitosamente.";
                return RedirectToAction("Administrador");
            }

            // Si el modelo no es válido, recarga vista con datos actuales
            var viewModel = new ProductoViewModel
            {
                nuevoProducto = producto,
                Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync(),
                Categorias = await _context.Categorias.ToListAsync()
            };

            return View("Administrador", viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> EditarProducto(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction("Administrador");
            }


            return View(producto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction("Administrador");
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "El producto fue eliminado";
            return RedirectToAction("Administrador");
        }






        private IQueryable<Producto> ObtenerProductos()
        {
            return _context.Productos.Include(p => p.Categoria).AsQueryable();
        }

        public async Task<IActionResult> Cliente(int pagina = 1)
        {

            int productosPorPagina = 6;
            var productos = ObtenerProductos().Where(p => p.Stock > 0);

            var productosPaginados = await productos
                .OrderBy(p => p.ProductoId)
                .Skip((pagina - 1) * productosPorPagina)
                .Take(productosPorPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)productos.Count() / productosPorPagina);

            return View(productosPaginados);
        }


        [HttpPost]
        public async Task<IActionResult> AgregarAlCarrito(int productoId, int cantidad)
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
                TempData["ErrorMessage"] = "No se pudo agregar el producto al carrito.";
                TempData["ProductoId"] = productoId;
                return RedirectToAction("Cliente", "Productos");
            }

            var userId = cliente.ClienteId;
            var carrito = await _context.Carrito
                .Include(c => c.detalleCarrito)
                .FirstOrDefaultAsync(c => c.ClienteId == userId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    ClienteId = userId,
                    Cliente = cliente,
                    detalleCarrito = new List<DetalleCarrito>(),
                    FechaCreacion = DateTime.Now
                };
                _context.Carrito.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "El producto no existe.";
                return RedirectToAction("Cliente", "Productos");
            }

            // Verificar si ya hay unidades en el carrito
            var detalleExistente = carrito.detalleCarrito.FirstOrDefault(dc => dc.ProductoId == productoId);
            int cantidadEnCarrito = detalleExistente?.Cantidad ?? 0;

            // Verificar si hay suficiente stock
            if (producto.Stock < cantidad + cantidadEnCarrito)
            {
                if (cantidadEnCarrito > 0)
                {
                    TempData["ErrorMessage"] = $"No hay suficiente stock. Ya tienes {cantidadEnCarrito} unidades en el carrito y solo quedan {producto.Stock} unidades disponibles.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"No hay suficiente stock. Solo hay {producto.Stock} unidades disponibles.";
                }
                return RedirectToAction("Cliente", "Productos");
            }

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += cantidad;
                _context.DetalleCarrito.Update(detalleExistente);
            }
            else
            {
                var detalleCarrito = new DetalleCarrito
                {
                    CarritoId = carrito.CarritoId,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = (decimal)producto.Precio,
                    Carrito = carrito,
                    Producto = producto
                };

                _context.DetalleCarrito.Add(detalleCarrito);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Producto agregado al carrito correctamente.";

            return RedirectToAction("Cliente", "Productos");
        }
    }
}
