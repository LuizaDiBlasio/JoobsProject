using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class TipoContrato2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "TipoContrato",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "TipoContrato",
                columns: new[] { "IdTipoContrato", "Tipo" },
                values: new object[,]
                {
                    { 1, "Sem Termo" },
                    { 2, "A Termo" },
                    { 3, "Prestação de Serviços" },
                    { 4, "Tempo Parcial" },
                    { 5, "Curta Duração" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TipoContrato",
                keyColumn: "IdTipoContrato",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TipoContrato",
                keyColumn: "IdTipoContrato",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TipoContrato",
                keyColumn: "IdTipoContrato",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TipoContrato",
                keyColumn: "IdTipoContrato",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TipoContrato",
                keyColumn: "IdTipoContrato",
                keyValue: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "TipoContrato",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
