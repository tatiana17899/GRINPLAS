using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GRINPLAS.Controllers
{
    
    public class InicioGerenController : Controller
    {
        private readonly ILogger<InicioGerenController> _logger;

        public InicioGerenController(ILogger<InicioGerenController> logger)
        {
            _logger = logger;
        }

        public IActionResult InicioGerente()
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