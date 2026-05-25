using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class constraintFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfertaEmprego_Empresa_EmpresaIdEmpresa",
                table: "OfertaEmprego");

            migrationBuilder.DropIndex(
                name: "IX_OfertaEmprego_EmpresaIdEmpresa",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "EmpresaIdEmpresa",
                table: "OfertaEmprego");

            //migrationBuilder.CreateTable(
            //    name: "Notifications",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        IdNotification = table.Column<int>(type: "int", nullable: false),
            //        Notification = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsRead = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Notifications", x => x.UserId);
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdEmpresa",
                table: "OfertaEmprego",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_LogoEmpresa_IdEmpresaFoto",
                table: "LogoEmpresa",
                column: "IdEmpresaFoto");

            migrationBuilder.CreateIndex(
                name: "IX_Foto_IdCandidatoFoto",
                table: "Foto",
                column: "IdCandidatoFoto");

            migrationBuilder.AddForeignKey(
                name: "FK_Foto_Candidato_IdCandidatoFoto",
                table: "Foto",
                column: "IdCandidatoFoto",
                principalTable: "Candidato",
                principalColumn: "IdCandidato",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogoEmpresa_Empresa_IdEmpresaFoto",
                table: "LogoEmpresa",
                column: "IdEmpresaFoto",
                principalTable: "Empresa",
                principalColumn: "IdEmpresa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfertaEmprego_Empresa_IdEmpresa",
                table: "OfertaEmprego",
                column: "IdEmpresa",
                principalTable: "Empresa",
                principalColumn: "IdEmpresa",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Foto_Candidato_IdCandidatoFoto",
                table: "Foto");

            migrationBuilder.DropForeignKey(
                name: "FK_LogoEmpresa_Empresa_IdEmpresaFoto",
                table: "LogoEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_OfertaEmprego_Empresa_IdEmpresa",
                table: "OfertaEmprego");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_OfertaEmprego_IdEmpresa",
                table: "OfertaEmprego");

            migrationBuilder.DropIndex(
                name: "IX_LogoEmpresa_IdEmpresaFoto",
                table: "LogoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_Foto_IdCandidatoFoto",
                table: "Foto");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaIdEmpresa",
                table: "OfertaEmprego",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_EmpresaIdEmpresa",
                table: "OfertaEmprego",
                column: "EmpresaIdEmpresa");

            migrationBuilder.AddForeignKey(
                name: "FK_OfertaEmprego_Empresa_EmpresaIdEmpresa",
                table: "OfertaEmprego",
                column: "EmpresaIdEmpresa",
                principalTable: "Empresa",
                principalColumn: "IdEmpresa",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
