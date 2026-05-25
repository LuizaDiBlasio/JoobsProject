using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class EscolaridadeLookupTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Educacao",
                table: "CV");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "IdEscolaridade",
                table: "CV");

            migrationBuilder.AddColumn<string>(
                name: "Educacao",
                table: "CV",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
