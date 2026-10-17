using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaEspecialidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especialidad",
                columns: table => new
                {
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidad", x => x.IdEspecialidad);
                });

            migrationBuilder.InsertData(
                table: "Especialidad",
                columns: new[] { "IdEspecialidad", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Revisión y reparación general del vehículo.", "Mecánica general" },
                    { 2, "Diagnóstico y reparación de motores diésel.", "Motores diésel" },
                    { 3, "Diagnóstico y reparación de motores de gasolina.", "Motores de gasolina" },
                    { 4, "Sistema de frenos: pastillas, discos, tambores y circuito hidráulico o neumático.", "Frenos" },
                    { 5, "Amortiguadores, muelles, rótulas y sistema de dirección.", "Suspensión y dirección" },
                    { 6, "Batería, alternador, cableado, sensores y módulos electrónicos.", "Sistema eléctrico y electrónica" },
                    { 7, "Caja de cambios, embrague y tren motriz.", "Transmisión y embrague" },
                    { 8, "Sistema de climatización y refrigerante.", "Aire acondicionado" },
                    { 9, "Cambio y reparación de llantas, alineamiento y balanceo.", "Llantas y alineamiento" },
                    { 10, "Reparación de carrocería, enderezado y pintura.", "Carrocería y pintura" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Especialidad_Nombre",
                table: "Especialidad",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Especialidad");
        }
    }
}
