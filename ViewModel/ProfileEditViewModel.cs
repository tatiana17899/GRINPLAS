using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.ViewModel
{
    public class ProfileEditViewModel
    {
        public string? NombreEmpresa { get; set; }
        public string? Email { get; set; }
        public string? TipDoc { get; set; }
        public string? NumDoc { get; set; }
        public string? Telefono { get; set; }
        public string? Imagen { get; set; }
    }
}