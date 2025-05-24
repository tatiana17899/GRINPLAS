using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRINPLAS.Models
{
    public class NotificacionDto
    {
        public int notificacionId { get; set; }
        public string? titulo { get; set; }
        public string? mensaje { get; set; }
        public DateTime fechaCreacion { get; set; }
        public bool leida { get; set; }
        public string? tipo { get; set; }
        public int? pedidoId { get; set; }
        
        // Solo incluir ID del usuario, no el objeto completo
        public string? usuarioId { get; set; }
    }
}