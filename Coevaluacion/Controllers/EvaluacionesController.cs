using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.Models;
using Coevaluacion.ViewModels;

namespace Coevaluacion.Controllers
{
    public class EvaluacionesController : Controller
    {
        private readonly AppDbContext _context;

        public EvaluacionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Evaluaciones
        public async Task<IActionResult> Index()
        {
            var evaluaciones = await _context.Evaluaciones
                .Include(e => e.Evaluador)
                .Include(e => e.Evaluado)
                .Include(e => e.Periodo)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            return View(evaluaciones);
        }

        // GET: Evaluaciones/Seleccionar
        public async Task<IActionResult> Seleccionar()
        {
            await PopulateDropDownsAsync();
            return View(new SeleccionEvaluadorViewModel());
        }

        // POST: Evaluaciones/Seleccionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Seleccionar(SeleccionEvaluadorViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // Validar que el evaluador no haya evaluado ya en este período
                bool yaEvaluo = await _context.Evaluaciones
                    .AnyAsync(e => e.EvaluadorId == vm.EvaluadorId && e.PeriodoId == vm.PeriodoId);

                if (yaEvaluo)
                {
                    TempData["ErrorMessage"] = "Ya completaste tus evaluaciones para este período.";
                    await PopulateDropDownsAsync(vm.EvaluadorId, vm.PeriodoId);
                    return View(vm);
                }

                // Si todo está bien, redirigir al paso 2
                return RedirectToAction(nameof(Capturar), new { evaluadorId = vm.EvaluadorId, periodoId = vm.PeriodoId });
            }

            await PopulateDropDownsAsync(vm.EvaluadorId, vm.PeriodoId);
            return View(vm);
        }

        // GET: Evaluaciones/Capturar
        public async Task<IActionResult> Capturar(int evaluadorId, int periodoId)
        {
            // Re-verificar que no haya evaluado ya (por si entra por URL)
            bool yaEvaluo = await _context.Evaluaciones
                .AnyAsync(e => e.EvaluadorId == evaluadorId && e.PeriodoId == periodoId);

            if (yaEvaluo)
            {
                TempData["ErrorMessage"] = "Ya completaste tus evaluaciones para este período.";
                return RedirectToAction(nameof(Seleccionar));
            }

            var evaluador = await _context.Integrantes.FindAsync(evaluadorId);
            var periodo = await _context.Periodos.FindAsync(periodoId);

            if (evaluador == null || periodo == null)
            {
                return NotFound();
            }

            // Obtener compañeros de equipo excluyendo al evaluador
            var companeros = await _context.Integrantes
                .Where(i => i.EquipoId == evaluador.EquipoId && i.Id != evaluadorId)
                .OrderBy(i => i.Apellido).ThenBy(i => i.Nombre)
                .ToListAsync();

            if (!companeros.Any())
            {
                TempData["ErrorMessage"] = "No hay compañeros de equipo para evaluar.";
                return RedirectToAction(nameof(Seleccionar));
            }

            // Obtener criterios activos
            var criteriosActivos = await _context.Criterios
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            if (!criteriosActivos.Any())
            {
                TempData["ErrorMessage"] = "No hay criterios de evaluación activos en el sistema.";
                return RedirectToAction(nameof(Seleccionar));
            }

            var vm = new BatchEvaluacionViewModel
            {
                EvaluadorId = evaluador.Id,
                EvaluadorNombre = $"{evaluador.Nombre} {evaluador.Apellido}",
                PeriodoId = periodo.Id,
                PeriodoNombre = periodo.Nombre
            };

            foreach (var comp in companeros)
            {
                var compVm = new CompaneroEvaluacionViewModel
                {
                    EvaluadoId = comp.Id,
                    EvaluadoNombre = $"{comp.Nombre} {comp.Apellido}"
                };

                foreach (var crit in criteriosActivos)
                {
                    compVm.Criterios.Add(new CriterioCalificacionViewModel
                    {
                        CriterioId = crit.Id,
                        Nombre = crit.Nombre,
                        Descripcion = crit.Descripcion
                    });
                }
                
                vm.Companeros.Add(compVm);
            }

            return View(vm);
        }

        // POST: Evaluaciones/Capturar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capturar(BatchEvaluacionViewModel vm)
        {
            if (ModelState.IsValid)
            {
                // Doble chequeo de seguridad
                bool yaEvaluo = await _context.Evaluaciones
                    .AnyAsync(e => e.EvaluadorId == vm.EvaluadorId && e.PeriodoId == vm.PeriodoId);

                if (yaEvaluo)
                {
                    TempData["ErrorMessage"] = "Ya completaste tus evaluaciones para este período.";
                    return RedirectToAction(nameof(Index));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var fechaEvaluacion = DateTime.Now;

                    foreach (var comp in vm.Companeros)
                    {
                        var evaluacion = new Evaluacion
                        {
                            EvaluadorId = vm.EvaluadorId,
                            EvaluadoId = comp.EvaluadoId,
                            PeriodoId = vm.PeriodoId,
                            Comentario = comp.Comentario,
                            Fecha = fechaEvaluacion
                        };

                        _context.Evaluaciones.Add(evaluacion);
                        await _context.SaveChangesAsync(); // Para obtener el Id

                        foreach (var crit in comp.Criterios)
                        {
                            var detalle = new DetalleEvaluacion
                            {
                                EvaluacionId = evaluacion.Id,
                                CriterioId = crit.CriterioId,
                                Calificacion = crit.Calificacion
                            };
                            _context.DetallesEvaluacion.Add(detalle);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Todas las evaluaciones fueron registradas correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Ocurrió un error inesperado al guardar las evaluaciones.");
                }
            }

            // Si hay error, necesitamos volver a cargar los nombres (no vienen en el POST si son hidden y se manipulan, pero los pusimos ocultos en la vista)
            return View(vm);
        }

        private async Task PopulateDropDownsAsync(int? evaluadorId = null, int? periodoId = null)
        {
            var evaluadores = await _context.Integrantes
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .Select(i => new { i.Id, NombreCompleto = $"{i.Nombre} {i.Apellido}" })
                .ToListAsync();

            var periodos = await _context.Periodos
                .Where(p => p.Activo)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            ViewData["EvaluadorId"] = new SelectList(evaluadores, "Id", "NombreCompleto", evaluadorId);
            ViewData["PeriodoId"] = new SelectList(periodos, "Id", "Nombre", periodoId);
        }
    }
}
