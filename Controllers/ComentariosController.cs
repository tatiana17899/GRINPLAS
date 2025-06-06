using System;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Mvc;
using ClasificacionModelo;
using System.Threading.Tasks;
using System.Linq;

namespace GRINPLAS.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComentariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comentarios/Index
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
                // Debug: imprime la etiqueta para ver qué está devolviendo
                Console.WriteLine("Etiqueta predicha: " + etiqueta);


                comentario.EsPositivo = etiqueta?.ToLower() == "positivo";

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Comentario enviado correctamente";
                return RedirectToAction("Index");
            }

            return View(comentario);
        }

    }
}
