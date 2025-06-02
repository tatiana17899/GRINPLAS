using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using GRINPLAS.Data;
using GRINPLAS.Models;

namespace GRINPLAS.Controllers
{
    public class HistorialReclamosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialReclamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
            {
                return NotFound();
            }

            var reclamos = await _context.Reclamaciones
                .Where(r => r.ClienteId == cliente.ClienteId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View("~/Views/Reclamaciones/HistorialReclamosCliente.cshtml", reclamos ?? new List<Reclamaciones>());
        }
    }
}