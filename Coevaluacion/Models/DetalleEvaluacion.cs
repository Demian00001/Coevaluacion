using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.Models
{
    public class DetalleEvaluacion
    {
        public int Id { get; set; }

        [Required]
        public int EvaluacionId { get; set; }
        public Evaluacion? Evaluacion { get; set; }

        [Required]
        public int CriterioId { get; set; }
        public Criterio? Criterio { get; set; }

        [Required(ErrorMessage = "La calificación es requerida.")]
        [Range(1, 10, ErrorMessage = "La calificación debe estar entre 1 y 10.")]
        public int Calificacion { get; set; }
    }
}
