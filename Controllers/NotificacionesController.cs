using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Controllers
{
    // Controlador protegido: solo usuarios autenticados pueden acceder
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor: se inyecta el contexto de BD y el manejador de usuarios
        public NotificacionesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificacionesController> logger)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene las últimas 10 notificaciones del usuario autenticado
        /// </summary>
        /// 
        [HttpGet]
        public async Task<IActionResult> ObtenerNotificaciones()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Usuario no autenticado" });

            var notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == user.Id)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(10)
                .Select(n => new
                {
                    notificacionId = n.NotificacionId,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    fechaCreacion = n.FechaCreacion,
                    tipo = n.Tipo ,
                    pedidoId = n.PedidoId,
                    leida = n.Leida,
                    usuarioId = n.UsuarioId
                })
                .ToListAsync();

            return Json(new { success = true, data = notificaciones });
        }
        /// <summary>
        /// Devuelve el número de notificaciones no leídas del usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ContadorNoLeidas()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(0);

                var count = await _context.Notificaciones
                    .Where(n => n.UsuarioId == user.Id && !n.Leida)
                    .CountAsync();

                return Json(count);
            }
            catch (Exception ex)
            {
                // Si ocurre un error, devuelve 0 por defecto
                return Json(0);
            }
        }
         /// <summary>
        /// Marca una notificación específica como leída
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarcarComoLeida([FromForm] int id)
        {
            try
            {

                var notificacion = await _context.Notificaciones.FindAsync(id);
                if (notificacion == null)
                {
                    return NotFound(new { success = false, message = $"Notificación no encontrada: {id}" });
                }

                // Verificar que el usuario actual sea el dueño de la notificación
                var user = await _userManager.GetUserAsync(User);
                if (user == null || notificacion.UsuarioId != user.Id)
                {
                    return Unauthorized(new { success = false, message = "No autorizado" });
                }

                notificacion.Leida = true;
                _context.Update(notificacion);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
            }
        }
         /// <summary>
        /// Marca todas las notificaciones del usuario como leídas
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var notificaciones = await _context.Notificaciones
                    .Where(n => n.UsuarioId == user.Id && !n.Leida)
                    .ToListAsync();

                foreach (var notificacion in notificaciones)
                {
                    notificacion.Leida = true;
                    _context.Update(notificacion);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Todas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

            try
            {
                var notificaciones = await _context.Notificaciones
                    .Where(n => n.UsuarioId == user.Id)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Select(n => new
                    {
                        notificacionId = n.NotificacionId,
                        titulo = n.Titulo ?? "Sin título",
                        mensaje = n.Mensaje ?? "Sin mensaje",
                        fechaCreacion = n.FechaCreacion,
                        tipo = n.Tipo ?? "general",
                        pedidoId = n.PedidoId,
                        leida = n.Leida
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = notificaciones,
                    count = notificaciones.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerNotificacionesNoLeidas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Usuario no autenticado" });

            var notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == user.Id && !n.Leida)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(10)
                .Select(n => new 
                {
                    notificacionId = n.NotificacionId,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    fechaCreacion = n.FechaCreacion,
                    tipo = (n.Tipo ?? "general").ToLower(),
                    pedidoId = n.PedidoId,
                    leida = n.Leida,
                    usuarioId = n.UsuarioId
                })
                .ToListAsync();

            return Json(new { success = true, data = notificaciones });
        }
        public async Task<IActionResult> CrearNotificacion(string usuarioId, string titulo, string mensaje, string tipo, int? pedidoId = null)
    {
        try
        {
            // Verificar que el usuario existe
            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
            {
                
                return NotFound("Usuario no encontrado");
            }

            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                PedidoId = pedidoId,
                FechaCreacion = DateTime.UtcNow,
                Leida = false
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            
            return Ok(new { success = true, notificacionId = notificacion.NotificacionId });
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, "Error interno al crear notificación");
        }
    }
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GenerarNotificacionesStockBajo()
        {
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

            
            var productosBajoStock = await _context.Productos
                .Where(p => p.Stock <= 5)
                .ToListAsync();

            int creadas = 0;
            foreach (var producto in productosBajoStock)
            {
                
                var notificacionesAnteriores = await _context.Notificaciones
                    .Where(n => n.UsuarioId == user.Id
                        && n.Tipo == "bajo_stock"
                        && n.Mensaje.Contains(producto.Nombre))
                    .ToListAsync();

                if (notificacionesAnteriores.Any())
                {
                    _context.Notificaciones.RemoveRange(notificacionesAnteriores);
                }

                
                var notificacion = new Notificacion
                {
                    UsuarioId = user.Id,
                    Titulo = "Alerta",
                    Mensaje = $"Solo queda {producto.Stock} unidad{(producto.Stock == 1 ? "" : "es")} del producto \"{producto.Nombre}\".",
                    Tipo = "bajo_stock",
                    PedidoId = null,
                    FechaCreacion = DateTime.UtcNow,
                    Leida = false
                };
                _context.Notificaciones.Add(notificacion);
                creadas++;
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, creadas });
        }
    }
}