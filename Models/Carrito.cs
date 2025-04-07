using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    [Table("tb_Carrito")]
    public class Carrito
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CarritoId { get; set; }

        [ForeignKey("Cliente")]
        public long Id_Cliente { get; set; }
        public Cliente Cliente { get; set; } = default!;

        public DateTime Fec_cre { get; set; }
        public ICollection<CarritoDetalle> CarritoDetalles { get; set; }
    }
}