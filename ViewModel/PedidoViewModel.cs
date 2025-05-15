using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.ViewModel
{
    public class PedidoViewModel
    {
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public Pedido nuevoPedido { get; set; } = new Pedido();
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public decimal TotalMangas { get; set; }
        public decimal TotalBolsas { get; set; }
        public string? FechaInicioFiltro { get; set; }
        public string? FechaFinFiltro { get; set; }
    }
}