using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coevaluacion.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluacionEstructura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvaluadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    EvaluadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Integrantes_EvaluadoId",
                        column: x => x.EvaluadoId,
                        principalTable: "Integrantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Integrantes_EvaluadorId",
                        column: x => x.EvaluadorId,
                        principalTable: "Integrantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Periodos_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "Periodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesEvaluacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EvaluacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriterioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Calificacion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesEvaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesEvaluacion_Criterios_CriterioId",
                        column: x => x.CriterioId,
                        principalTable: "Criterios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesEvaluacion_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEvaluacion_CriterioId",
                table: "DetallesEvaluacion",
                column: "CriterioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEvaluacion_EvaluacionId",
                table: "DetallesEvaluacion",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_EvaluadoId",
                table: "Evaluaciones",
                column: "EvaluadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_EvaluadorId",
                table: "Evaluaciones",
                column: "EvaluadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_PeriodoId",
                table: "Evaluaciones",
                column: "PeriodoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesEvaluacion");

            migrationBuilder.DropTable(
                name: "Evaluaciones");
        }
    }
}
