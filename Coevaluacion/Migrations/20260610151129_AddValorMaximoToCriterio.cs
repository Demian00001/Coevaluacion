using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coevaluacion.Migrations
{
    /// <inheritdoc />
    public partial class AddValorMaximoToCriterio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValorMaximo",
                table: "Criterios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorMaximo",
                table: "Criterios");
        }
    }
}
