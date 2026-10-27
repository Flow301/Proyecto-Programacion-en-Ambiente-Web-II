using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaOrdenMantenimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdOrden",
                table: "HistorialEstadoActivo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrdenMantenimiento",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    IdActivo = table.Column<int>(type: "int", nullable: false),
                    IdTipoMantenimiento = table.Column<int>(type: "int", nullable: false),
                    IdPrioridad = table.Column<int>(type: "int", nullable: true),
                    IdEstadoOrden = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioSolicitante = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KilometrajeDiagnostico = table.Column<int>(type: "int", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    FechaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CostoRepuestos = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostoManoObra = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenMantenimiento", x => x.IdOrden);
                    table.ForeignKey(
                        name: "FK_OrdenMantenimiento_Activo",
                        column: x => x.IdActivo,
                        principalTable: "Activo",
                        principalColumn: "IdActivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenMantenimiento_EstadoOrden",
                        column: x => x.IdEstadoOrden,
                        principalTable: "EstadoOrden",
                        principalColumn: "IdEstadoOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenMantenimiento_Prioridad",
                        column: x => x.IdPrioridad,
                        principalTable: "Prioridad",
                        principalColumn: "IdPrioridad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenMantenimiento_TipoMantenimiento",
                        column: x => x.IdTipoMantenimiento,
                        principalTable: "TipoMantenimiento",
                        principalColumn: "IdTipoMantenimiento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenMantenimiento_Usuario",
                        column: x => x.IdUsuarioSolicitante,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstadoOrden",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    IdEstadoAnterior = table.Column<int>(type: "int", nullable: true),
                    IdEstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoOrden", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoOrden_EstadoAnterior",
                        column: x => x.IdEstadoAnterior,
                        principalTable: "EstadoOrden",
                        principalColumn: "IdEstadoOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoOrden_EstadoNuevo",
                        column: x => x.IdEstadoNuevo,
                        principalTable: "EstadoOrden",
                        principalColumn: "IdEstadoOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoOrden_OrdenMantenimiento",
                        column: x => x.IdOrden,
                        principalTable: "OrdenMantenimiento",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoOrden_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoActivo_IdOrden",
                table: "HistorialEstadoActivo",
                column: "IdOrden");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoOrden_IdEstadoAnterior",
                table: "HistorialEstadoOrden",
                column: "IdEstadoAnterior");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoOrden_IdEstadoNuevo",
                table: "HistorialEstadoOrden",
                column: "IdEstadoNuevo");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoOrden_IdOrden",
                table: "HistorialEstadoOrden",
                column: "IdOrden");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoOrden_IdUsuario",
                table: "HistorialEstadoOrden",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMantenimiento_IdActivo",
                table: "OrdenMantenimiento",
                column: "IdActivo");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMantenimiento_IdEstadoOrden",
                table: "OrdenMantenimiento",
                column: "IdEstadoOrden");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMantenimiento_IdPrioridad",
                table: "OrdenMantenimiento",
                column: "IdPrioridad");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMantenimiento_IdTipoMantenimiento",
                table: "OrdenMantenimiento",
                column: "IdTipoMantenimiento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMantenimiento_IdUsuarioSolicitante",
                table: "OrdenMantenimiento",
                column: "IdUsuarioSolicitante");

            migrationBuilder.CreateIndex(
                name: "UQ_OrdenMantenimiento_Codigo",
                table: "OrdenMantenimiento",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialEstadoActivo_OrdenMantenimiento",
                table: "HistorialEstadoActivo",
                column: "IdOrden",
                principalTable: "OrdenMantenimiento",
                principalColumn: "IdOrden",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistorialEstadoActivo_OrdenMantenimiento",
                table: "HistorialEstadoActivo");

            migrationBuilder.DropTable(
                name: "HistorialEstadoOrden");

            migrationBuilder.DropTable(
                name: "OrdenMantenimiento");

            migrationBuilder.DropIndex(
                name: "IX_HistorialEstadoActivo_IdOrden",
                table: "HistorialEstadoActivo");

            migrationBuilder.DropColumn(
                name: "IdOrden",
                table: "HistorialEstadoActivo");
        }
    }
}
