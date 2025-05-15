using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }  

        [Required]
        public string? Nombre { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}