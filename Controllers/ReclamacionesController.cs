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
      return View(new Reclamaciones());
    }

    [HttpPost]
    public IActionResult CrearReclamo(Reclamaciones model)
    {
        if (ModelState.IsValid)
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = _context.Clientes.FirstOrDefault(c => c.ApplicationUserId == userId);

            if (cliente == null)
            {
                ModelState.AddModelError("", "No se encontró el cliente.");
                return View("Cliente", model);
            }

            model.ClienteId = cliente.ClienteId;
            model.Nombre = cliente.NombreEmpresa;
            model.Correo = User.FindFirstValue(ClaimTypes.Email);
            model.Telefono = cliente.Telefono;

            _context.Reclamaciones.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Reclamo registrado correctamente";
            return RedirectToAction("Cliente");
        }

        return View("Cliente", model);
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

    public async Task<IActionResult> Admin(int page = 1, int pageSize = 4)
    {
        var totalReclamaciones = await _context.Reclamaciones.CountAsync();
        var reclamaciones = await _context.Reclamaciones
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new ReclamacionViewModel
        {
            Reclamaciones = reclamaciones,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalReclamaciones / (double)pageSize)
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