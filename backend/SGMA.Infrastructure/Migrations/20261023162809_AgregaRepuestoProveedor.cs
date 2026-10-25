using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaRepuestoProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepuestoProveedor",
                columns: table => new
                {
                    IdRepuesto = table.Column<int>(type: "int", nullable: false),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    PrecioOfrecido = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TiempoEntregaDias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestoProveedor", x => new { x.IdRepuesto, x.IdProveedor });
                    table.CheckConstraint("CK_RepuestoProveedor_PrecioOfrecido", "[PrecioOfrecido] > 0");
                    table.CheckConstraint("CK_RepuestoProveedor_TiempoEntregaDias", "[TiempoEntregaDias] >= 1");
                    table.ForeignKey(
                        name: "FK_RepuestoProveedor_Proveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedor",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepuestoProveedor_Repuesto",
                        column: x => x.IdRepuesto,
                        principalTable: "Repuesto",
                        principalColumn: "IdRepuesto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoProveedor_IdProveedor",
                table: "RepuestoProveedor",
                column: "IdProveedor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepuestoProveedor");
        }
    }
}
