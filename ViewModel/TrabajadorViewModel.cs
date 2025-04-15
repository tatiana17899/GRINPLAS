using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

using System.ComponentModel.DataAnnotations;

namespace GRINPLAS.ViewModel
{
    public class TrabajadorViewModel
{
    public int IdTrabajador { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
    public string Apellidos { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    public string Telefono { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 caracteres")]
    public string DNI { get; set; }

    [Required(ErrorMessage = "La posición laboral es obligatoria")]
    public string PosicionLaboral { get; set; }
}
}