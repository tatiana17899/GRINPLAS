using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GRINPLAS.Controllers
{

    public class InicioAdmiController : Controller
    {
        private readonly ILogger<InicioAdmiController> _logger;

        public InicioAdmiController(ILogger<InicioAdmiController> logger)
        {
            _logger = logger;
        }

        public IActionResult Inicio()
        {
            return View();
        }
        [HttpGet]
         public IActionResult TyC()
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