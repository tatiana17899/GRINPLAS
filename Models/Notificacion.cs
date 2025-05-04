using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Notificacion
    {
        [Key]
        public int NotificacionId { get; set; }
        
        [Required]
        public string? UsuarioId { get; set; } 
        
        [Required]
        public string? Titulo { get; set; }
        
        [Required]
        public string? Mensaje { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public bool Leida { get; set; } = false;
        
        public string? Tipo { get; set; } 
        
        public int? PedidoId { get; set; } 
         [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }
        
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }
    }
}