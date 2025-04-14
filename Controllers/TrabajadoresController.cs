using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GRINPLAS.Data;
using GRINPLAS.Models;
using GRINPLAS.ViewModel;
using System.Text.RegularExpressions;

namespace GRINPLAS.Controllers
{
    [Authorize(Roles = "GerenteGeneral")]
    public class TrabajadoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TrabajadoresController> _logger;

        public TrabajadoresController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TrabajadoresController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Trabajadores.ToListAsync());
        }

        public IActionResult Create() => View(new TrabajadorViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrabajadorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var email = $"user{Guid.NewGuid().ToString("N")[..8]}@grinplas.com";
                var password = GenerarContrasenaValida();

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(e => ModelState.AddModelError("", e.Description));
                    return View(model);
                }

                var trabajador = new Trabajadores
                {
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    Telefono = model.Telefono,
                    DNI = model.DNI,
                    PosicionLaboral = model.PosicionLaboral,
                    ApplicationUserId = user.Id
                };

                _context.Add(trabajador);
                await _context.SaveChangesAsync(); // Guarda primero para obtener el IdTrabajador

                // Establece la relación inversa
                user.Trabajador = trabajador;
                await _userManager.UpdateAsync(user);

                await _userManager.AddToRoleAsync(user, model.PosicionLaboral);

                return RedirectToAction("Credenciales", new { email, password });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear trabajador");
                ModelState.AddModelError("", "Error al crear el trabajador");
                return View(model);
            }
        }

        public IActionResult Credenciales(string email, string password)
        {
            ViewBag.Email = email;
            ViewBag.Password = password;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetTrabajadores()
        {
            var trabajadores = await _context.Trabajadores
                .Select(t => new {
                    idTrabajador = t.IdTrabajador,
                    nombre = t.Nombre,
                    apellidos = t.Apellidos,
                    telefono = t.Telefono,
                    dni = t.DNI,
                    posicionLaboral = t.PosicionLaboral
                })
                .ToListAsync();
            
            return Json(new { data = trabajadores });
        }
        private string GenerarContrasenaValida()
        {
            var random = new Random();
            var password = new[]
            {
                (char)random.Next(65, 90),  
                (char)random.Next(97, 122), 
                (char)random.Next(48, 57),  
                (char)random.Next(33, 47), 
            }.Concat(Enumerable.Repeat(0, 4).Select(_ => (char)random.Next(97, 122)))
             .OrderBy(c => random.Next())
             .ToArray();

            return new string(password);
        }
        //FALTA EDITAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Incluir el usuario relacionado para evitar problemas de carga
                var trabajador = await _context.Trabajadores
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.IdTrabajador == id);
                
                if (trabajador == null)
                {
                    return Json(new { success = false, message = "Trabajador no encontrado" });
                }

                // Eliminar primero el trabajador para evitar problemas de referencia
                _context.Trabajadores.Remove(trabajador);
                await _context.SaveChangesAsync();

                // Luego eliminar el usuario si existe
                if (trabajador.User != null)
                {
                    await _userManager.DeleteAsync(trabajador.User);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar trabajador");
                return Json(new { success = false, message = ex.Message });
            }
        }
        private bool TrabajadorExists(int id)
        {
            return _context.Trabajadores.Any(e => e.IdTrabajador == id);
        }
    }
}