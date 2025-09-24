using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class DetalleCarrito
    {

        //variables del detalle de carrito
        [Key]
        public int DetalleId { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public decimal PrecioUnitario { get; set; }
        [Required]
        public int CarritoId { get; set; }
        [ForeignKey("CarritoId")]
        public Carrito? Carrito { get; set; }
        [Required]
        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }
        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
