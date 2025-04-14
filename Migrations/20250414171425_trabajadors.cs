using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class trabajadors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTrabajador",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTrabajador",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);
        }
    }
}
