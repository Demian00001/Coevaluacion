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
            // La validación de negocio (FechaFin >= FechaInicio) se maneja automáticamente 
            // mediante la implementación de IValidatableObject en la entidad Periodo.
            if (ModelState.IsValid)
            {
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
            // Verificar existencia antes de eliminar
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
