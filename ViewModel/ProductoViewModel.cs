using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;


namespace GRINPLAS.ViewModel
{
    public class ProductoViewModel
    {
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public Producto nuevoProducto { get; set; } = new Producto();
        public string? NombreFiltro { get; set; }
        public string? CategoriaFiltro { get; set; }
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
    }
}
