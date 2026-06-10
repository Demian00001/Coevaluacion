using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.Models;

namespace Coevaluacion.Controllers
{
    public class PeriodosController : Controller
    {
        private readonly AppDbContext _context;

        public PeriodosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Periodos
        public async Task<IActionResult> Index()
        {
            // Listar períodos ordenados por FechaInicio
            var periodos = await _context.Periodos
                .OrderBy(p => p.FechaInicio)
                .ToListAsync();

            return View(periodos);
        }

        // GET: Periodos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo = await _context.Periodos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (periodo == null)
            {
                return NotFound();
            }

            return View(periodo);
        }

        // GET: Periodos/Create
        public IActionResult Create()
        {
            return View(new Periodo { Nombre = string.Empty, FechaInicio = DateTime.Today, FechaFin = DateTime.Today.AddDays(30) });
        }

        // POST: Periodos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
        {
            if (ModelState.IsValid)
            {
                // Restricción 6: Si el nuevo periodo es activo, desactivar los demás
                if (periodo.Activo)
                {
                    var periodosActivos = await _context.Periodos.Where(p => p.Activo).ToListAsync();
                    foreach (var p in periodosActivos)
                    {
                        p.Activo = false;
                        _context.Update(p);
                    }
                }

                _context.Add(periodo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Período creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(periodo);
        }

        // GET: Periodos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo = await _context.Periodos.FindAsync(id);
            if (periodo == null)
            {
                return NotFound();
            }
            return View(periodo);
        }

        // POST: Periodos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
        {
            if (id != periodo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar existencia antes de actualizar
                    var periodoExiste = await _context.Periodos.AnyAsync(p => p.Id == id);
                    if (!periodoExiste)
                    {
                        return NotFound();
                    }

                    // Restricción 6: Si el periodo modificado se marca como activo, desactivar los demás
                    if (periodo.Activo)
                    {
                        var periodosActivos = await _context.Periodos.Where(p => p.Activo && p.Id != periodo.Id).ToListAsync();
                        foreach (var p in periodosActivos)
                        {
                            p.Activo = false;
                            _context.Update(p);
                        }
                    }

                    _context.Update(periodo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Período actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeriodoExists(periodo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(periodo);
        }

        // GET: Periodos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo = await _context.Periodos
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (periodo == null)
            {
                return NotFound();
            }

            return View(periodo);
        }

        // POST: Periodos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Nueva Restricción: No permitir eliminar un periodo activo
            bool esActivo = await _context.Periodos.AnyAsync(p => p.Id == id && p.Activo);
            if (esActivo)
            {
                TempData["ErrorMessage"] = "No se puede eliminar un período activo.";
                return RedirectToAction(nameof(Index));
            }

            // Restricción 4: No permitir eliminar un periodo con evaluaciones
            bool tieneEvaluaciones = await _context.Evaluaciones.AnyAsync(e => e.PeriodoId == id);
            if (tieneEvaluaciones)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el periodo porque posee evaluaciones registradas.";
                return RedirectToAction(nameof(Index));
            }

            var periodo = await _context.Periodos.FindAsync(id);
            if (periodo != null)
            {
                _context.Periodos.Remove(periodo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Período eliminado correctamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool PeriodoExists(int id)
        {
            return _context.Periodos.Any(e => e.Id == id);
        }
    }
}
