using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
  public class Reclamaciones
  {
      [Key]
      public int ReclamacionId { get; set; }
      
      public string? Nombre { get; set; }
      
      public string? Correo { get; set; }
      
      public string? Telefono { get; set; }
      
      [Required]
      public string? Detalle { get; set; }
      
      public string? Respuesta { get; set; }
      
      public bool Estado { get; set; } = false; // false = no atendido, true = atendido
      
      public DateTime FechaCreacion { get; set; } = DateTime.Now;

      [ForeignKey("Cliente")]
      public int ClienteId { get; set; }
      public virtual Cliente? Cliente { get; set; }
  }
}