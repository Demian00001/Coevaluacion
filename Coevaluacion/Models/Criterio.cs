using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.Models
{
    public class Criterio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public required string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
