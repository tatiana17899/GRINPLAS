using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Data;
using GRINPLAS.Models;
using GRINPLAS.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GRINPLAS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, 
                            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileEditViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Modelo inválido" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                // Actualiza el email si es diferente
                if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
                {
                    user.Email = model.Email;
                    await _userManager.UpdateAsync(user);
                }

                // Obtiene o crea el cliente
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id) ?? new Cliente();

                // Actualiza los campos del cliente
                cliente.ApplicationUserId = user.Id;
                cliente.NombreEmpresa = model.NombreEmpresa ?? cliente.NombreEmpresa;
                cliente.TipDoc = model.TipDoc ?? cliente.TipDoc;
                cliente.NumDoc = model.NumDoc ?? cliente.NumDoc;
                cliente.Telefono = model.Telefono ?? cliente.Telefono;
                cliente.Imagen = model.Imagen ?? cliente.Imagen;

                if (cliente.ClienteId == 0) // Si es nuevo
                {
                    _context.Clientes.Add(cliente);
                }
                else
                {
                    _context.Clientes.Update(cliente);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            string currentPassword, 
            string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { 
                success = false, 
                message = string.Join(", ", result.Errors.Select(e => e.Description)) 
            });
        }
    }
}