using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.ViewModel
{
//modales paea el carrito
    public class CarritoViewModel
    {
        public List<Producto> Productos { get; set; }
        public List<DetalleCarrito> Carrito { get; set; }
        public decimal Subtotal => Carrito?.Sum(detalle => detalle.Subtotal) ?? 0;
    }
}
