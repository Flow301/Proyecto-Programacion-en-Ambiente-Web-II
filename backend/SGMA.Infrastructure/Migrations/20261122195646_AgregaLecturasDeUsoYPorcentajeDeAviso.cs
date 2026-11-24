using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaLecturasDeUsoYPorcentajeDeAviso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CategoriaActivo_MargenAvisoKm",
                table: "CategoriaActivo");

            migrationBuilder.RenameColumn(
                name: "MargenAvisoKm",
                table: "CategoriaActivo",
                newName: "PorcentajeAviso");

            // El margen en km pasa a porcentaje del intervalo (margen 1000 de 10000 = aviso al 90 %), dentro de 1 a 99,
            // para que las categorías creadas por los usuarios cumplan la nueva restricción
            migrationBuilder.Sql(
                "UPDATE CategoriaActivo SET PorcentajeAviso = " +
                "CASE WHEN 100 - PorcentajeAviso * 100 / IntervaloKm < 1 THEN 1 " +
                "WHEN 100 - PorcentajeAviso * 100 / IntervaloKm > 99 THEN 99 " +
                "ELSE 100 - PorcentajeAviso * 100 / IntervaloKm END");

            migrationBuilder.CreateTable(
                name: "LecturaUso",
                columns: table => new
                {
                    IdLectura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdActivo = table.Column<int>(type: "int", nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdOrden = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturaUso", x => x.IdLectura);
                    table.CheckConstraint("CK_LecturaUso_Kilometraje", "[Kilometraje] >= 0");
                    table.ForeignKey(
                        name: "FK_LecturaUso_Activo",
                        column: x => x.IdActivo,
                        principalTable: "Activo",
                        principalColumn: "IdActivo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LecturaUso_OrdenMantenimiento",
                        column: x => x.IdOrden,
                        principalTable: "OrdenMantenimiento",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LecturaUso_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 1,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 2,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 3,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 4,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 5,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 6,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 7,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 8,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 9,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 10,
                column: "PorcentajeAviso",
                value: 90);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CategoriaActivo_PorcentajeAviso",
                table: "CategoriaActivo",
                sql: "[PorcentajeAviso] BETWEEN 1 AND 99");

            migrationBuilder.CreateIndex(
                name: "IX_LecturaUso_Activo_Fecha",
                table: "LecturaUso",
                columns: new[] { "IdActivo", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_LecturaUso_IdOrden",
                table: "LecturaUso",
                column: "IdOrden");

            migrationBuilder.CreateIndex(
                name: "IX_LecturaUso_IdUsuario",
                table: "LecturaUso",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LecturaUso");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CategoriaActivo_PorcentajeAviso",
                table: "CategoriaActivo");

            migrationBuilder.RenameColumn(
                name: "PorcentajeAviso",
                table: "CategoriaActivo",
                newName: "MargenAvisoKm");

            migrationBuilder.Sql("UPDATE CategoriaActivo SET MargenAvisoKm = IntervaloKm * (100 - MargenAvisoKm) / 100");

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 1,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 2,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 3,
                column: "MargenAvisoKm",
                value: 1500);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 4,
                column: "MargenAvisoKm",
                value: 2000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 5,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 6,
                column: "MargenAvisoKm",
                value: 1500);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 7,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 8,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 9,
                column: "MargenAvisoKm",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 10,
                column: "MargenAvisoKm",
                value: 500);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CategoriaActivo_MargenAvisoKm",
                table: "CategoriaActivo",
                sql: "[MargenAvisoKm] >= 0 AND [MargenAvisoKm] < [IntervaloKm]");
        }
    }
}
