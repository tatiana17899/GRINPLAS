using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class MigracionComprobantePago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComprobantePago",
                table: "Pedidos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComprobantePago",
                table: "Pedidos");
        }
    }
}
