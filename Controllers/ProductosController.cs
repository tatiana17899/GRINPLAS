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

            if(user == null){
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles= await _userManager.GetRolesAsync(user);

            if(!userRoles.Contains("Administrador")){
                return RedirectToPage("/Account/AccessDenied");
            }

            var viewModel = new ProductoViewModel{
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

            // Filtrar productos por nombre y categoría
            var productosQuery = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrEmpty(nombreFiltro))
            {
                productosQuery = productosQuery.Where(p => p.Nombre.ToLower().Contains(nombreFiltro.ToLower()));
            }

            if (categoriaFiltro.HasValue && categoriaFiltro.Value != 0) // 0 representa "Todas las categorías"
            {
                productosQuery = productosQuery.Where(p => p.CategoriaId == categoriaFiltro.Value);
            }

            var viewModel = new ProductoViewModel
            {
                nuevoProducto = new Producto(),
                Productos = await productosQuery.ToListAsync(),
                Categorias = await _context.Categorias.ToListAsync(),
            };

            ViewData["nombreFiltro"] = nombreFiltro; // Para mantener el valor en el input
            ViewData["categoriaFiltro"] = categoriaFiltro; // Para mantener el valor en el select

            return View("Administrador", viewModel);
        }
    }
}
