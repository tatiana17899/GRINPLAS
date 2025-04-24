using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.Models
{
    public class BoletaViewModel
    {
        public Pedido Pedido { get; set; }
        public Cliente Cliente { get; set; }
        public List<DetallePedido> DetallesPedido { get; set; }
    }
}