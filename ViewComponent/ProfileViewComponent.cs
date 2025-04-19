using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GRINPLAS.Models;
using GRINPLAS.Data;
using GRINPLAS.ViewModel;
using Microsoft.EntityFrameworkCore;

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
                
            var model = new ProfileEditViewModel
            {
                Email = user.Email,
                NombreEmpresa = cliente?.NombreEmpresa,
                TipDoc = cliente?.TipDoc,
                NumDoc = cliente?.NumDoc,
                Telefono = cliente?.Telefono,
                Imagen = cliente?.Imagen
            };
            
            return View(model);
        }
    }
}