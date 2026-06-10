using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coevaluacion.Migrations
{
    /// <inheritdoc />
    public partial class RemovePesoAndValorMaximoFromCriterio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Criterios");

            migrationBuilder.DropColumn(
                name: "ValorMaximo",
                table: "Criterios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Peso",
                table: "Criterios",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ValorMaximo",
                table: "Criterios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
