using Microsoft.AspNetCore.Mvc;
using GRINPLAS.Models;
using GRINPLAS.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;

namespace GRINPLAS.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos el contexto para acceder a la base de datos
        public ComentariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la lista de comentarios desde la base de datos
        public async Task<IActionResult> Index()
        {
            var comentarios = await _context.Comentarios.AsNoTracking().ToListAsync();
            return View(comentarios);
        }

        // Acción para exportar la lista de comentarios a Excel
        public async Task<IActionResult> ExportarExcel()
        {
            var comentarios = await _context.Comentarios.AsNoTracking().ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Comentarios");

                // Encabezados de columnas
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Nombres";
                worksheet.Cell(1, 3).Value = "Teléfono";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Comentario";
                worksheet.Cell(1, 6).Value = "Tipo";

                // Rellenar filas con datos
                for (int i = 0; i < comentarios.Count; i++)
                {
                    var c = comentarios[i];
                    worksheet.Cell(i + 2, 1).Value = c.Id;
                    worksheet.Cell(i + 2, 2).Value = c.Nombres;
                    worksheet.Cell(i + 2, 3).Value = c.Telefono;
                    worksheet.Cell(i + 2, 4).Value = c.Email;
                    worksheet.Cell(i + 2, 5).Value = c.Contenido;
                    worksheet.Cell(i + 2, 6).Value = c.EsPositivo ? "Positivo" : "Negativo";
                }

                // Autoajustar columnas para mejor visualización
                worksheet.Columns().AdjustToContents();

                // Preparar archivo para descarga
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    string nombreArchivo = $"Comentarios-{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo
                    );
                }
            }
        }
    }
}
