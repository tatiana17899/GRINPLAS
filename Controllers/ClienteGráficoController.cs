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
  
    public class ClienteGraficoController : Controller
    {
        private readonly ILogger<ClienteGraficoController> _logger;
        private readonly ApplicationDbContext _context;

        public ClienteGraficoController (
            ILogger<ClienteGraficoController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClientesPorSemana(string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out var fechaSeleccionada))
                {
                    return BadRequest(new { success = false, error = "Fecha inválida" });
                }

                fechaSeleccionada = DateTime.SpecifyKind(fechaSeleccionada, DateTimeKind.Utc);
                
                var inicioSemana = fechaSeleccionada.AddDays(-(int)fechaSeleccionada.DayOfWeek + (int)DayOfWeek.Monday);
                var finSemana = inicioSemana.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

                var clientes = await _context.Clientes
                    .Include(c => c.User)
                    .Where(c => c.User.FechaRegistro >= inicioSemana && 
                            c.User.FechaRegistro <= finSemana)
                    .ToListAsync();

                var data = new int[7];
                foreach (var cliente in clientes)
                {
                    if (cliente.User?.FechaRegistro != null)
                    {
                        var dia = (int)cliente.User.FechaRegistro.DayOfWeek;
                        // Ajuste para que Lunes sea 0
                        var index = (dia == 0) ? 6 : dia - 1;
                        data[index]++;
                    }
                }

                return Json(new
                {
                    success = true,
                    labels = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" },
                    data,
                    inicioSemana = inicioSemana.ToString("yyyy-MM-dd"),
                    finSemana = finSemana.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes por semana");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}