using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.Models;

namespace Coevaluacion.Controllers
{
    public class EquiposController : Controller
    {
        private readonly AppDbContext _context;

        public EquiposController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Equipos
        public async Task<IActionResult> Index()
        {
            // Listar equipos ordenados alfabéticamente
            var equipos = await _context.Equipos
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(equipos);
        }

        // GET: Equipos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipo == null)
            {
                return NotFound();
            }

            return View(equipo);
        }

        // GET: Equipos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Equipos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Equipo equipo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(equipo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Equipo creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(equipo);
        }

        // GET: Equipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                return NotFound();
            }
            return View(equipo);
        }

        // POST: Equipos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Equipo equipo)
        {
            if (id != equipo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar existencia antes de actualizar
                    var equipoExiste = await _context.Equipos.AnyAsync(e => e.Id == id);
                    if (!equipoExiste)
                    {
                        return NotFound();
                    }

                    _context.Update(equipo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Equipo actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipoExists(equipo.Id))
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
            return View(equipo);
        }

        // GET: Equipos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Equipos
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (equipo == null)
            {
                return NotFound();
            }

            return View(equipo);
        }

        // POST: Equipos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Obtener Ids de los integrantes del equipo
            var integrantesIds = await _context.Integrantes
                .Where(i => i.EquipoId == id)
                .Select(i => i.Id)
                .ToListAsync();

            if (integrantesIds.Any())
            {
                // Restricción 2: Verificar si alguno tiene historial
                bool tieneHistorial = await _context.Evaluaciones
                    .AnyAsync(e => integrantesIds.Contains(e.EvaluadorId) || integrantesIds.Contains(e.EvaluadoId));

                if (tieneHistorial)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el equipo porque contiene integrantes con historial de evaluaciones.";
                    return RedirectToAction(nameof(Index));
                }
                
                // Restricción 3 (Adicional): No permitir eliminar si tiene integrantes registrados
                TempData["ErrorMessage"] = "No se puede eliminar el equipo porque posee integrantes registrados.";
                return RedirectToAction(nameof(Index));
            }

            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo != null)
            {
                _context.Equipos.Remove(equipo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Equipo eliminado correctamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool EquipoExists(int id)
        {
            return _context.Equipos.Any(e => e.Id == id);
        }
    }
}
