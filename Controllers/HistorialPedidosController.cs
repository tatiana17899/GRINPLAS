using GRINPLAS.Data;
using GRINPLAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GRINPLAS.Controllers
{
    [Authorize]
    public class HistorialPedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HistorialPedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
    
//SIMULACIÓN DE DATOOOOOOOOOS
 /*public async Task<IActionResult> Index()
{
    // SIMULACIÓN DE CLIENTE FICTICIO
    var clienteFicticio = new Cliente
    {
        ClienteId = 1,
        NombreEmpresa = "Juan Luis Rodriguez",
    };

    // SIMULACIÓN DE PEDIDOS
    var pedidosSimulados = new List<Pedido>
    {
        new Pedido
        {
            PedidoId = 101,
            Cliente = clienteFicticio,
            Direccion = "Av. Principal 123",
            FechaEmision = DateTime.Now,
            Total = 150.00m,
            BoletaEmitida = "/images/remisión 1.png",
            Pago = "/images/comprobante pago.png",
            FechaEntrega = DateTime.Now.AddDays(3),
            Status = "Pendiente"
        },
        new Pedido
        {
            PedidoId = 102,
            Cliente = clienteFicticio,
            Direccion = "Av. Secundaria 456",
            FechaEmision = DateTime.Now,
            Total = 200.00m,
            BoletaEmitida = "/docs/boleta2.png",
            Pago = "/docs/pago2.png",
            FechaEntrega = DateTime.Now.AddDays(7),
            Status = "Entregado"
        },
        new Pedido
        {
            PedidoId = 103,
            Cliente = clienteFicticio,
            Direccion = "Av. Fontana 789",
            FechaEmision = DateTime.Now,
            Total = 180.00m,
            BoletaEmitida = "/docs/boleta2.png",
            Pago = "/docs/pago2.png",
            FechaEntrega = DateTime.Now.AddDays(7),
            Status = "Pendiente"
        },
        new Pedido
        {
            PedidoId = 104,
            Cliente = clienteFicticio,
            Direccion = "Av. MIlitar 789",
            FechaEmision = DateTime.Now,
            Total = 210.00m,
            BoletaEmitida = "/docs/boleta2.png",
            Pago = "/docs/pago2.png",
            FechaEntrega = DateTime.Now.AddDays(7),
            Status = "Entregado"
        },
        new Pedido
       


    };

    return View(pedidosSimulados);
}   }}
*/



// PARA DATOOOS REALES 

 public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId); 

            if (cliente == null)
            {
               
                return View(new List<Pedido>());

            }


            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.ClienteId == cliente.ClienteId)
                .OrderByDescending(p => p.FechaEntrega) 
                .ToListAsync();

            return View(pedidos); 
        }

    } }