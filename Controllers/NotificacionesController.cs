
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
                    tipo = n.Tipo,
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
    }
}

