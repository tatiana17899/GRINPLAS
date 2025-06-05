using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class @decimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sueldo",
                table: "Trabajadores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Sueldo",
                table: "Trabajadores",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
