using Coevaluacion.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Coevaluacion.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerarReporteIndividual(ReporteIndividualViewModel data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureStandardPage(page);
                    
                    page.Header().Element(ComposeStandardHeader);
                    page.Content().Element(c => ComposeIndividualContent(c, data));
                    page.Footer().Element(ComposeStandardFooter);
                });
            }).GeneratePdf();
        }

        public byte[] GenerarReporteEquipo(ReporteEquipoViewModel data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureStandardPage(page);
                    
                    page.Header().Element(ComposeStandardHeader);
                    page.Content().Element(c => ComposeEquipoContent(c, data));
                    page.Footer().Element(ComposeStandardFooter);
                });
            }).GeneratePdf();
        }

        public byte[] GenerarRankingGeneral(RankingGeneralViewModel data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigureStandardPage(page);
                    
                    page.Header().Element(ComposeStandardHeader);
                    page.Content().Element(c => ComposeRankingContent(c, data));
                    page.Footer().Element(ComposeStandardFooter);
                });
            }).GeneratePdf();
        }

        // --- MÉTODOS COMUNES ---

        private void ConfigureStandardPage(PageDescriptor page)
        {
            page.Size(PageSizes.Letter);
            page.Margin(2, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));
        }

        private void ComposeStandardHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Sistema de Coevaluación").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Universidad La Salle").FontSize(14).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(5).Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeStandardFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
                x.Span(" | Generado automáticamente por el Sistema de Coevaluación").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }

        // --- COMPOSICIÓN INDIVIDUAL ---

        private void ComposeIndividualContent(IContainer container, ReporteIndividualViewModel data)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("Reporte Individual de Desempeño").FontSize(16).SemiBold();

                // Datos del estudiante
                column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Estudiante: {data.NombreEstudiante}").SemiBold();
                        c.Item().Text($"Equipo: {data.EquipoNombre}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Periodo: {data.PeriodoNombre}");
                        c.Item().Text($"Promedio General: {data.PromedioGeneral:F2}").SemiBold().FontColor(Colors.Green.Darken2);
                    });
                });

                // Criterios
                column.Item().Text("Resultados por Criterio").FontSize(14).SemiBold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Criterio").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Promedio").SemiBold();
                    });

                    foreach (var crit in data.PromediosPorCriterio)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(crit.CriterioNombre);
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(crit.Promedio.ToString("F2"));
                    }
                });

                // Comentarios
                column.Item().Text("Comentarios Recibidos").FontSize(14).SemiBold();
                if (data.Comentarios.Any())
                {
                    foreach (var com in data.Comentarios)
                    {
                        column.Item().PaddingLeft(10).Text($"• \"{com}\"").Italic().FontColor(Colors.Grey.Darken2);
                    }
                }
                else
                {
                    column.Item().Text("No se registraron comentarios en este periodo.").Italic().FontColor(Colors.Grey.Medium);
                }
            });
        }

        // --- COMPOSICIÓN EQUIPO ---

        private void ComposeEquipoContent(IContainer container, ReporteEquipoViewModel data)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("Reporte de Desempeño por Equipo").FontSize(16).SemiBold();

                // Datos del equipo
                column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Equipo: {data.EquipoNombre}").SemiBold();
                        c.Item().Text($"Periodo: {data.PeriodoNombre}");
                        c.Item().Text($"Integrantes: {data.CantidadIntegrantes}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Promedio del Equipo: {data.PromedioGeneralEquipo:F2}").SemiBold().FontColor(Colors.Blue.Darken2);
                        c.Item().Text($"Mejor Evaluado: {data.MejorIntegranteNombre}");
                    });
                });

                // Tabla Integrantes
                column.Item().Text("Ranking Interno del Equipo").FontSize(14).SemiBold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Pos").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Nombre").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Promedio").SemiBold();
                    });

                    int pos = 1;
                    foreach (var integrante in data.Integrantes)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(pos.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(integrante.NombreCompleto);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(integrante.PromedioGeneral.ToString("F2"));
                        pos++;
                    }
                });
            });
        }

        // --- COMPOSICIÓN RANKING ---

        private void ComposeRankingContent(IContainer container, RankingGeneralViewModel data)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("Ranking General de Desempeño").FontSize(16).SemiBold();

                // Estadísticas Globales
                column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Periodo: {data.PeriodoNombre}").SemiBold();
                        c.Item().Text($"Total Evaluados: {data.TotalEstudiantes}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Promedio Global: {data.PromedioGlobal:F2}").SemiBold().FontColor(Colors.Blue.Darken2);
                        c.Item().Text($"Mejor: {data.MejorPromedio:F2} | Peor: {data.PeorPromedio:F2}");
                    });
                });

                // Tabla Completa
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Pos").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Nombre").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Equipo").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Promedio").SemiBold();
                    });

                    foreach (var est in data.Estudiantes)
                    {
                        // Resaltar los primeros 3
                        var bgColor = est.Posicion switch
                        {
                            1 => Colors.Yellow.Lighten4,
                            2 => Colors.Grey.Lighten3,
                            3 => Colors.Orange.Lighten4,
                            _ => Colors.White
                        };

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(est.Posicion.ToString());
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(est.NombreCompleto);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(est.EquipoNombre);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(est.PromedioGeneral.ToString("F2"));
                    }
                });
            });
        }
    }
}
