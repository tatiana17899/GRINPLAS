using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Trabajadores
    {
        [Key]
        public int IdTrabajador {get; set;}
 
        public string? ApplicationUserId { get; set; }
        
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? User { get; set; } 
        [Required]
        public string? Nombre {get; set;}
        [Required]
        public string? Apellidos {get; set;}
        [Required]
        public string? Telefono {get; set;}
        [Required]
        public string? DNI {get; set;}
        [Required]
        public string? PosicionLaboral {get; set;}
        [Required]
        public decimal? Sueldo {get; set;}
    }
}