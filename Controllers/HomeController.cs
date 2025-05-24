using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GRINPLAS.Models;
using GRINPLAS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GRINPLAS.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> PerfilCliente()
    {
        var userId = _userManager.GetUserId(User); 

        var cliente = await _context.Clientes
            .Include(c => c.User) 
            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

        if (cliente == null)
        {
            var user = await _userManager.GetUserAsync(User);
            cliente = new Cliente 
            {
                ApplicationUserId = userId,
                User = user,
                NombreEmpresa = "No especificado",
                TipDoc = "No especificado",
                NumDoc = "No especificado",
                Telefono = "No especificado"
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        return View(cliente);
    }

    public IActionResult HistorialCliente()
    {
        var clientes = _context.Clientes.Include(c => c.User).ToList();
        return View(clientes);
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Informacion()
    {
        return View();
    }
    public IActionResult Procesos()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
