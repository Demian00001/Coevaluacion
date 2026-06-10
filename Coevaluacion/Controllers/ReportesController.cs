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

        public ReportesController(AppDbContext context)
        {
            _context = context;
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
            var periodo = await _context.Periodos.FindAsync(periodoId);
            var estudiante = await _context.Integrantes.Include(i => i.Equipo).FirstOrDefaultAsync(i => i.Id == estudianteId);

            if (periodo == null || estudiante == null)
            {
                return NotFound();
            }

            // Validar si existen evaluaciones
            var existenEvaluaciones = await _context.Evaluaciones
                .AnyAsync(e => e.EvaluadoId == estudianteId && e.PeriodoId == periodoId);

            if (!existenEvaluaciones)
            {
                TempData["ErrorMessage"] = "No existen evaluaciones para este estudiante en el período seleccionado.";
                return RedirectToAction(nameof(Individual));
            }

            // Obtener todas las evaluaciones recibidas con sus detalles y criterios
            var evaluaciones = await _context.Evaluaciones
                .Include(e => e.Detalles)
                    .ThenInclude(d => d.Criterio)
                .Where(e => e.EvaluadoId == estudianteId && e.PeriodoId == periodoId)
                .ToListAsync();

            // Calcular promedios por criterio
            var promediosPorCriterio = evaluaciones
                .SelectMany(e => e.Detalles)
                .GroupBy(d => d.Criterio!.Nombre)
                .Select(g => new PromedioCriterioViewModel
                {
                    CriterioNombre = g.Key,
                    Promedio = g.Average(d => d.Calificacion)
                })
                .OrderBy(p => p.CriterioNombre)
                .ToList();

            // Calcular promedio general
            double promedioGeneral = 0;
            if (promediosPorCriterio.Any())
            {
                promedioGeneral = promediosPorCriterio.Average(p => p.Promedio);
            }

            // Extraer comentarios
            var comentarios = evaluaciones
                .Where(e => !string.IsNullOrWhiteSpace(e.Comentario))
                .Select(e => e.Comentario!)
                .ToList();

            var viewModel = new ReporteIndividualViewModel
            {
                NombreEstudiante = $"{estudiante.Nombre} {estudiante.Apellido}",
                EquipoNombre = estudiante.Equipo?.Nombre ?? "Sin Equipo",
                PeriodoNombre = periodo.Nombre,
                PromediosPorCriterio = promediosPorCriterio,
                PromedioGeneral = promedioGeneral,
                Comentarios = comentarios
            };

            return View(viewModel);
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
            var periodo = await _context.Periodos.FindAsync(periodoId);
            var equipo = await _context.Equipos.FindAsync(equipoId);

            if (periodo == null || equipo == null)
            {
                return NotFound();
            }

            // Obtener integrantes del equipo
            var integrantes = await _context.Integrantes
                .Where(i => i.EquipoId == equipoId)
                .ToListAsync();

            if (!integrantes.Any())
            {
                TempData["ErrorMessage"] = "El equipo seleccionado no tiene integrantes.";
                return RedirectToAction(nameof(Equipo));
            }

            // Obtener todas las evaluaciones del equipo en el periodo
            var evaluacionesEquipo = await _context.Evaluaciones
                .Include(e => e.Detalles)
                .Include(e => e.Evaluado)
                .Where(e => e.Evaluado!.EquipoId == equipoId && e.PeriodoId == periodoId)
                .ToListAsync();

            if (!evaluacionesEquipo.Any())
            {
                TempData["ErrorMessage"] = "No existen evaluaciones para el equipo seleccionado en el período indicado.";
                return RedirectToAction(nameof(Equipo));
            }

            var reporteVm = new ReporteEquipoViewModel
            {
                EquipoNombre = equipo.Nombre,
                PeriodoNombre = periodo.Nombre,
                CantidadIntegrantes = integrantes.Count
            };

            // Calcular promedios por integrante
            foreach (var integrante in integrantes)
            {
                var evaluacionesIntegrante = evaluacionesEquipo.Where(e => e.EvaluadoId == integrante.Id).ToList();
                double promedio = 0;

                if (evaluacionesIntegrante.Any())
                {
                    promedio = evaluacionesIntegrante
                        .SelectMany(e => e.Detalles)
                        .Average(d => d.Calificacion);
                }

                reporteVm.Integrantes.Add(new IntegrantePromedioViewModel
                {
                    NombreCompleto = $"{integrante.Nombre} {integrante.Apellido}",
                    PromedioGeneral = promedio
                });
            }

            // Ordenar de mayor a menor y filtrar los que tienen promedio > 0
            reporteVm.Integrantes = reporteVm.Integrantes
                .Where(i => i.PromedioGeneral > 0)
                .OrderByDescending(i => i.PromedioGeneral)
                .ThenBy(i => i.NombreCompleto)
                .ToList();

            // Calcular promedio general del equipo
            if (reporteVm.Integrantes.Any())
            {
                reporteVm.PromedioGeneralEquipo = reporteVm.Integrantes.Average(i => i.PromedioGeneral);
                reporteVm.MejorIntegranteNombre = reporteVm.Integrantes.First().NombreCompleto;
            }

            return View(reporteVm);
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
            var periodo = await _context.Periodos.FindAsync(periodoId);
            if (periodo == null)
            {
                return NotFound();
            }

            // Obtener todas las evaluaciones del periodo
            var evaluaciones = await _context.Evaluaciones
                .Include(e => e.Detalles)
                .Include(e => e.Evaluado)
                    .ThenInclude(i => i!.Equipo)
                .Where(e => e.PeriodoId == periodoId)
                .ToListAsync();

            if (!evaluaciones.Any())
            {
                TempData["ErrorMessage"] = "No existen evaluaciones para el período seleccionado.";
                return RedirectToAction(nameof(RankingGeneral));
            }

            var rankingVm = new RankingGeneralViewModel
            {
                PeriodoNombre = periodo.Nombre
            };

            // Agrupar evaluaciones por Evaluado (Estudiante)
            var evaluacionesPorEstudiante = evaluaciones
                .GroupBy(e => e.EvaluadoId)
                .ToList();

            foreach (var grupo in evaluacionesPorEstudiante)
            {
                var estudiante = grupo.First().Evaluado!;
                var equipoNombre = estudiante.Equipo?.Nombre ?? "Sin Equipo";

                // Promedio de todos los detalles de todas las evaluaciones que recibió
                var promedio = grupo.SelectMany(e => e.Detalles).Average(d => d.Calificacion);

                rankingVm.Estudiantes.Add(new EstudianteRankingViewModel
                {
                    NombreCompleto = $"{estudiante.Nombre} {estudiante.Apellido}",
                    EquipoNombre = equipoNombre,
                    PromedioGeneral = promedio
                });
            }

            // Ordenar de mayor a menor y asignar posiciones
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

            // Estadísticas Globales
            if (rankingVm.Estudiantes.Any())
            {
                rankingVm.TotalEstudiantes = rankingVm.Estudiantes.Count;
                rankingVm.PromedioGlobal = rankingVm.Estudiantes.Average(e => e.PromedioGeneral);
                rankingVm.MejorPromedio = rankingVm.Estudiantes.First().PromedioGeneral;
                rankingVm.PeorPromedio = rankingVm.Estudiantes.Last().PromedioGeneral;
            }

            return View(rankingVm);
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
