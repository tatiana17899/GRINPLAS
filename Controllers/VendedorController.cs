using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GRINPLAS.Controllers
{
    
    public class VendedorController : Controller
    {
        private readonly ILogger<VendedorController> _logger;

        public VendedorController(ILogger<VendedorController> logger)
        {
            _logger = logger;
        }

        public IActionResult InicioG()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}