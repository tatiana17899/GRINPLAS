using System.ComponentModel.DataAnnotations;

namespace GRINPLAS.Models
{
    public class Comentarios
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar su nombre.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Debe ingresar su teléfono.")]
        [Phone(ErrorMessage = "El teléfono no es válido.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Debe ingresar su correo.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar un comentario.")]
        public string Contenido { get; set; }

        public bool EsPositivo { get; set; } = true;
    }
}
