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
        public async Task<IActionResult> Index()
        {
            await PopulateDropDownsAsync();
            return View(new SeleccionReporteViewModel());
        }

        // POST: Reportes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SeleccionReporteViewModel vm)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Resultado), new { periodoId = vm.PeriodoId, estudianteId = vm.EstudianteId });
            }

            await PopulateDropDownsAsync(vm.PeriodoId, vm.EstudianteId);
            return View(vm);
        }

        // GET: Reportes/Resultado
        public async Task<IActionResult> Resultado(int periodoId, int estudianteId)
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
                return RedirectToAction(nameof(Index));
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
    }
}
