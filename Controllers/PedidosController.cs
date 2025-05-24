using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Identity;
using GRINPLAS.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Build.Framework;

namespace GRINPLAS.Controllers
{
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly ILogger<PedidosController> _logger;

        [ActivatorUtilitiesConstructor]
        public PedidosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<PedidosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public PedidosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : this(context, userManager, NullLogger<PedidosController>.Instance)
        {
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Administrador(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (_userManager == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("Administrador"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var viewModel = new PedidoViewModel
            {
                Pedidos = await _context.Pedidos.Include(p => p.Cliente).ToListAsync(),
                nuevoPedido = new Pedido(),
                Clientes = await _context.Clientes.ToListAsync(),
                Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync()
            };
            ViewBag.TotalMangas = await _context.DetallePedidos
               .Where(dp => dp.Producto.Categoria.Nombre == "Mangas")
               .SumAsync(dp => dp.PrecioTotal);

            ViewBag.TotalBolsas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Bolsas")
                .SumAsync(dp => dp.PrecioTotal);


            return View(viewModel);
        }
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> GerenteGeneral(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (_userManager == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("Vendedor"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }
            var viewModel = new PedidoViewModel
            {
                Pedidos = await _context.Pedidos.Include(p => p.Cliente).ToListAsync(),
                nuevoPedido = new Pedido(),
                Clientes = await _context.Clientes.ToListAsync(),
                Productos = await _context.Productos.Include(p => p.Categoria).ToListAsync()
            };
            ViewBag.TotalMangas = await _context.DetallePedidos
               .Where(dp => dp.Producto.Categoria.Nombre == "Mangas")
               .SumAsync(dp => dp.PrecioTotal);

            ViewBag.TotalBolsas = await _context.DetallePedidos
                .Where(dp => dp.Producto.Categoria.Nombre == "Bolsas")
                .SumAsync(dp => dp.PrecioTotal);


            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> ActualizarPedido([FromForm] int pedidoId, [FromForm] string status, [FromForm] string pago, [FromForm] DateTime? fechaEntrega)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                if (pedido.Status != status)
                {
                    await CrearNotificacion(
                        pedido.Cliente.User.Id,
                        "Estado del pedido",
                        $"Tu pedido está en \"{status}\"",
                        "estado",
                        pedido.PedidoId
                    );
                }
                if (pago == "aprobado" && pedido.Pago != "aprobado")
                {
                    await CrearNotificacion(
                        pedido.Cliente.User.Id,
                        "Comprobante de pago",
                        "Tu comprobante de pago fue aprobado",
                        "pago",
                        pedido.PedidoId
                    );
                }
                else if (pago == "desaprobado" && pedido.Pago != "desaprobado")
                {
                    await CrearNotificacion(
                        pedido.Cliente.User.Id,
                        "Comprobante de pago",
                        "Tu comprobante de pago fue rechazado",
                        "pago",
                        pedido.PedidoId
                    );
                }
                else if (pago == "Pendiente" && pedido.Pago != "Pendiente")
                {
                    await CrearNotificacion(
                        pedido.Cliente.User.Id,
                        "Comprobante de pago",
                        "Tu comprobante de pago está pendiente",
                        "pago",
                        pedido.PedidoId
                    );
                }

                pedido.Status = status;
                pedido.Pago = pago;

                if (fechaEntrega.HasValue)
                {
                    pedido.FechaEntrega = DateTime.SpecifyKind(fechaEntrega.Value, DateTimeKind.Utc);
                }
                else
                {
                    pedido.FechaEntrega = null;
                }

                _context.Update(pedido);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDireccion([FromForm] int pedidoId, [FromForm] string nuevaDireccion)
        {
            try
            {
                // Busca el pedido en la base de datos
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                // Actualiza la dirección
                pedido.Direccion = nuevaDireccion;
                _context.Update(pedido);
                await _context.SaveChangesAsync();
                await CrearNotificacion(
                    pedido.Cliente.User.Id,
                    "Dirección actualizada",
                    "La dirección de entrega de tu pedido ha sido actualizada",
                    "estado",
                    pedido.PedidoId
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la dirección del pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }

        //cancelar pedido
        [HttpPost]
        public async Task<IActionResult> CancelarPedido([FromForm] int pedidoId)
        {
            try
            {
                // Busca el pedido en la base de datos
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

                if (pedido == null)
                {
                    return Json(new { success = false, error = $"Pedido no encontrado (ID: {pedidoId})" });
                }

                // Cambia el estado del pedido a "Cancelado"
                pedido.Status = "cancelado";
                _context.Update(pedido);
                await _context.SaveChangesAsync();

                await CrearNotificacion(
                    pedido.Cliente.User.Id,
                    "Pedido cancelado",
                    "Tu pedido ha sido cancelado",
                    "estado",
                    pedido.PedidoId
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar el pedido");
                return Json(new { success = false, error = "Error interno del servidor" });
            }
        }
        private async Task CrearNotificacion(string usuarioId, string titulo, string mensaje, string tipo, int? pedidoId = null)
        {
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
        }
        [NonAction]
        public async Task<List<Notificacion>> ObtenerNotificacionesUsuario()
        {
            if (_userManager == null) return new List<Notificacion>();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new List<Notificacion>();

            return await _context.Notificaciones
                .Where(n => n.UsuarioId == user.Id && !n.Leida)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(5)
                .ToListAsync();
        }
        [HttpPost]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> CrearNotificacionNuevoPedido([FromBody] NotificacionDto notificacionDto)
        {
            try
            {
                // Obtener todos los usuarios vendedores
                var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");

                foreach (var vendedor in vendedores)
                {
                    var notificacion = new Notificacion
                    {
                        UsuarioId = vendedor.Id,
                        Titulo = notificacionDto.titulo ?? "Nuevo pedido",
                        Mensaje = notificacionDto.mensaje ?? "Un cliente ha realizado un nuevo pedido",
                        Tipo = "nuevo_pedido",
                        PedidoId = notificacionDto.pedidoId,
                        FechaCreacion = DateTime.UtcNow,
                        Leida = false
                    };

                    _context.Notificaciones.Add(notificacion);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación de nuevo pedido");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> VerificarNuevosPedidos()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                // Aumentamos el rango de tiempo a 24 horas para pruebas
                var ultimaVerificacion = DateTime.UtcNow.AddHours(-24);
                
                var nuevosPedidos = await _context.Pedidos
                    .Where(p => p.FechaEmision >= ultimaVerificacion && p.Status == "Pendiente")
                    .Include(p => p.Cliente)
                    .ToListAsync();

                // Obtener todos los vendedores
                var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");

                foreach (var pedido in nuevosPedidos)
                {
                    foreach (var vendedor in vendedores)
                    {
                        var notificacionExistente = await _context.Notificaciones
                            .FirstOrDefaultAsync(n => n.PedidoId == pedido.PedidoId && 
                                                n.UsuarioId == vendedor.Id && 
                                                n.Tipo == "nuevo_pedido");

                        if (notificacionExistente == null)
                        {
                            var notificacion = new Notificacion
                            {
                                UsuarioId = vendedor.Id,
                                Titulo = "Nuevo pedido recibido",
                                Mensaje = $"El cliente {pedido.Cliente.NombreEmpresa ?? "Cliente"} ha realizado un nuevo pedido (ID: {pedido.PedidoId})",
                                Tipo = "nuevo_pedido",
                                PedidoId = pedido.PedidoId,
                                FechaCreacion = DateTime.UtcNow,
                                Leida = false
                            };
                            var notificacionCliente = new Notificacion
                            {
                                UsuarioId = user.Id,
                                Titulo = "Confirmación de pedido",
                                Mensaje = $"Tu pedido #{pedido.PedidoId} se ha registrado correctamente",
                                Tipo = "confirmacion_pedido",
                                PedidoId = pedido.PedidoId,
                                FechaCreacion = DateTime.UtcNow,
                                Leida = false
                            };
                            _context.Notificaciones.Add(notificacion);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    data = nuevosPedidos.Select(p => new {
                        pedidoId = p.PedidoId,
                        clienteNombre = p.Cliente.NombreEmpresa ?? "Cliente",
                        fechaCreacion = p.FechaEmision.ToString("g")
                    }) 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar nuevos pedidos");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CrearPedido([FromBody] PedidoDto pedidoDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
                if (cliente == null) return BadRequest("Cliente no encontrado");

                // 1. Crear y guardar el pedido
                var pedido = new Pedido
                {
                    ClienteId = cliente.ClienteId,
                    FechaEmision = DateTime.UtcNow,
                    Status = "Pendiente",
                    Pago = "Pendiente",
                    Direccion = pedidoDto.Direccion,
                    Total = pedidoDto.Total
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync(); 

                // 2. Crear notificación para el cliente
                var notificacionCliente = new Notificacion
                {
                    UsuarioId = user.Id,
                    Titulo = "Confirmación de pedido",
                    Mensaje = $"Tu pedido #{pedido.PedidoId} se ha registrado correctamente",
                    Tipo = "confirmacion_pedido",
                    PedidoId = pedido.PedidoId,
                    FechaCreacion = DateTime.UtcNow,
                    Leida = false
                };
                _context.Notificaciones.Add(notificacionCliente);
                await _context.SaveChangesAsync();
                // 4. Confirmar transacción
                await transaction.CommitAsync();

                // Log para depuración
                _logger.LogInformation($"Notificación de confirmación creada con ID: {notificacionCliente.NotificacionId}");

                return Json(new { 
                    success = true, 
                    pedidoId = pedido.PedidoId,
                    notificacionId = notificacionCliente.NotificacionId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear pedido y notificaciones");
                return Json(new { 
                    success = false, 
                    error = "Error al crear el pedido",
                    detail = ex.Message 
                });
            }
        }
        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CrearNotificacionConfirmacionPedido([FromBody] NotificacionDto notificacionDto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

                var notificacion = new Notificacion
                {
                    UsuarioId = user.Id, // Notificación para el usuario actual (cliente)
                    Titulo = notificacionDto.titulo ?? "Confirmación de pedido",
                    Mensaje = notificacionDto.mensaje ?? "Tu pedido se ha registrado correctamente",
                    Tipo = "confirmacion_pedido", // Tipo específico para cliente
                    PedidoId = notificacionDto.pedidoId,
                    FechaCreacion = DateTime.UtcNow,
                    Leida = false
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                return Json(new { success = true, notificacionId = notificacion.NotificacionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación de confirmación");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}