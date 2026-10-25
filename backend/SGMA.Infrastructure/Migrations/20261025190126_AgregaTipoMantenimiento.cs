using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTipoMantenimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoMantenimiento",
                columns: table => new
                {
                    IdTipoMantenimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdNaturaleza = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidadRequerida = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoMantenimiento", x => x.IdTipoMantenimiento);
                    table.ForeignKey(
                        name: "FK_TipoMantenimiento_Especialidad",
                        column: x => x.IdEspecialidadRequerida,
                        principalTable: "Especialidad",
                        principalColumn: "IdEspecialidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TipoMantenimiento_NaturalezaMantenimiento",
                        column: x => x.IdNaturaleza,
                        principalTable: "NaturalezaMantenimiento",
                        principalColumn: "IdNaturaleza",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TipoMantenimiento",
                columns: new[] { "IdTipoMantenimiento", "Descripcion", "IdEspecialidadRequerida", "IdNaturaleza", "Nombre" },
                values: new object[,]
                {
                    { 1, null, 1, 1, "Servicio preventivo por kilometraje" },
                    { 2, null, 2, 1, "Servicio preventivo de motor diésel" },
                    { 3, null, 4, 2, "Reparación de frenos" },
                    { 4, null, 5, 2, "Reparación de suspensión y dirección" },
                    { 5, null, 6, 2, "Reparación del sistema eléctrico" },
                    { 6, null, 2, 2, "Reparación de motor diésel" },
                    { 7, null, 3, 2, "Reparación de motor de gasolina" },
                    { 8, null, 7, 2, "Reparación de transmisión y embrague" },
                    { 9, null, 8, 2, "Reparación de aire acondicionado" },
                    { 10, null, 9, 2, "Cambio y alineamiento de llantas" },
                    { 11, null, 10, 2, "Reparación de carrocería" },
                    { 12, null, 1, 3, "Auxilio por avería en ruta" },
                    { 13, null, 1, 3, "Atención de emergencia en taller" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TipoMantenimiento_IdEspecialidadRequerida",
                table: "TipoMantenimiento",
                column: "IdEspecialidadRequerida");

            migrationBuilder.CreateIndex(
                name: "IX_TipoMantenimiento_IdNaturaleza",
                table: "TipoMantenimiento",
                column: "IdNaturaleza");

            migrationBuilder.CreateIndex(
                name: "UQ_TipoMantenimiento_Nombre",
                table: "TipoMantenimiento",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipoMantenimiento");
        }
    }
}
