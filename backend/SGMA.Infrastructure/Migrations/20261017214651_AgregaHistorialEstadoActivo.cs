using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaHistorialEstadoActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialEstadoActivo",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdActivo = table.Column<int>(type: "int", nullable: false),
                    IdEstadoAnterior = table.Column<int>(type: "int", nullable: true),
                    IdEstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoActivo", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoActivo_Activo",
                        column: x => x.IdActivo,
                        principalTable: "Activo",
                        principalColumn: "IdActivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoActivo_EstadoAnterior",
                        column: x => x.IdEstadoAnterior,
                        principalTable: "EstadoActivo",
                        principalColumn: "IdEstadoActivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoActivo_EstadoNuevo",
                        column: x => x.IdEstadoNuevo,
                        principalTable: "EstadoActivo",
                        principalColumn: "IdEstadoActivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoActivo_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoActivo_IdActivo",
                table: "HistorialEstadoActivo",
                column: "IdActivo");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoActivo_IdEstadoAnterior",
                table: "HistorialEstadoActivo",
                column: "IdEstadoAnterior");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoActivo_IdEstadoNuevo",
                table: "HistorialEstadoActivo",
                column: "IdEstadoNuevo");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoActivo_IdUsuario",
                table: "HistorialEstadoActivo",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialEstadoActivo");
        }
    }
}
