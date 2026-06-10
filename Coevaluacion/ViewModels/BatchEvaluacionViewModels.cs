using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.ViewModels
{
    public class SeleccionEvaluadorViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un evaluador.")]
        [Display(Name = "Evaluador")]
        public int EvaluadorId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [Display(Name = "Periodo")]
        public int PeriodoId { get; set; }
    }

    public class BatchEvaluacionViewModel
    {
        [Required]
        public int EvaluadorId { get; set; }
        public string EvaluadorNombre { get; set; } = string.Empty;

        [Required]
        public int PeriodoId { get; set; }
        public string PeriodoNombre { get; set; } = string.Empty;

        public List<CompaneroEvaluacionViewModel> Companeros { get; set; } = new List<CompaneroEvaluacionViewModel>();
    }

    public class CompaneroEvaluacionViewModel
    {
        [Required]
        public int EvaluadoId { get; set; }
        public string EvaluadoNombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string? Comentario { get; set; }

        public List<CriterioCalificacionViewModel> Criterios { get; set; } = new List<CriterioCalificacionViewModel>();
    }

    public class CriterioCalificacionViewModel
    {
        [Required]
        public int CriterioId { get; set; }
        
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La calificación es requerida.")]
        [Range(1, 10, ErrorMessage = "La calificación debe estar entre 1 y 10.")]
        public int Calificacion { get; set; }
    }
}
