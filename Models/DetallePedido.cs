using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class DetallePedido
    {
        [Key]
        public int DetallePedidoId { get; set; }
        [Required]
        public int PedidoId { get; set; }
        [Required]
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }
        [Required]
        public int ProductoId { get; set; }
        [Required]
        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }
        public int Cantidad { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioTotal { get; set; }
        public double Medida { get; set; }
    }
}