using Microsoft.AspNetCore.Mvc;
using GRINPLAS.Models;
using GRINPLAS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GRINPLAS.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Build.Framework;

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

    public async Task<IActionResult> Admin()
    {
      var reclamaciones = await _context.Reclamaciones.OrderByDescending(r => r.FechaCreacion).ToListAsync();


      var viewModel = new ReclamacionViewModel
      {
        Reclamaciones = await _context.Reclamaciones.OrderByDescending(r => r.FechaCreacion).ToListAsync(),
      };

      return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool estado)
    {
      var reclamo = await _context.Reclamaciones.FindAsync(id);
      if (reclamo != null)
      {
        reclamo.Estado = estado;
        await _context.SaveChangesAsync();
      }
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ResponderReclamo(int id, [FromBody] string respuesta)
    {
        var reclamo = await _context.Reclamaciones.FindAsync(id);
        if (reclamo != null)
        {
            reclamo.Respuesta = respuesta;
            reclamo.Estado = true; // Marcar como atendido
            await _context.SaveChangesAsync();
            return Ok();
        }

        return NotFound();
    }
  }
}