using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GRINPLAS.Models;

namespace GRINPLAS.ViewModel
{
    public class ReclamacionViewModel
    {
        public List<Reclamaciones> Reclamaciones { get; set; } = new List<Reclamaciones>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}