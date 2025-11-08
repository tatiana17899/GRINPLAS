using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace GRINPLAS.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El campo Email es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El nombre de empresa o persona es requerido")]
            [StringLength(100, ErrorMessage = "El nombre de empresa o persona no puede exceder los 100 caracteres")]
            [Display(Name = "Nombre de Empresa")]
            public string NombreEmpresa { get; set; }

            [Required(ErrorMessage = "El tipo de documento es requerido")]
            [Display(Name = "Tipo de Documento")]
            public string TipDoc { get; set; }

            [Required(ErrorMessage = "El número de documento es requerido")]
            [Display(Name = "Número de Documento")]
            [CustomValidation(typeof(InputModel), "ValidateDocumentNumber")]
            public string NumDoc { get; set; }

            [Required(ErrorMessage = "El teléfono es requerido")]
            [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener exactamente 9 dígitos")]
            [Display(Name = "Teléfono")]
            public string Telefono { get; set; }
            
            [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
            [Display(Name = "Acepto los términos y condiciones")]
            public bool TerminosCondiciones { get; set; }

            [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder los 500 caracteres")]
            [Display(Name = "Comentarios")]
            public string Comentarios { get; set; }

            public static ValidationResult ValidateDocumentNumber(string numDoc, ValidationContext context)
            {
                var instance = (InputModel)context.ObjectInstance;

                if (string.IsNullOrEmpty(numDoc))
                    return new ValidationResult("El número de documento es requerido");

                if (!numDoc.All(char.IsDigit))
                    return new ValidationResult("El documento solo debe contener números");

                if (instance.TipDoc == "DNI" && numDoc.Length != 8)
                    return new ValidationResult("El DNI debe tener exactamente 8 dígitos");

                if (instance.TipDoc == "RUC" && (numDoc.Length < 11 || numDoc.Length > 20))
                    return new ValidationResult("El RUC debe tener entre 11 y 20 dígitos");

                return ValidationResult.Success;
            }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                {
                    
                    return Page();
                }    


            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Input.Email", "Este correo electrónico ya está registrado.");
            }


            // Validación adicional del documento
            if (Input.TipDoc == "DNI" && (Input.NumDoc.Length != 8 || !Input.NumDoc.All(char.IsDigit)))
            {
                ModelState.AddModelError("Input.NumDoc", "El DNI debe tener exactamente 8 dígitos numéricos");
            }
            else if (Input.TipDoc == "RUC" && (Input.NumDoc.Length < 11 || Input.NumDoc.Length > 20 || !Input.NumDoc.All(char.IsDigit)))
            {
                ModelState.AddModelError("Input.NumDoc", "El RUC debe tener entre 11 y 20 dígitos numéricos");
            }
            if (!Input.TerminosCondiciones)
            {
                ModelState.AddModelError("Input.TerminosCondiciones", "Debes aceptar los términos y condiciones.");
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.PhoneNumber = Input.Telefono;
                
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var cliente = new Cliente
                    {
                        ApplicationUserId = user.Id,
                        NombreEmpresa = Input.NombreEmpresa,
                        TipDoc = Input.TipDoc,
                        NumDoc = Input.NumDoc,
                        Telefono = Input.Telefono,
                        Comentarios = Input.Comentarios,
                    };

                    try
                    {
                        _context.Clientes.Add(cliente);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear el cliente");
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError(string.Empty, "Error al crear el cliente. Por favor, inténtelo de nuevo.");
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}