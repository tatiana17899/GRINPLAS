using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }
        [Required]
        public string? ApplicationUserId { get; set; }
        [Required]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? User { get; set; } 
        public string? NombreEmpresa { get; set; }
        public string? TipDoc { get; set; }
        public string? NumDoc { get; set; }
        public string? Telefono { get; set; }
        public string? Imagen {get;set;}
        public DateTime FecCre {get;set;}
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public Carrito? Carrito { get; set; }
    }
}