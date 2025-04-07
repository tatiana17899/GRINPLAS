using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    [Table("tb_DetallePedido")]
    public class DetallePedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DetalleIdpe { get; set; }

        [ForeignKey("Pedido")]
        public long PedidoId { get; set; }
        public Pedido Pedido { get; set; } = default!;

        [ForeignKey("Producto")]
        public long ProductoId { get; set; }
        public Producto Producto { get; set; } = default!;

        public int Cantidad { get; set; }
        public decimal Precio_Uni { get; set; }
    }
}