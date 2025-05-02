// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GRINPLAS.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

                /// <summary>
                ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
                ///     directly from your code. This API may change or be removed in future releases.
                /// </summary>
                public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es requerido")]
            [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es requerida")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El código de verificación es requerido")]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // Decodificar el código nuevamente para asegurar integridad
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
            
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, Input.Password);
            
            if (result.Succeeded)
            {
                // Invalidar el token después de usarlo
                await _userManager.UpdateSecurityStampAsync(user);
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // Manejar errores específicos
            if (result.Errors.Any(e => e.Code == "InvalidToken"))
            {
                ModelState.AddModelError(string.Empty, "El enlace de restablecimiento ha expirado o es inválido. Por favor solicite uno nuevo.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return Page();
        }
    }
}
