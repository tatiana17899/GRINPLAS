using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Carrito
    {
        [Key]
        public int CarritoId { get; set; }
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
        [Required]
        public DateTime FechaCreacion { get; set; }
        public ICollection<DetalleCarrito> detalleCarrito { get; set; } = new List<DetalleCarrito>();
        
    }
}