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

        [Authorize(Roles = "Cliente")]
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

            if(!userRoles.Contains("Cliente")){
                return RedirectToPage("/Account/AccessDenied");
            }

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

            var viewModel = new CarritoViewModel
            {
                Productos = carrito.detalleCarrito.Select(dc => dc.Producto).Where(producto => producto != null).ToList()!,
                Carrito = carrito.detalleCarrito.ToList()
            };

            return View(viewModel);
        }

    }
}