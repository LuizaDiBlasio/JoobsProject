using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class InitCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Candidato",
                columns: table => new
                {
                    IdCandidato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<int>(type: "int", nullable: true),
                    Morada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNasc = table.Column<DateTime>(type: "Date", nullable: true),
                    LinkedIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidato", x => x.IdCandidato);
                    table.ForeignKey(
                        name: "FK_Candidato_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Concelho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<int>(type: "int", nullable: true),
                    NoFuncionarios = table.Column<int>(type: "int", nullable: true),
                    ZonaAtuacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedIn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.IdEmpresa);
                    table.ForeignKey(
                        name: "FK_Empresa_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CV",
                columns: table => new
                {
                    IdCV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Concelho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Escolaridade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpProfissional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Competencias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Interesses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCandidatoCv = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CV", x => x.IdCV);
                    table.ForeignKey(
                        name: "FK_CV_Candidato_IdCandidatoCv",
                        column: x => x.IdCandidatoCv,
                        principalTable: "Candidato",
                        principalColumn: "IdCandidato");
                });

            migrationBuilder.CreateTable(
                name: "FileCV",
                columns: table => new
                {
                    IdFile = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IdCandidatoFile = table.Column<int>(type: "int", nullable: false),
                    CandidatoIdCandidato = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCV", x => x.IdFile);
                    table.ForeignKey(
                        name: "FK_FileCV_Candidato_CandidatoIdCandidato",
                        column: x => x.CandidatoIdCandidato,
                        principalTable: "Candidato",
                        principalColumn: "IdCandidato",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Foto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCandidatoFoto = table.Column<int>(type: "int", nullable: false),
                    CandidatoIdCandidato = table.Column<int>(type: "int", nullable: false),
                    FotoPerfil = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foto_Candidato_CandidatoIdCandidato",
                        column: x => x.CandidatoIdCandidato,
                        principalTable: "Candidato",
                        principalColumn: "IdCandidato",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Foto_Candidato_IdCandidatoFoto",
                        column: x => x.IdCandidatoFoto,
                        principalTable: "Candidato",
                        principalColumn: "IdCandidato",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogoEmpresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresaFoto = table.Column<int>(type: "int", nullable: false),
                    empresaIdEmpresa = table.Column<int>(type: "int", nullable: false),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogoEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogoEmpresa_Empresa_empresaIdEmpresa",
                        column: x => x.empresaIdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogoEmpresa_Empresa_IdEmpresaFoto",
                        column: x => x.IdEmpresaFoto,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfertaEmprego",
                columns: table => new
                {
                    IdOferta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salario = table.Column<float>(type: "real", nullable: true),
                    Concelho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoContrato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Requisitos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VagaDisponivel = table.Column<bool>(type: "bit", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contagem = table.Column<int>(type: "int", nullable: false),
                    Jornada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegimeTrabalho = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfertaEmprego", x => x.IdOferta);
                    table.ForeignKey(
                        name: "FK_OfertaEmprego_Empresa_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    IdReview = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.IdReview);
                    table.ForeignKey(
                        name: "FK_Review_Empresa_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AplicacaoTrabalho",
                columns: table => new
                {
                    IdAplicacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdOferta = table.Column<int>(type: "int", nullable: true),
                    IdCandidato = table.Column<int>(type: "int", nullable: true),
                    DataAplicacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    aplicacaoAceite = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacaoTrabalho", x => x.IdAplicacao);
                    table.ForeignKey(
                        name: "FK_AplicacaoTrabalho_Candidato_IdCandidato",
                        column: x => x.IdCandidato,
                        principalTable: "Candidato",
                        principalColumn: "IdCandidato");
                    table.ForeignKey(
                        name: "FK_AplicacaoTrabalho_OfertaEmprego_IdOferta",
                        column: x => x.IdOferta,
                        principalTable: "OfertaEmprego",
                        principalColumn: "IdOferta");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacaoTrabalho_IdCandidato",
                table: "AplicacaoTrabalho",
                column: "IdCandidato");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacaoTrabalho_IdOferta",
                table: "AplicacaoTrabalho",
                column: "IdOferta");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Candidato_UserId",
                table: "Candidato",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CV_IdCandidatoCv",
                table: "CV",
                column: "IdCandidatoCv");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_UserId",
                table: "Empresa",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileCV_CandidatoIdCandidato",
                table: "FileCV",
                column: "CandidatoIdCandidato");

            migrationBuilder.CreateIndex(
                name: "IX_Foto_CandidatoIdCandidato",
                table: "Foto",
                column: "CandidatoIdCandidato");

            migrationBuilder.CreateIndex(
                name: "IX_Foto_IdCandidatoFoto",
                table: "Foto",
                column: "IdCandidatoFoto");

            migrationBuilder.CreateIndex(
                name: "IX_LogoEmpresa_empresaIdEmpresa",
                table: "LogoEmpresa",
                column: "empresaIdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_LogoEmpresa_IdEmpresaFoto",
                table: "LogoEmpresa",
                column: "IdEmpresaFoto");

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdEmpresa",
                table: "OfertaEmprego",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_Review_IdEmpresa",
                table: "Review",
                column: "IdEmpresa");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AplicacaoTrabalho");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CV");

            migrationBuilder.DropTable(
                name: "FileCV");

            migrationBuilder.DropTable(
                name: "Foto");

            migrationBuilder.DropTable(
                name: "LogoEmpresa");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "OfertaEmprego");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Candidato");

            migrationBuilder.DropTable(
                name: "Empresa");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
