using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregaVariableDeControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdVariableControl",
                table: "CategoriaActivo",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "VariableControl",
                columns: table => new
                {
                    IdVariableControl = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariableControl", x => x.IdVariableControl);
                });

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 1,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 2,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 3,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 4,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 5,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 6,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 7,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 8,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 9,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CategoriaActivo",
                keyColumn: "IdCategoria",
                keyValue: 10,
                column: "IdVariableControl",
                value: 1);

            migrationBuilder.InsertData(
                table: "VariableControl",
                columns: new[] { "IdVariableControl", "Nombre", "Unidad" },
                values: new object[] { 1, "Kilometraje", "km" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaActivo_IdVariableControl",
                table: "CategoriaActivo",
                column: "IdVariableControl");

            migrationBuilder.CreateIndex(
                name: "UQ_VariableControl_Nombre",
                table: "VariableControl",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaActivo_VariableControl",
                table: "CategoriaActivo",
                column: "IdVariableControl",
                principalTable: "VariableControl",
                principalColumn: "IdVariableControl",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriaActivo_VariableControl",
                table: "CategoriaActivo");

            migrationBuilder.DropTable(
                name: "VariableControl");

            migrationBuilder.DropIndex(
                name: "IX_CategoriaActivo_IdVariableControl",
                table: "CategoriaActivo");

            migrationBuilder.DropColumn(
                name: "IdVariableControl",
                table: "CategoriaActivo");
        }
    }
}
