using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.ViewModels
{
    public class EvaluacionViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un evaluador.")]
        [Display(Name = "Evaluador")]
        public int EvaluadorId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [Display(Name = "Periodo")]
        public int PeriodoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar a quién va a evaluar.")]
        [Display(Name = "Evaluado")]
        public int EvaluadoId { get; set; }

        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string? Comentario { get; set; }

        public List<CriterioEvaluacionViewModel> Criterios { get; set; } = new List<CriterioEvaluacionViewModel>();
    }

    public class CriterioEvaluacionViewModel
    {
        public int CriterioId { get; set; }
        
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La calificación es requerida.")]
        [Range(1, 10, ErrorMessage = "La calificación debe estar entre 1 y 10.")]
        public int Calificacion { get; set; }
    }
}
