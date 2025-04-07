using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    [Table("tb_Cliente")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id_Cliente { get; set; }

        public string? Nombre { get; set; }
        public string? Tipodoc { get; set; }
        public string? NumDoc { get; set; }
        public int Telefono { get; set; }
        public string? Imagen { get; set; }

        [ForeignKey("User")]
        public string User_Id { get; set; } = default!;

        public ICollection<Carrito> Carritos { get; set; }
        public ICollection<Pedido> Pedidos { get; set; }
    }
}