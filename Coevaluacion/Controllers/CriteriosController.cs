using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Data;
using Coevaluacion.Models;

namespace Coevaluacion.Controllers
{
    public class CriteriosController : Controller
    {
        private readonly AppDbContext _context;

        public CriteriosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Criterios
        public async Task<IActionResult> Index()
        {
            var criterios = await _context.Criterios
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(criterios);
        }

        // GET: Criterios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterio = await _context.Criterios
                .FirstOrDefaultAsync(m => m.Id == id);

            if (criterio == null)
            {
                return NotFound();
            }

            return View(criterio);
        }

        // GET: Criterios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Criterios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Activo")] Criterio criterio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(criterio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Criterio creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(criterio);
        }

        // GET: Criterios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterio = await _context.Criterios.FindAsync(id);
            if (criterio == null)
            {
                return NotFound();
            }
            return View(criterio);
        }

        // POST: Criterios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Activo")] Criterio criterio)
        {
            if (id != criterio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar existencia antes de actualizar
                    var criterioExiste = await _context.Criterios.AnyAsync(c => c.Id == id);
                    if (!criterioExiste)
                    {
                        return NotFound();
                    }

                    _context.Update(criterio);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Criterio actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CriterioExists(criterio.Id))
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
            return View(criterio);
        }

        // GET: Criterios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var criterio = await _context.Criterios
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (criterio == null)
            {
                return NotFound();
            }

            return View(criterio);
        }

        // POST: Criterios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Verificar existencia antes de eliminar
            var criterio = await _context.Criterios.FindAsync(id);
            if (criterio != null)
            {
                _context.Criterios.Remove(criterio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Criterio eliminado correctamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool CriterioExists(int id)
        {
            return _context.Criterios.Any(e => e.Id == id);
        }
    }
}
