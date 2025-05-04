using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GRINPLAS.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaNotificaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PRIMERO crea la tabla
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    NotificacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: true),
                    PedidoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.NotificacionId);
                    table.ForeignKey(
                        name: "FK_Notificaciones_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoId");
                });

            // LUEGO crea los índices si es necesario
            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_PedidoId",
                table: "Notificaciones",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");
        }
    }
}
