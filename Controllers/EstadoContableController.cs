using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GRINPLAS.Data;
using GRINPLAS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GRINPLAS.Controllers
{
    [Authorize]
    public class EstadoContableController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 4;

        public EstadoContableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, int pageNumber = 1)
        {
            // Consulta base de gastos para la paginación
            var query = _context.Gastos.AsQueryable();

            // Calcular totales
            decimal totalIngresos;
            decimal totalGastos;

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
                ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

                // Filtrar gastos por fecha
                query = query.Where(g => g.Fecha.Date >= fechaInicio.Value.Date && 
                                        g.Fecha.Date <= fechaFin.Value.Date);

                // Calcular ingresos filtrados por fecha
                totalIngresos = await _context.Pedidos
                    .Where(p => p.FechaEmision.Date >= fechaInicio.Value.Date &&
                            p.FechaEmision.Date <= fechaFin.Value.Date)
                    .SumAsync(p => p.Total);

                // Calcular gastos filtrados por fecha
                totalGastos = await _context.Gastos
                    .Where(g => g.Fecha.Date >= fechaInicio.Value.Date &&
                            g.Fecha.Date <= fechaFin.Value.Date)
                    .SumAsync(g => g.Valor);
            }
            else
            {
                // Usar el mes actual por defecto para la visualización
                fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin = DateTime.Now;
                ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
                ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

                // Calcular todos los ingresos
                totalIngresos = await _context.Pedidos.SumAsync(p => p.Total);

                // Calcular todos los gastos
                totalGastos = await _context.Gastos.SumAsync(g => g.Valor);
            }

            // Obtener total de registros para la paginación
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Validar número de página
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

            // Obtener los gastos paginados (4 por página)
            var gastos = await query
                .OrderByDescending(g => g.Fecha)
                .Skip((pageNumber - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Configurar ViewBag para la paginación
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;
            ViewBag.TotalItems = totalItems;
            
            // Guardar parámetros de filtrado para los links de paginación
            ViewBag.FechaInicioParam = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFinParam = fechaFin?.ToString("yyyy-MM-dd");

            // Configurar totales y datos del gráfico
            ViewBag.TotalIngresos = totalIngresos.ToString("N2");
            ViewBag.TotalGastos = Math.Abs(totalGastos).ToString("N2");
            ViewBag.ChartData = new
            {
                Ingresos = totalIngresos,
                Gastos = Math.Abs(totalGastos)
            };

            return View(gastos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Fecha,Concepto,Valor")] Gasto gasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetGasto(int id)
        {
            try
            {
                var gasto = await _context.Gastos.FindAsync(id);
                if (gasto == null)
                {
                    return Json(new { success = false, message = "Gasto no encontrado" });
                }
                return Json(new { success = true, data = gasto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] Gasto gasto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Modelo inválido" });
                }

                var gastoExistente = await _context.Gastos.FindAsync(gasto.Id);
                if (gastoExistente == null)
                {
                    return Json(new { success = false, message = "Gasto no encontrado" });
                }

                gastoExistente.Fecha = gasto.Fecha;
                gastoExistente.Concepto = gasto.Concepto;
                gastoExistente.Valor = gasto.Valor;

                _context.Update(gastoExistente);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto == null)
            {
                return NotFound();
            }

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool GastoExists(int id)
        {
            return _context.Gastos.Any(e => e.Id == id);
        }
    }
}