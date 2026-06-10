using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Coevaluacion.Models;
using Coevaluacion.Data;
using Coevaluacion.ViewModels;

namespace Coevaluacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // 1. Tarjetas de Resumen
            viewModel.TotalEquipos = await _context.Equipos.CountAsync();
            viewModel.TotalIntegrantes = await _context.Integrantes.CountAsync();
            viewModel.TotalEvaluaciones = await _context.Evaluaciones.CountAsync();
            viewModel.TotalPeriodosActivos = await _context.Periodos.CountAsync(p => p.Activo);

            // 2. Últimas Evaluaciones (Top 5)
            viewModel.UltimasEvaluaciones = await _context.Evaluaciones
                .Include(e => e.Evaluador)
                .Include(e => e.Evaluado)
                .OrderByDescending(e => e.Fecha)
                .Take(5)
                .Select(e => new UltimaEvaluacionViewModel
                {
                    EvaluadorNombre = $"{e.Evaluador!.Nombre} {e.Evaluador.Apellido}",
                    EvaluadoNombre = $"{e.Evaluado!.Nombre} {e.Evaluado.Apellido}",
                    Fecha = e.Fecha
                })
                .ToListAsync();

            // 3. Top 5 Estudiantes
            // Agrupar todas las evaluaciones por estudiante y calcular su promedio global
            var evaluacionesPorEstudiante = await _context.Evaluaciones
                .Include(e => e.Detalles)
                .Include(e => e.Evaluado)
                .ToListAsync();

            var topEstudiantesTemp = evaluacionesPorEstudiante
                .GroupBy(e => e.EvaluadoId)
                .Select(g => new TopEstudianteViewModel
                {
                    NombreCompleto = $"{g.First().Evaluado!.Nombre} {g.First().Evaluado!.Apellido}",
                    PromedioGeneral = g.SelectMany(e => e.Detalles).Average(d => d.Calificacion)
                })
                .Where(x => x.PromedioGeneral > 0)
                .OrderByDescending(x => x.PromedioGeneral)
                .ThenBy(x => x.NombreCompleto)
                .Take(5)
                .ToList();

            // Asignar posiciones
            for (int i = 0; i < topEstudiantesTemp.Count; i++)
            {
                topEstudiantesTemp[i].Posicion = i + 1;
            }
            viewModel.TopEstudiantes = topEstudiantesTemp;

            // 4. Promedio por Equipo para Gráfica Chart.js
            var equipos = await _context.Equipos.Include(e => e.Integrantes).ToListAsync();
            foreach (var equipo in equipos)
            {
                var evaluacionesEquipo = evaluacionesPorEstudiante
                    .Where(e => e.Evaluado!.EquipoId == equipo.Id)
                    .ToList();

                double promedioEquipo = 0;
                if (evaluacionesEquipo.Any())
                {
                    promedioEquipo = evaluacionesEquipo
                        .SelectMany(e => e.Detalles)
                        .Average(d => d.Calificacion);
                }

                // Sólo agregamos a la gráfica los equipos que tienen promedio o existen
                if (promedioEquipo > 0 || equipo.Integrantes.Any())
                {
                    viewModel.EquiposLabels.Add(equipo.Nombre);
                    viewModel.EquiposPromedios.Add(Math.Round(promedioEquipo, 2));
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
