using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.ViewModels
{
    public class DashboardViewModel
    {
        // Métricas Generales
        public int TotalEquipos { get; set; }
        public int TotalIntegrantes { get; set; }
        public int TotalEvaluaciones { get; set; }
        public int TotalPeriodosActivos { get; set; }

        // Listados
        public List<TopEstudianteViewModel> TopEstudiantes { get; set; } = new List<TopEstudianteViewModel>();
        public List<UltimaEvaluacionViewModel> UltimasEvaluaciones { get; set; } = new List<UltimaEvaluacionViewModel>();

        // Datos para Gráfica (Chart.js)
        public List<string> EquiposLabels { get; set; } = new List<string>();
        public List<double> EquiposPromedios { get; set; } = new List<double>();
    }

    public class TopEstudianteViewModel
    {
        public int Posicion { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double PromedioGeneral { get; set; }
    }

    public class UltimaEvaluacionViewModel
    {
        public string EvaluadorNombre { get; set; } = string.Empty;
        public string EvaluadoNombre { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime Fecha { get; set; }
    }
}
