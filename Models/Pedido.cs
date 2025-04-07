using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    [Table("tb_Pedido")]
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PedidoId { get; set; }

        [ForeignKey("Cliente")]
        public long Id_Cliente { get; set; }
        public Cliente Cliente { get; set; } = default!;

        public string? Direccion { get; set; }
        public string? Img_Boleta { get; set; }
        public string? Rut_Pdf { get; set; }
        public DateTime Fec_emi { get; set; }
        public DateTime Fec_entre { get; set; }
        public string? Estado { get; set; }
        public decimal Total { get; set; }

        public ICollection<DetallePedido> DetallesPedido { get; set; }
    }
}