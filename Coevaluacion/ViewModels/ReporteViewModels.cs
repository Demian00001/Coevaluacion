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

    public class SeleccionReporteEquipoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [Display(Name = "Periodo")]
        public int PeriodoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un equipo.")]
        [Display(Name = "Equipo")]
        public int EquipoId { get; set; }
    }

    public class ReporteEquipoViewModel
    {
        public string EquipoNombre { get; set; } = string.Empty;
        public string PeriodoNombre { get; set; } = string.Empty;
        public int CantidadIntegrantes { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGeneralEquipo { get; set; }
        
        public string MejorIntegranteNombre { get; set; } = string.Empty;

        public List<IntegrantePromedioViewModel> Integrantes { get; set; } = new List<IntegrantePromedioViewModel>();
    }

    public class IntegrantePromedioViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGeneral { get; set; }
    }

    public class SeleccionRankingViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un periodo.")]
        [Display(Name = "Periodo")]
        public int PeriodoId { get; set; }
    }

    public class RankingGeneralViewModel
    {
        public string PeriodoNombre { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGlobal { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double MejorPromedio { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PeorPromedio { get; set; }

        public List<EstudianteRankingViewModel> Estudiantes { get; set; } = new List<EstudianteRankingViewModel>();
    }

    public class EstudianteRankingViewModel
    {
        public int Posicion { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string EquipoNombre { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGeneral { get; set; }
    }
}
