using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaDetalleYAsignacionesDeOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionTecnico",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    IdTecnico = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HorasTrabajadas = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    TarifaAplicada = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionTecnico", x => new { x.IdOrden, x.IdTecnico });
                    table.CheckConstraint("CK_AsignacionTecnico_HorasTrabajadas", "[HorasTrabajadas] >= 0");
                    table.ForeignKey(
                        name: "FK_AsignacionTecnico_OrdenMantenimiento",
                        column: x => x.IdOrden,
                        principalTable: "OrdenMantenimiento",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionTecnico_Tecnico",
                        column: x => x.IdTecnico,
                        principalTable: "Tecnico",
                        principalColumn: "IdTecnico",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleRepuesto",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    IdRepuesto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleRepuesto", x => new { x.IdOrden, x.NumeroLinea });
                    table.CheckConstraint("CK_DetalleRepuesto_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_DetalleRepuesto_CostoUnitario", "[CostoUnitario] > 0");
                    table.ForeignKey(
                        name: "FK_DetalleRepuesto_OrdenMantenimiento",
                        column: x => x.IdOrden,
                        principalTable: "OrdenMantenimiento",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRepuesto_Repuesto",
                        column: x => x.IdRepuesto,
                        principalTable: "Repuesto",
                        principalColumn: "IdRepuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepuestoPendiente",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    IdRepuesto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestoPendiente", x => new { x.IdOrden, x.IdRepuesto });
                    table.CheckConstraint("CK_RepuestoPendiente_Cantidad", "[Cantidad] > 0");
                    table.ForeignKey(
                        name: "FK_RepuestoPendiente_OrdenMantenimiento",
                        column: x => x.IdOrden,
                        principalTable: "OrdenMantenimiento",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepuestoPendiente_Repuesto",
                        column: x => x.IdRepuesto,
                        principalTable: "Repuesto",
                        principalColumn: "IdRepuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionTecnico_IdTecnico",
                table: "AsignacionTecnico",
                column: "IdTecnico");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRepuesto_IdRepuesto",
                table: "DetalleRepuesto",
                column: "IdRepuesto");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoPendiente_IdRepuesto",
                table: "RepuestoPendiente",
                column: "IdRepuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionTecnico");

            migrationBuilder.DropTable(
                name: "DetalleRepuesto");

            migrationBuilder.DropTable(
                name: "RepuestoPendiente");
        }
    }
}
