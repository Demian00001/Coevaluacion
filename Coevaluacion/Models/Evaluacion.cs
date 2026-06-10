using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.Models
{
    public class Evaluacion
    {
        public int Id { get; set; }

        [Required]
        public int EvaluadorId { get; set; }
        public Integrante? Evaluador { get; set; }

        [Required]
        public int EvaluadoId { get; set; }
        public Integrante? Evaluado { get; set; }

        [Required]
        public int PeriodoId { get; set; }
        public Periodo? Periodo { get; set; }

        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string? Comentario { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public ICollection<DetalleEvaluacion> Detalles { get; set; } = new List<DetalleEvaluacion>();
    }
}
