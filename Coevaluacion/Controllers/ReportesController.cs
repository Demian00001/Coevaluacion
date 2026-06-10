using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.ViewModels;

namespace Coevaluacion.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Coevaluacion.Services.IPdfReportService _pdfService;

        public ReportesController(AppDbContext context, Coevaluacion.Services.IPdfReportService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Reportes
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reportes/Individual
        public async Task<IActionResult> Individual()
        {
            await PopulateDropDownsAsync();
            return View(new SeleccionReporteViewModel());
        }

        // POST: Reportes/Individual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Individual(SeleccionReporteViewModel vm)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(ResultadoIndividual), new { periodoId = vm.PeriodoId, estudianteId = vm.EstudianteId });
            }

            await PopulateDropDownsAsync(vm.PeriodoId, vm.EstudianteId);
            return View(vm);
        }

        // GET: Reportes/ResultadoIndividual
        public async Task<IActionResult> ResultadoIndividual(int periodoId, int estudianteId)
        {
            var viewModel = await BuildReporteIndividualViewModelAsync(periodoId, estudianteId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "No existen evaluaciones para este estudiante en el período seleccionado.";
                return RedirectToAction(nameof(Individual));
            }
            
            // Pasamos los IDs originales a la vista mediante ViewBag para los botones
            ViewBag.PeriodoId = periodoId;
            ViewBag.EstudianteId = estudianteId;

            return View(viewModel);
        }

        // GET: Reportes/DescargarIndividualPdf
        public async Task<IActionResult> DescargarIndividualPdf(int periodoId, int estudianteId)
        {
            var viewModel = await BuildReporteIndividualViewModelAsync(periodoId, estudianteId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "No existen datos para exportar.";
                return RedirectToAction(nameof(Individual));
            }

            var pdfBytes = _pdfService.GenerarReporteIndividual(viewModel);
            return File(pdfBytes, "application/pdf", $"ReporteIndividual_{viewModel.NombreEstudiante.Replace(" ", "")}.pdf");
        }

        private async Task<ReporteIndividualViewModel?> BuildReporteIndividualViewModelAsync(int periodoId, int estudianteId)
        {
            var periodo = await _context.Periodos.FindAsync(periodoId);
            var estudiante = await _context.Integrantes.Include(i => i.Equipo).FirstOrDefaultAsync(i => i.Id == estudianteId);

            if (periodo == null || estudiante == null)
                return null;

            var existenEvaluaciones = await _context.Evaluaciones
                .AnyAsync(e => e.EvaluadoId == estudianteId && e.PeriodoId == periodoId);

            if (!existenEvaluaciones) return null;

            var evaluaciones = await _context.Evaluaciones
                .Include(e => e.Detalles).ThenInclude(d => d.Criterio)
                .Where(e => e.EvaluadoId == estudianteId && e.PeriodoId == periodoId)
                .ToListAsync();

            var promediosPorCriterio = evaluaciones
                .SelectMany(e => e.Detalles)
                .GroupBy(d => d.Criterio!.Nombre)
                .Select(g => new PromedioCriterioViewModel
                {
                    CriterioNombre = g.Key,
                    Promedio = g.Average(d => d.Calificacion)
                })
                .OrderBy(p => p.CriterioNombre).ToList();

            double promedioGeneral = promediosPorCriterio.Any() ? promediosPorCriterio.Average(p => p.Promedio) : 0;

            var comentarios = evaluaciones
                .Where(e => !string.IsNullOrWhiteSpace(e.Comentario))
                .Select(e => e.Comentario!).ToList();

            return new ReporteIndividualViewModel
            {
                NombreEstudiante = $"{estudiante.Nombre} {estudiante.Apellido}",
                EquipoNombre = estudiante.Equipo?.Nombre ?? "Sin Equipo",
                PeriodoNombre = periodo.Nombre,
                PromediosPorCriterio = promediosPorCriterio,
                PromedioGeneral = promedioGeneral,
                Comentarios = comentarios
            };
        }

        // GET: Reportes/Equipo
        public async Task<IActionResult> Equipo()
        {
            await PopulateEquipoDropDownsAsync();
            return View(new SeleccionReporteEquipoViewModel());
        }

        // POST: Reportes/Equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Equipo(SeleccionReporteEquipoViewModel vm)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(ResultadoEquipo), new { periodoId = vm.PeriodoId, equipoId = vm.EquipoId });
            }

            await PopulateEquipoDropDownsAsync(vm.PeriodoId, vm.EquipoId);
            return View(vm);
        }

        // GET: Reportes/ResultadoEquipo
        public async Task<IActionResult> ResultadoEquipo(int periodoId, int equipoId)
        {
            var viewModel = await BuildReporteEquipoViewModelAsync(periodoId, equipoId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "No existen datos o evaluaciones para el equipo seleccionado en el período indicado.";
                return RedirectToAction(nameof(Equipo));
            }

            ViewBag.PeriodoId = periodoId;
            ViewBag.EquipoId = equipoId;

            return View(viewModel);
        }

        // GET: Reportes/DescargarEquipoPdf
        public async Task<IActionResult> DescargarEquipoPdf(int periodoId, int equipoId)
        {
            var viewModel = await BuildReporteEquipoViewModelAsync(periodoId, equipoId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "No existen datos para exportar.";
                return RedirectToAction(nameof(Equipo));
            }

            var pdfBytes = _pdfService.GenerarReporteEquipo(viewModel);
            return File(pdfBytes, "application/pdf", $"ReporteEquipo_{viewModel.EquipoNombre.Replace(" ", "")}.pdf");
        }

        private async Task<ReporteEquipoViewModel?> BuildReporteEquipoViewModelAsync(int periodoId, int equipoId)
        {
            var periodo = await _context.Periodos.FindAsync(periodoId);
            var equipo = await _context.Equipos.FindAsync(equipoId);

            if (periodo == null || equipo == null)
                return null;

            var integrantes = await _context.Integrantes.Where(i => i.EquipoId == equipoId).ToListAsync();
            if (!integrantes.Any()) return null;

            var evaluacionesEquipo = await _context.Evaluaciones
                .Include(e => e.Detalles)
                .Include(e => e.Evaluado)
                .Where(e => e.Evaluado!.EquipoId == equipoId && e.PeriodoId == periodoId)
                .ToListAsync();

            if (!evaluacionesEquipo.Any()) return null;

            var reporteVm = new ReporteEquipoViewModel
            {
                EquipoNombre = equipo.Nombre,
                PeriodoNombre = periodo.Nombre,
                CantidadIntegrantes = integrantes.Count
            };

            foreach (var integrante in integrantes)
            {
                var evaluacionesIntegrante = evaluacionesEquipo.Where(e => e.EvaluadoId == integrante.Id).ToList();
                double promedio = evaluacionesIntegrante.Any() 
                    ? evaluacionesIntegrante.SelectMany(e => e.Detalles).Average(d => d.Calificacion) 
                    : 0;

                reporteVm.Integrantes.Add(new IntegrantePromedioViewModel
                {
                    NombreCompleto = $"{integrante.Nombre} {integrante.Apellido}",
                    PromedioGeneral = promedio
                });
            }

            reporteVm.Integrantes = reporteVm.Integrantes
                .Where(i => i.PromedioGeneral > 0)
                .OrderByDescending(i => i.PromedioGeneral)
                .ThenBy(i => i.NombreCompleto)
                .ToList();

            if (reporteVm.Integrantes.Any())
            {
                reporteVm.PromedioGeneralEquipo = reporteVm.Integrantes.Average(i => i.PromedioGeneral);
                reporteVm.MejorIntegranteNombre = reporteVm.Integrantes.First().NombreCompleto;
            }

            return reporteVm;
        }

        // GET: Reportes/RankingGeneral
        public async Task<IActionResult> RankingGeneral()
        {
            var periodos = await _context.Periodos
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Nombre");
            return View(new SeleccionRankingViewModel());
        }

        // POST: Reportes/RankingGeneral
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RankingGeneral(SeleccionRankingViewModel vm)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(GenerarRankingGeneral), new { periodoId = vm.PeriodoId });
            }

            var periodos = await _context.Periodos
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Nombre", vm.PeriodoId);
            return View(vm);
        }

        // GET: Reportes/GenerarRankingGeneral
        public async Task<IActionResult> GenerarRankingGeneral(int periodoId)
        {
            var rankingVm = await BuildRankingGeneralViewModelAsync(periodoId);
            if (rankingVm == null)
            {
                TempData["ErrorMessage"] = "No existen evaluaciones para el período seleccionado.";
                return RedirectToAction(nameof(RankingGeneral));
            }

            ViewBag.PeriodoId = periodoId;

            return View(rankingVm);
        }

        // GET: Reportes/DescargarRankingPdf
        public async Task<IActionResult> DescargarRankingPdf(int periodoId)
        {
            var rankingVm = await BuildRankingGeneralViewModelAsync(periodoId);
            if (rankingVm == null)
            {
                TempData["ErrorMessage"] = "No existen datos para exportar.";
                return RedirectToAction(nameof(RankingGeneral));
            }

            var pdfBytes = _pdfService.GenerarRankingGeneral(rankingVm);
            return File(pdfBytes, "application/pdf", $"RankingGeneral_{rankingVm.PeriodoNombre.Replace(" ", "")}.pdf");
        }

        private async Task<RankingGeneralViewModel?> BuildRankingGeneralViewModelAsync(int periodoId)
        {
            var periodo = await _context.Periodos.FindAsync(periodoId);
            if (periodo == null) return null;

            var evaluaciones = await _context.Evaluaciones
                .Include(e => e.Detalles)
                .Include(e => e.Evaluado).ThenInclude(i => i!.Equipo)
                .Where(e => e.PeriodoId == periodoId)
                .ToListAsync();

            if (!evaluaciones.Any()) return null;

            var rankingVm = new RankingGeneralViewModel { PeriodoNombre = periodo.Nombre };

            var evaluacionesPorEstudiante = evaluaciones.GroupBy(e => e.EvaluadoId).ToList();

            foreach (var grupo in evaluacionesPorEstudiante)
            {
                var estudiante = grupo.First().Evaluado!;
                var equipoNombre = estudiante.Equipo?.Nombre ?? "Sin Equipo";
                var promedio = grupo.SelectMany(e => e.Detalles).Average(d => d.Calificacion);

                rankingVm.Estudiantes.Add(new EstudianteRankingViewModel
                {
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                    EquipoNombre = equipoNombre,
                    PromedioGeneral = promedio
                });
            }

            rankingVm.Estudiantes = rankingVm.Estudiantes
                .Where(e => e.PromedioGeneral > 0)
                .OrderByDescending(e => e.PromedioGeneral)
                .ThenBy(e => e.NombreCompleto)
                .ToList();

            int posicionActual = 1;
            foreach (var est in rankingVm.Estudiantes)
            {
                est.Posicion = posicionActual++;
            }

            if (rankingVm.Estudiantes.Any())
            {
                rankingVm.TotalEstudiantes = rankingVm.Estudiantes.Count;
                rankingVm.PromedioGlobal = rankingVm.Estudiantes.Average(e => e.PromedioGeneral);
                rankingVm.MejorPromedio = rankingVm.Estudiantes.First().PromedioGeneral;
                rankingVm.PeorPromedio = rankingVm.Estudiantes.Last().PromedioGeneral;
            }

            return rankingVm;
        }
        private async Task PopulateDropDownsAsync(int? periodoId = null, int? estudianteId = null)
        {
            var periodos = await _context.Periodos
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            var estudiantes = await _context.Integrantes
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .Select(i => new { i.Id, NombreCompleto = $"{i.Nombre} {i.Apellido}" })
                .ToListAsync();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Nombre", periodoId);
            ViewData["EstudianteId"] = new SelectList(estudiantes, "Id", "NombreCompleto", estudianteId);
        }

        private async Task PopulateEquipoDropDownsAsync(int? periodoId = null, int? equipoId = null)
        {
            var periodos = await _context.Periodos
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            var equipos = await _context.Equipos
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Nombre", periodoId);
            ViewData["EquipoId"] = new SelectList(equipos, "Id", "Nombre", equipoId);
        }
    }
}
