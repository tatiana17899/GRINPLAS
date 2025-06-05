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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<TrabajadoresController> _logger;

        public TrabajadoresController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, // Inyectado RoleManager
            ILogger<TrabajadoresController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager; // Asignado RoleManager
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
                ApplicationUser user = null;
                string email = null;
                string password = null;

                // Solo crear usuario y credenciales si es Vendedor
                if (model.PosicionLaboral == "Vendedor")
                {
                    email = $"user{Guid.NewGuid().ToString("N")[..8]}@grinplas.com";
                    password = GenerarContrasenaValida();

                    user = new ApplicationUser
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
                }

                var trabajador = new Trabajadores
                {
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    Telefono = model.Telefono,
                    DNI = model.DNI,
                    PosicionLaboral = model.PosicionLaboral,
                    Sueldo = model.Sueldo,
                    ApplicationUserId = user?.Id // Será null si no es Vendedor
                };

                _context.Add(trabajador);
                await _context.SaveChangesAsync();

                // Solo asociar usuario si es Vendedor
                if (user != null)
                {
                    user.Trabajador = trabajador;
                    await _userManager.UpdateAsync(user);
                    await _userManager.AddToRoleAsync(user, model.PosicionLaboral);
                    
                    // Redirigir a credenciales solo para Vendedores
                    return RedirectToAction("Credenciales", new { email, password });
                }

                // Para otros roles, redirigir al listado
                return RedirectToAction(nameof(Index));
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
                    posicionLaboral = t.PosicionLaboral,
                    sueldo = t.Sueldo 
                })
                .ToListAsync();
            
            return Json(new { 
                data = trabajadores 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrabajadorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var trabajador = await _context.Trabajadores
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.IdTrabajador == model.IdTrabajador);
                
                if (trabajador == null)
                {
                    return Json(new { success = false, message = "Trabajador no encontrado" });
                }

                // Actualizar datos del trabajador
                trabajador.Nombre = model.Nombre;
                trabajador.Apellidos = model.Apellidos;
                trabajador.Telefono = model.Telefono;
                trabajador.DNI = model.DNI;
                trabajador.PosicionLaboral = model.PosicionLaboral;
                trabajador.Sueldo = model.Sueldo;

                _context.Update(trabajador);
                await _context.SaveChangesAsync();

                // Actualizar rol del usuario si es diferente
                if (trabajador.User != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(trabajador.User);
                    var rolesConFuncionalidad = new[] { "GerenteGeneral", "Vendedor" };
                    
                    // Solo manejar roles con funcionalidad específica
                    if (rolesConFuncionalidad.Contains(model.PosicionLaboral))
                    {
                        // Remover roles anteriores que tengan funcionalidad
                        var rolesToRemove = currentRoles.Where(r => rolesConFuncionalidad.Contains(r)).ToList();
                        if (rolesToRemove.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(trabajador.User, rolesToRemove);
                        }

                        // Agregar nuevo rol si existe - AQUÍ ESTÁ LA CORRECCIÓN
                        if (await _roleManager.RoleExistsAsync(model.PosicionLaboral))
                        {
                            await _userManager.AddToRoleAsync(trabajador.User, model.PosicionLaboral);
                        }
                    }
                }

                return Json(new { success = true, message = "Trabajador actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar trabajador");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var trabajador = await _context.Trabajadores
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.IdTrabajador == id);
                
                if (trabajador == null)
                {
                    return Json(new { success = false, message = "Trabajador no encontrado" });
                }

                _context.Trabajadores.Remove(trabajador);
                await _context.SaveChangesAsync();

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