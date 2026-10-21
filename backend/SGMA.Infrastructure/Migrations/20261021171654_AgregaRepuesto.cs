using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaRepuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repuesto",
                columns: table => new
                {
                    IdRepuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuesto", x => x.IdRepuesto);
                    table.CheckConstraint("CK_Repuesto_CostoUnitario", "[CostoUnitario] > 0");
                    table.CheckConstraint("CK_Repuesto_StockActual", "[StockActual] >= 0");
                    table.CheckConstraint("CK_Repuesto_StockMinimo", "[StockMinimo] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Repuesto_Codigo",
                table: "Repuesto",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repuesto");
        }
    }
}
