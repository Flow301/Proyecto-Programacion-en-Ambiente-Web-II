using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreaCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadoActivo",
                columns: table => new
                {
                    IdEstadoActivo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoActivo", x => x.IdEstadoActivo);
                });

            migrationBuilder.CreateTable(
                name: "EstadoOrden",
                columns: table => new
                {
                    IdEstadoOrden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoOrden", x => x.IdEstadoOrden);
                });

            migrationBuilder.CreateTable(
                name: "NaturalezaMantenimiento",
                columns: table => new
                {
                    IdNaturaleza = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaturalezaMantenimiento", x => x.IdNaturaleza);
                });

            migrationBuilder.CreateTable(
                name: "NivelCertificacion",
                columns: table => new
                {
                    IdNivelCertificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NivelCertificacion", x => x.IdNivelCertificacion);
                });

            migrationBuilder.CreateTable(
                name: "Prioridad",
                columns: table => new
                {
                    IdPrioridad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nivel = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridad", x => x.IdPrioridad);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                });

            migrationBuilder.InsertData(
                table: "EstadoActivo",
                columns: new[] { "IdEstadoActivo", "Nombre" },
                values: new object[,]
                {
                    { 1, "Operativo" },
                    { 2, "En Mantenimiento" },
                    { 3, "Fuera de Servicio" },
                    { 4, "Dado de Baja" }
                });

            migrationBuilder.InsertData(
                table: "EstadoOrden",
                columns: new[] { "IdEstadoOrden", "EsFinal", "Nombre" },
                values: new object[,]
                {
                    { 1, false, "Solicitada" },
                    { 2, false, "Diagnosticada" },
                    { 3, false, "Aprobada" },
                    { 4, true, "Rechazada" },
                    { 5, false, "En Ejecución" },
                    { 6, false, "En Espera de Repuesto" },
                    { 7, true, "Completada" },
                    { 8, true, "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "NaturalezaMantenimiento",
                columns: new[] { "IdNaturaleza", "Nombre" },
                values: new object[,]
                {
                    { 1, "Preventivo" },
                    { 2, "Correctivo" },
                    { 3, "Emergencia" }
                });

            migrationBuilder.InsertData(
                table: "NivelCertificacion",
                columns: new[] { "IdNivelCertificacion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Básico" },
                    { 2, "Intermedio" },
                    { 3, "Avanzado" },
                    { 4, "Experto" }
                });

            migrationBuilder.InsertData(
                table: "Prioridad",
                columns: new[] { "IdPrioridad", "Nivel", "Nombre" },
                values: new object[,]
                {
                    { 1, (short)1, "Baja" },
                    { 2, (short)2, "Media" },
                    { 3, (short)3, "Alta" },
                    { 4, (short)4, "Crítica" }
                });

            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "IdRol", "Nombre" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Coordinador/Técnico" },
                    { 3, "Solicitante/Consulta" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoActivo_Nombre",
                table: "EstadoActivo",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoOrden_Nombre",
                table: "EstadoOrden",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NaturalezaMantenimiento_Nombre",
                table: "NaturalezaMantenimiento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NivelCertificacion_Nombre",
                table: "NivelCertificacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Prioridad_Nombre",
                table: "Prioridad",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Rol_Nombre",
                table: "Rol",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstadoActivo");

            migrationBuilder.DropTable(
                name: "EstadoOrden");

            migrationBuilder.DropTable(
                name: "NaturalezaMantenimiento");

            migrationBuilder.DropTable(
                name: "NivelCertificacion");

            migrationBuilder.DropTable(
                name: "Prioridad");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
