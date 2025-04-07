using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    [Table("tb_Producto")]
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProductoId { get; set; }

        [ForeignKey("Categoria")]
        public long CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = default!;

        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Imagen { get; set; }
        public decimal Precio_Uni { get; set; }
        public int Stock { get; set; }

        public ICollection<CarritoDetalle> CarritoDetalles { get; set; }
        public ICollection<DetallePedido> DetallesPedido { get; set; }
    }
}