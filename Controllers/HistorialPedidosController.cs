using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


//funcionalidad fecha de entrega y validaciones
namespace GRINPLAS.Controllers
{
    [Authorize]
    public class HistorialPedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HistorialPedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
                public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId); 

            if (cliente == null)
            {
               
                return View(new List<Pedido>());

            }

            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.ClienteId == cliente.ClienteId)
                .OrderByDescending(p => p.FechaEntrega) 
                .ToListAsync();

            return View(pedidos); 
        }

    }
}
