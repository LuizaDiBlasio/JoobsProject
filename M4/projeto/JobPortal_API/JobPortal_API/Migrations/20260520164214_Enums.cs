using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class Enums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CV_Escolaridade_IdEscolaridade",
                table: "CV");

            migrationBuilder.DropTable(
                name: "Escolaridade");

            migrationBuilder.DropIndex(
                name: "IX_CV_IdEscolaridade",
                table: "CV");

            migrationBuilder.DropColumn(
                name: "IsFullTime",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "IsPresencial",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "IdEscolaridade",
                table: "CV");

            migrationBuilder.AddColumn<string>(
                name: "Jornada",
                table: "OfertaEmprego",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegimeTrabalho",
                table: "OfertaEmprego",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Escolaridade",
                table: "CV",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Jornada",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "RegimeTrabalho",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "Escolaridade",
                table: "CV");

            migrationBuilder.AddColumn<bool>(
                name: "IsFullTime",
                table: "OfertaEmprego",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPresencial",
                table: "OfertaEmprego",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdEscolaridade",
                table: "CV",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Escolaridade",
                columns: table => new
                {
                    IdEscolaridade = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escolaridade", x => x.IdEscolaridade);
                });

            migrationBuilder.InsertData(
                table: "Escolaridade",
                columns: new[] { "IdEscolaridade", "Tipo" },
                values: new object[,]
                {
                    { 1, "Ensino Básico" },
                    { 2, "Ensino Secundário" },
                    { 3, "Licenciatura" },
                    { 4, "Mestrado" },
                    { 5, "Doutoramento" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CV_IdEscolaridade",
                table: "CV",
                column: "IdEscolaridade");

            migrationBuilder.AddForeignKey(
                name: "FK_CV_Escolaridade_IdEscolaridade",
                table: "CV",
                column: "IdEscolaridade",
                principalTable: "Escolaridade",
                principalColumn: "IdEscolaridade",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
