using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//detalle de variables para gastos
namespace GRINPLAS.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? Concepto { get; set; }
        public decimal Valor { get; set; }
    }
}
