using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.Models
{
    public class Periodo : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida.")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha de fin no puede ser menor que la fecha de inicio.",
                    new[] { nameof(FechaFin) });
            }
        }
    }
}
