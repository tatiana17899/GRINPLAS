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
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificacionesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificacionesController> logger)
        {
            _context = context;
            _userManager = userManager;
        }

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
                return Json(0);
            }
        }

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
    }
}