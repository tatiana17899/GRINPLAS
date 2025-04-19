using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GRINPLAS.ViewComponents
{
    public class ProfileViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileViewComponent(UserManager<ApplicationUser> userManager, 
                                ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return Content(string.Empty);
            
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
                
            return View(cliente ?? new Cliente {
                NombreEmpresa = "No especificado",
                TipDoc = "No especificado",
                NumDoc = "No especificado",
                Telefono = "No especificado"
            });
        }
    }
}