using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }
        [Required]
        public int ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
        [Required]
        public string? Direccion { get; set; }
        [Required]
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaEntrega { get; set; }
        [Required]
        [StringLength(50)]
        public string? Status { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public string? Pago { get; set; }
        public string? BoletaEmitida { get; set; }
        public ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    }
}