using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTecnicoYTecnicoEspecialidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tecnico",
                columns: table => new
                {
                    IdTecnico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Identificacion = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    TarifaHora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnico", x => x.IdTecnico);
                    table.CheckConstraint("CK_Tecnico_TarifaHora", "[TarifaHora] > 0");
                });

            migrationBuilder.CreateTable(
                name: "TecnicoEspecialidad",
                columns: table => new
                {
                    IdTecnico = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false),
                    IdNivelCertificacion = table.Column<int>(type: "int", nullable: false),
                    FechaCertificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TecnicoEspecialidad", x => new { x.IdTecnico, x.IdEspecialidad });
                    table.ForeignKey(
                        name: "FK_TecnicoEspecialidad_Especialidad",
                        column: x => x.IdEspecialidad,
                        principalTable: "Especialidad",
                        principalColumn: "IdEspecialidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TecnicoEspecialidad_NivelCertificacion",
                        column: x => x.IdNivelCertificacion,
                        principalTable: "NivelCertificacion",
                        principalColumn: "IdNivelCertificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TecnicoEspecialidad_Tecnico",
                        column: x => x.IdTecnico,
                        principalTable: "Tecnico",
                        principalColumn: "IdTecnico",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Tecnico_Identificacion",
                table: "Tecnico",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TecnicoEspecialidad_IdEspecialidad",
                table: "TecnicoEspecialidad",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "IX_TecnicoEspecialidad_IdNivelCertificacion",
                table: "TecnicoEspecialidad",
                column: "IdNivelCertificacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TecnicoEspecialidad");

            migrationBuilder.DropTable(
                name: "Tecnico");
        }
    }
}
