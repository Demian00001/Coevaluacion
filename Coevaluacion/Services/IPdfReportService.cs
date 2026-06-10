using Coevaluacion.ViewModels;

namespace Coevaluacion.Services
{
    public interface IPdfReportService
    {
        byte[] GenerarReporteIndividual(ReporteIndividualViewModel data);
        byte[] GenerarReporteEquipo(ReporteEquipoViewModel data);
        byte[] GenerarRankingGeneral(RankingGeneralViewModel data);
    }
}
