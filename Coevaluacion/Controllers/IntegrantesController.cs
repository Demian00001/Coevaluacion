using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.Models;

namespace Coevaluacion.Controllers
{
    public class IntegrantesController : Controller
    {
        private readonly AppDbContext _context;

        public IntegrantesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Integrantes
        public async Task<IActionResult> Index()
        {
            // Listar integrantes ordenados por apellido y nombre, e incluir el Equipo
            var integrantes = await _context.Integrantes
                .Include(i => i.Equipo)
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .ToListAsync();

            return View(integrantes);
        }

        // GET: Integrantes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var integrante = await _context.Integrantes
                .Include(i => i.Equipo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (integrante == null)
            {
                return NotFound();
            }

            return View(integrante);
        }

        // GET: Integrantes/Create
        public IActionResult Create()
        {
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Nombre");
            return View();
        }

        // POST: Integrantes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,Correo,Matricula,EquipoId")] Integrante integrante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(integrante);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Integrante creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Nombre", integrante.EquipoId);
            return View(integrante);
        }

        // GET: Integrantes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var integrante = await _context.Integrantes.FindAsync(id);
            if (integrante == null)
            {
                return NotFound();
            }
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Nombre", integrante.EquipoId);
            return View(integrante);
        }

        // POST: Integrantes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Correo,Matricula,EquipoId")] Integrante integrante)
        {
            if (id != integrante.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var integranteOriginal = await _context.Integrantes.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (integranteOriginal == null)
                    {
                        return NotFound();
                    }

                    // Restricción 5: No permitir cambiar de equipo si tiene historial
                    if (integranteOriginal.EquipoId != integrante.EquipoId)
                    {
                        bool tieneHistorial = await _context.Evaluaciones
                            .AnyAsync(e => e.EvaluadorId == id || e.EvaluadoId == id);

                        if (tieneHistorial)
                        {
                            integrante.EquipoId = integranteOriginal.EquipoId; // Revertir cambio
                            TempData["ErrorMessage"] = "No se puede cambiar de equipo porque el integrante posee historial de evaluaciones. Los demás datos sí fueron actualizados.";
                        }
                    }

                    _context.Update(integrante);
                    await _context.SaveChangesAsync();

                    if (TempData["ErrorMessage"] == null)
                    {
                        TempData["SuccessMessage"] = "Integrante actualizado correctamente.";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IntegranteExists(integrante.Id))
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
            ViewData["EquipoId"] = new SelectList(_context.Equipos, "Id", "Nombre", integrante.EquipoId);
            return View(integrante);
        }

        // GET: Integrantes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var integrante = await _context.Integrantes
                .Include(i => i.Equipo)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (integrante == null)
            {
                return NotFound();
            }

            return View(integrante);
        }

        // POST: Integrantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Restricción 1: No permitir eliminar si ha sido evaluador o evaluado
            bool tieneHistorial = await _context.Evaluaciones
                .AnyAsync(e => e.EvaluadorId == id || e.EvaluadoId == id);

            if (tieneHistorial)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el integrante porque posee historial de evaluaciones.";
                return RedirectToAction(nameof(Index));
            }

            var integrante = await _context.Integrantes.FindAsync(id);
            if (integrante != null)
            {
                _context.Integrantes.Remove(integrante);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Integrante eliminado correctamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool IntegranteExists(int id)
        {
            return _context.Integrantes.Any(e => e.Id == id);
        }
    }
}
