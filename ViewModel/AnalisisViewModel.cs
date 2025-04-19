using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.ViewModel
{
    public class AnalisisViewModel
    {
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public int TotalClientes { get; set; }
        public decimal GananciaTotal { get; set; }
        public int VentasBolsas { get; set; }
        public int VentasMangas { get; set; }
    }
}