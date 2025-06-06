using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GRINPLAS.Data;
using GRINPLAS.Models;
using ClasificacionModelo;
using ClosedXML.Excel;

namespace GRINPLAS.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComentariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var comentarios = _context.Comentarios.ToList();
            return View(comentarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarComentario(Comentarios comentario)
        {
            if (ModelState.IsValid)
            {
                var input = new MLModelTextClasification.ModelInput
                {
                    Comment = comentario.Contenido
                };

                var resultado = MLModelTextClasification.Predict(input);
                var etiqueta = resultado.PredictedLabel;

                comentario.EsPositivo = etiqueta?.ToLower() == "positivo";

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Comentario enviado correctamente";
                return RedirectToAction("Index");
            }

            return View(comentario);
        }

        [HttpGet]
        public IActionResult ExportarExcel()
        {
            var comentarios = _context.Comentarios.ToList();

            if (!comentarios.Any())
            {
                return BadRequest("No hay comentarios para exportar.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Comentarios");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Nombres";
            worksheet.Cell(1, 3).Value = "Teléfono";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Comentario";
            worksheet.Cell(1, 6).Value = "Tipo";

            int fila = 2;
            foreach (var c in comentarios)
            {
                worksheet.Cell(fila, 1).Value = c.Id;
                worksheet.Cell(fila, 2).Value = c.Nombres;
                worksheet.Cell(fila, 3).Value = c.Telefono;
                worksheet.Cell(fila, 4).Value = c.Email;
                worksheet.Cell(fila, 5).Value = c.Contenido;
                worksheet.Cell(fila, 6).Value = c.EsPositivo ? "Positivo" : "Negativo";
                fila++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Comentarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }




    }
}
