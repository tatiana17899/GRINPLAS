// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using GRINPLAS.Models;

namespace GRINPLAS.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "¿Quieres acordarte de tus credenciales?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
// Verificar primero si el usuario existe
                var user = await _userManager.FindByEmailAsync(Input.Email);
                
                if (user == null)
                {
                    // Usuario no existe
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas");
                    return Page();
                }

                // Intento de inicio de sesión
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, 
                    Input.Password, 
                    Input.RememberMe, 
                    lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario ha iniciado sesión.");
                    
                    // Obtener el usuario actual
                    if (user != null && await _userManager.IsInRoleAsync(user, "GerenteGeneral"))
                    {
                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var layoutClaim = existingClaims.FirstOrDefault(c => c.Type == "LayoutPreference");

                        if (layoutClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, layoutClaim);
                        }
                        await _userManager.AddClaimAsync(user, new Claim("LayoutPreference", "Gerente"));
                        await _signInManager.RefreshSignInAsync(user);
                        return RedirectToAction("InicioGerente", "InicioGeren");
                    }


                    if (user != null && await _userManager.IsInRoleAsync(user, "Administrador"))
                    {
                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var layoutClaim = existingClaims.FirstOrDefault(c => c.Type == "LayoutPreference");

                        if (layoutClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, layoutClaim);
                        }
                        await _userManager.AddClaimAsync(user, new Claim("LayoutPreference", "Administrador"));
                        await _signInManager.RefreshSignInAsync(user);
                        return RedirectToAction("Inicio", "InicioAdmi");
                    }


                    if (user != null && await _userManager.IsInRoleAsync(user, "Vendedor"))
                    {
                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var layoutClaim = existingClaims.FirstOrDefault(c => c.Type == "LayoutPreference");

                        if (layoutClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, layoutClaim);
                        }
                        await _userManager.AddClaimAsync(user, new Claim("LayoutPreference", "Vendedor"));
                        await _signInManager.RefreshSignInAsync(user);
                        return RedirectToAction("InicioG", "Vendedor");
                    }

                    return RedirectToAction("Cliente", "Productos");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Cuenta de usuario bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // Contraseña incorrecta
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas");
                    return Page();
                }
            }

            // Si llegamos aquí, algo falló, volver a mostrar el formulario
            return Page();
        }
    }
}