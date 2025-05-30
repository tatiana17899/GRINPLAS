using Microsoft.AspNetCore.Mvc;
using GRINPLAS.Models;
using GRINPLAS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GRINPLAS.Controllers
{
  public class ReclamacionesController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ReclamacionesController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Cliente()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearReclamo(Reclamaciones reclamo)
    {
      try
      {
        if (ModelState.IsValid)
        {
          var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

          if (string.IsNullOrEmpty(userId))
          {
            TempData["Error"] = "Usuario no autenticado";
            return View("Cliente", reclamo);
          }

          var cliente = await _context.Clientes
              .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

          if (cliente == null)
          {
            TempData["Error"] = "Cliente no encontrado";
            return View("Cliente", reclamo);
          }

          reclamo.ClienteId = cliente.ClienteId;
          reclamo.Nombre = cliente.NombreEmpresa;
          reclamo.Correo = User.FindFirstValue(ClaimTypes.Email);
          reclamo.Telefono = cliente.Telefono;
          reclamo.FechaCreacion = DateTime.Now;
          reclamo.Estado = false;

          _context.Reclamaciones.Add(reclamo);
          await _context.SaveChangesAsync();

          TempData["Success"] = "Reclamo creado exitosamente";
          return View("Cliente", reclamo);
        }

        TempData["Error"] = "Por favor complete todos los campos requeridos";
        return View("Cliente", reclamo);
      }
      catch (Exception ex)
      {
        TempData["Error"] = "Error al crear el reclamo: " + ex.Message;
        return View("Cliente", reclamo);
      }
    }

    public async Task<IActionResult> HistorialCliente()
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

        return View(reclamos ?? new List<Reclamaciones>());
    }
  }
}