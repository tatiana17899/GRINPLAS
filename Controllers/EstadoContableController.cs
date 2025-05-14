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

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            // Calcular el total de ingresos sumando todos los pedidos
            decimal totalIngresos = await _context.Pedidos.SumAsync(p => p.Total);
            ViewBag.TotalIngresos = totalIngresos.ToString("N2"); // Formato con separador de miles

            // Calcular el total de gastos
            decimal totalGastos = await _context.Gastos.SumAsync(g => g.Valor);
            ViewBag.TotalGastos = totalGastos.ToString("N2");
            
            var gastos = await _context.Gastos
            .OrderByDescending(g => g.Fecha)
            .Skip((pageNumber - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();

            var cantidadGastos = await _context.Gastos.CountAsync();
            var totalPages = (int)Math.Ceiling(cantidadGastos / (double)_pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            // Datos para la gráfica
            ViewBag.ChartData = new
            {
                Ingresos = totalIngresos,
                Gastos = totalGastos
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