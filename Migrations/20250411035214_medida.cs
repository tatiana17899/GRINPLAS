using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class medida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Medida",
                table: "DetallePedidos");

            migrationBuilder.AddColumn<DateTime>(
                name: "FecCre",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FecCre",
                table: "Clientes");

            migrationBuilder.AddColumn<double>(
                name: "Medida",
                table: "DetallePedidos",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
