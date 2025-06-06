using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GRINPLAS.Models
{
    public class ApplicationUser: IdentityUser
    {
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public Cliente? Cliente { get; set;}
        public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public Trabajadores? Trabajador { get; set; }
    }
}