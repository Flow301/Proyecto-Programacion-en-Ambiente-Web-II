using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCategoriaActivoYActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaActivo",
                columns: table => new
                {
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Prefijo = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    IntervaloKm = table.Column<int>(type: "int", nullable: false),
                    MargenAvisoKm = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaActivo", x => x.IdCategoria);
                    table.CheckConstraint("CK_CategoriaActivo_IntervaloKm", "[IntervaloKm] > 0");
                    table.CheckConstraint("CK_CategoriaActivo_MargenAvisoKm", "[MargenAvisoKm] >= 0 AND [MargenAvisoKm] < [IntervaloKm]");
                });

            migrationBuilder.CreateTable(
                name: "Activo",
                columns: table => new
                {
                    IdActivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    IdEstadoActivo = table.Column<int>(type: "int", nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "date", nullable: false),
                    ValorReferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Placa = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Anio = table.Column<short>(type: "smallint", nullable: false),
                    KilometrajeActual = table.Column<int>(type: "int", nullable: false),
                    KilometrajeUltimoPreventivo = table.Column<int>(type: "int", nullable: false),
                    IntervaloKmPropio = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activo", x => x.IdActivo);
                    table.CheckConstraint("CK_Activo_IntervaloKmPropio", "[IntervaloKmPropio] IS NULL OR [IntervaloKmPropio] > 0");
                    table.CheckConstraint("CK_Activo_KilometrajeActual", "[KilometrajeActual] >= 0");
                    table.CheckConstraint("CK_Activo_KilometrajeUltimoPreventivo", "[KilometrajeUltimoPreventivo] BETWEEN 0 AND [KilometrajeActual]");
                    table.CheckConstraint("CK_Activo_ValorReferencia", "[ValorReferencia] >= 0");
                    table.ForeignKey(
                        name: "FK_Activo_CategoriaActivo",
                        column: x => x.IdCategoria,
                        principalTable: "CategoriaActivo",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activo_EstadoActivo",
                        column: x => x.IdEstadoActivo,
                        principalTable: "EstadoActivo",
                        principalColumn: "IdEstadoActivo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CategoriaActivo",
                columns: new[] { "IdCategoria", "Descripcion", "IntervaloKm", "MargenAvisoKm", "Nombre", "Prefijo" },
                values: new object[,]
                {
                    { 1, null, 10000, 1000, "Pickup", "PKP" },
                    { 2, null, 10000, 1000, "Camión liviano", "CML" },
                    { 3, null, 15000, 1500, "Camión pesado", "CMP" },
                    { 4, null, 20000, 2000, "Tractocamión", "TRC" },
                    { 5, null, 10000, 1000, "Bus urbano", "BUS" },
                    { 6, null, 15000, 1500, "Bus interurbano", "BUI" },
                    { 7, null, 10000, 1000, "Microbús", "MCB" },
                    { 8, null, 10000, 1000, "Van de carga", "VAN" },
                    { 9, null, 10000, 1000, "Vehículo administrativo", "ADM" },
                    { 10, null, 5000, 500, "Motocicleta", "MOT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activo_IdCategoria",
                table: "Activo",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Activo_IdEstadoActivo",
                table: "Activo",
                column: "IdEstadoActivo");

            migrationBuilder.CreateIndex(
                name: "UQ_Activo_Codigo",
                table: "Activo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Activo_Placa",
                table: "Activo",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CategoriaActivo_Nombre",
                table: "CategoriaActivo",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CategoriaActivo_Prefijo",
                table: "CategoriaActivo",
                column: "Prefijo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activo");

            migrationBuilder.DropTable(
                name: "CategoriaActivo");
        }
    }
}
