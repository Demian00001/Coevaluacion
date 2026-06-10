using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.ViewModels
{
    public class SeleccionReporteViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [Display(Name = "Periodo")]
        public int PeriodoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Display(Name = "Estudiante")]
        public int EstudianteId { get; set; }
    }

    public class ReporteIndividualViewModel
    {
        public string NombreEstudiante { get; set; } = string.Empty;
        public string EquipoNombre { get; set; } = string.Empty;
        public string PeriodoNombre { get; set; } = string.Empty;

        public List<PromedioCriterioViewModel> PromediosPorCriterio { get; set; } = new List<PromedioCriterioViewModel>();
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGeneral { get; set; }

        public List<string> Comentarios { get; set; } = new List<string>();
    }

    public class PromedioCriterioViewModel
    {
        public string CriterioNombre { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Promedio { get; set; }
    }
}
