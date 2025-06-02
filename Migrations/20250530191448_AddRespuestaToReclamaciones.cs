using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class AddRespuestaToReclamaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Respuesta",
                table: "Reclamaciones",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Respuesta",
                table: "Reclamaciones");
        }
    }
}
