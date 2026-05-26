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
                name: "Concelho",
                columns: table => new
                {
                    IdConcelho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeConcelho = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concelho", x => x.IdConcelho);
                });

            migrationBuilder.CreateTable(
                name: "TipoContrato",
                columns: table => new
                {
                    IdTipoContrato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoContrato", x => x.IdTipoContrato);
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
                    IdConcelho = table.Column<int>(type: "int", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_Empresa_Concelho_IdConcelho",
                        column: x => x.IdConcelho,
                        principalTable: "Concelho",
                        principalColumn: "IdConcelho",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CV",
                columns: table => new
                {
                    IdCV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdConcelho = table.Column<int>(type: "int", nullable: false),
                    Educacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_CV_Concelho_IdConcelho",
                        column: x => x.IdConcelho,
                        principalTable: "Concelho",
                        principalColumn: "IdConcelho",
                        onDelete: ReferentialAction.Cascade);
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
                });

            migrationBuilder.CreateTable(
                name: "OfertaEmprego",
                columns: table => new
                {
                    IdOferta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    EmpresaIdEmpresa = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salario = table.Column<float>(type: "real", nullable: true),
                    IdConcelho = table.Column<int>(type: "int", nullable: true),
                    IdTipoContrato = table.Column<int>(type: "int", nullable: true),
                    Requisitos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VagaDisponivel = table.Column<bool>(type: "bit", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contagem = table.Column<int>(type: "int", nullable: false),
                    IsFullTime = table.Column<bool>(type: "bit", nullable: true),
                    IsPresencial = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfertaEmprego", x => x.IdOferta);
                    table.ForeignKey(
                        name: "FK_OfertaEmprego_Concelho_IdConcelho",
                        column: x => x.IdConcelho,
                        principalTable: "Concelho",
                        principalColumn: "IdConcelho");
                    table.ForeignKey(
                        name: "FK_OfertaEmprego_Empresa_EmpresaIdEmpresa",
                        column: x => x.EmpresaIdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfertaEmprego_TipoContrato_IdTipoContrato",
                        column: x => x.IdTipoContrato,
                        principalTable: "TipoContrato",
                        principalColumn: "IdTipoContrato");
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

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 1, "Abrantes" },
                    { 2, "Aguiar da Beira" },
                    { 3, "Alandroal" },
                    { 4, "Albergaria-a-Velha" },
                    { 5, "Albufeira" },
                    { 6, "Alcanena" },
                    { 7, "Alcobaça" },
                    { 8, "Alcochete" },
                    { 9, "Alcoutim" },
                    { 10, "Alcácer do Sal" },
                    { 11, "Alenquer" },
                    { 12, "Alfândega da Fé" },
                    { 13, "Alijó" },
                    { 14, "Aljezur" },
                    { 15, "Aljustrel" },
                    { 16, "Almada" },
                    { 17, "Almeida" },
                    { 18, "Almeirim" },
                    { 19, "Almodôvar" },
                    { 20, "Alpiarça" },
                    { 21, "Alter do Chão" },
                    { 22, "Alvaiázere" },
                    { 23, "Alvito" },
                    { 24, "Amadora" },
                    { 25, "Amarante" },
                    { 26, "Amares" },
                    { 27, "Anadia" },
                    { 28, "Angra do Heroísmo" },
                    { 29, "Ansião" },
                    { 30, "Arcos de Valdevez" },
                    { 31, "Arganil" },
                    { 32, "Armamar" },
                    { 33, "Arouca" },
                    { 34, "Arraiolos" },
                    { 35, "Arronches" },
                    { 36, "Arruda dos Vinhos" },
                    { 37, "Aveiro" },
                    { 38, "Avis" },
                    { 39, "Azambuja" },
                    { 40, "Baião" },
                    { 41, "Barcelos" },
                    { 42, "Barrancos" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 43, "Barreiro" },
                    { 44, "Batalha" },
                    { 45, "Beja" },
                    { 46, "Belmonte" },
                    { 47, "Benavente" },
                    { 48, "Bombarral" },
                    { 49, "Borba" },
                    { 50, "Boticas" },
                    { 51, "Braga" },
                    { 52, "Bragança" },
                    { 53, "Cabeceiras de Basto" },
                    { 54, "Cadaval" },
                    { 55, "Caldas da Rainha" },
                    { 56, "Calheta " },
                    { 57, "Caminha" },
                    { 58, "Campo Maior" },
                    { 59, "Cantanhede" },
                    { 60, "Carrazeda de Ansiães" },
                    { 61, "Carregal do Sal" },
                    { 62, "Cartaxo" },
                    { 63, "Cascais" },
                    { 64, "Castanheira de Pêra" },
                    { 65, "Castelo Branco" },
                    { 66, "Castelo de Paiva" },
                    { 67, "Castelo de Vide" },
                    { 68, "Castro Daire" },
                    { 69, "Castro Marim" },
                    { 70, "Castro Verde" },
                    { 71, "Celorico da Beira" },
                    { 72, "Celorico de Basto" },
                    { 73, "Chamusca" },
                    { 74, "Chaves" },
                    { 75, "Cinfães" },
                    { 76, "Coimbra" },
                    { 77, "Condeixa-a-Nova" },
                    { 78, "Constância" },
                    { 79, "Coruche" },
                    { 80, "Corvo" },
                    { 81, "Covilhã" },
                    { 82, "Crato" },
                    { 83, "Cuba" },
                    { 84, "Câmara de Lobos" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 85, "Elvas" },
                    { 86, "Entroncamento" },
                    { 87, "Espinho" },
                    { 88, "Esposende" },
                    { 89, "Estarreja" },
                    { 90, "Estremoz" },
                    { 91, "Fafe" },
                    { 92, "Faro" },
                    { 93, "Felgueiras" },
                    { 94, "Ferreira do Alentejo" },
                    { 95, "Ferreira do Zêzere" },
                    { 96, "Figueira da Foz" },
                    { 97, "Figueira de Castelo Rodrigo" },
                    { 98, "Figueiró dos Vinhos" },
                    { 99, "Fornos de Algodres" },
                    { 100, "Freixo de Espada à Cinta" },
                    { 101, "Fronteira" },
                    { 102, "Funchal" },
                    { 103, "Fundão" },
                    { 104, "Gavião" },
                    { 105, "Golegã" },
                    { 106, "Gondomar" },
                    { 107, "Gouveia" },
                    { 108, "Grândola" },
                    { 109, "Guarda" },
                    { 110, "Guimarães" },
                    { 111, "Góis" },
                    { 112, "Horta" },
                    { 113, "Idanha-a-Nova" },
                    { 114, "Lagoa " },
                    { 115, "Lagos" },
                    { 116, "Lajes das Flores" },
                    { 117, "Lajes do Pico" },
                    { 118, "Lamego" },
                    { 119, "Leiria" },
                    { 120, "Lisboa" },
                    { 121, "Loulé" },
                    { 122, "Loures" },
                    { 123, "Lourinhã" },
                    { 124, "Lousada" },
                    { 125, "Lousã" },
                    { 126, "Macedo de Cavaleiros" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 127, "Machico" },
                    { 128, "Madalena" },
                    { 129, "Mafra" },
                    { 130, "Maia" },
                    { 131, "Mangualde" },
                    { 132, "Manteigas" },
                    { 133, "Marco de Canaveses" },
                    { 134, "Marinha Grande" },
                    { 135, "Marvão" },
                    { 136, "Matosinhos" },
                    { 137, "Mação" },
                    { 138, "Mealhada" },
                    { 139, "Meda" },
                    { 140, "Melgaço" },
                    { 141, "Mesão Frio" },
                    { 142, "Mira" },
                    { 143, "Miranda do Corvo" },
                    { 144, "Miranda do Douro" },
                    { 145, "Mirandela" },
                    { 146, "Mogadouro" },
                    { 147, "Moimenta da Beira" },
                    { 148, "Moita" },
                    { 149, "Monchique" },
                    { 150, "Mondim de Basto" },
                    { 151, "Monforte" },
                    { 152, "Montalegre" },
                    { 153, "Montemor-o-Novo" },
                    { 154, "Montemor-o-Velho" },
                    { 155, "Montijo" },
                    { 156, "Monção" },
                    { 157, "Mora" },
                    { 158, "Mortágua" },
                    { 159, "Moura" },
                    { 160, "Mourão" },
                    { 161, "Murtosa" },
                    { 162, "Murça" },
                    { 163, "Mértola" },
                    { 164, "Nazaré" },
                    { 165, "Nelas" },
                    { 166, "Nisa" },
                    { 167, "Nordeste" },
                    { 168, "Odemira" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 169, "Odivelas" },
                    { 170, "Oeiras" },
                    { 171, "Oleiros" },
                    { 172, "Olhão" },
                    { 173, "Oliveira de Azeméis" },
                    { 174, "Oliveira de Frades" },
                    { 175, "Oliveira do Bairro" },
                    { 176, "Oliveira do Hospital" },
                    { 177, "Ourique" },
                    { 178, "Ourém" },
                    { 179, "Ovar" },
                    { 180, "Palmela" },
                    { 181, "Pampilhosa da Serra" },
                    { 182, "Paredes" },
                    { 183, "Paredes de Coura" },
                    { 184, "Paços de Ferreira" },
                    { 185, "Pedrógão Grande" },
                    { 186, "Penacova" },
                    { 187, "Penafiel" },
                    { 188, "Penalva do Castelo" },
                    { 189, "Penamacor" },
                    { 190, "Penedono" },
                    { 191, "Penela" },
                    { 192, "Peniche" },
                    { 193, "Peso da Régua" },
                    { 194, "Pinhel" },
                    { 195, "Pombal" },
                    { 196, "Ponta Delgada" },
                    { 197, "Ponta do Sol" },
                    { 198, "Ponte da Barca" },
                    { 199, "Ponte de Lima" },
                    { 200, "Ponte de Sor" },
                    { 201, "Portalegre" },
                    { 202, "Portel" },
                    { 203, "Portimão" },
                    { 204, "Porto" },
                    { 205, "Porto Moniz" },
                    { 206, "Porto Santo" },
                    { 207, "Porto de Mós" },
                    { 208, "Povoação" },
                    { 209, "Praia da Vitória" },
                    { 210, "Proença-a-Nova" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 211, "Póvoa de Lanhoso" },
                    { 212, "Póvoa de Varzim" },
                    { 213, "Redondo" },
                    { 214, "Reguengos de Monsaraz" },
                    { 215, "Resende" },
                    { 216, "Ribeira Brava" },
                    { 217, "Ribeira Grande" },
                    { 218, "Ribeira de Pena" },
                    { 219, "Rio Maior" },
                    { 220, "Sabrosa" },
                    { 221, "Sabugal" },
                    { 222, "Salvaterra de Magos" },
                    { 223, "Santa Comba Dão" },
                    { 224, "Santa Cruz" },
                    { 225, "Santa Cruz da Graciosa" },
                    { 226, "Santa Cruz das Flores" },
                    { 227, "Santa Maria da Feira" },
                    { 228, "Santa Marta de Penaguião" },
                    { 229, "Santana" },
                    { 230, "Santarém" },
                    { 231, "Santiago do Cacém" },
                    { 232, "Santo Tirso" },
                    { 233, "Sardoal" },
                    { 234, "Seia" },
                    { 235, "Seixal" },
                    { 236, "Sernancelhe" },
                    { 237, "Serpa" },
                    { 238, "Sertã" },
                    { 239, "Sesimbra" },
                    { 240, "Setúbal" },
                    { 241, "Sever do Vouga" },
                    { 242, "Silves" },
                    { 243, "Sines" },
                    { 244, "Sintra" },
                    { 245, "Sobral de Monte Agraço" },
                    { 246, "Soure" },
                    { 247, "Sousel" },
                    { 248, "Sátão" },
                    { 249, "São Brás de Alportel" },
                    { 250, "São João da Madeira" },
                    { 251, "São João da Pesqueira" },
                    { 252, "São Pedro do Sul" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 253, "São Roque do Pico" },
                    { 254, "São Vicente" },
                    { 255, "Tabuaço" },
                    { 256, "Tarouca" },
                    { 257, "Tavira" },
                    { 258, "Terras de Bouro" },
                    { 259, "Tomar" },
                    { 260, "Tondela" },
                    { 261, "Torre de Moncorvo" },
                    { 262, "Torres Novas" },
                    { 263, "Torres Vedras" },
                    { 264, "Trancoso" },
                    { 265, "Trofa" },
                    { 266, "Tábua" },
                    { 267, "Vagos" },
                    { 268, "Vale de Cambra" },
                    { 269, "Valença" },
                    { 270, "Valongo" },
                    { 271, "Valpaços" },
                    { 272, "Velas" },
                    { 273, "Vendas Novas" },
                    { 274, "Viana do Alentejo" },
                    { 275, "Viana do Castelo" },
                    { 276, "Vidigueira" },
                    { 277, "Vieira do Minho" },
                    { 278, "Vila Flor" },
                    { 279, "Vila Franca de Xira" },
                    { 280, "Vila Franca do Campo" },
                    { 281, "Vila Nova da Barquinha" },
                    { 282, "Vila Nova de Cerveira" },
                    { 283, "Vila Nova de Famalicão" },
                    { 284, "Vila Nova de Foz Côa" },
                    { 285, "Vila Nova de Gaia" },
                    { 286, "Vila Nova de Paiva" },
                    { 287, "Vila Nova de Poiares" },
                    { 288, "Vila Pouca de Aguiar" },
                    { 289, "Vila Real" },
                    { 290, "Vila Real de Santo António" },
                    { 291, "Vila Velha de Ródão" },
                    { 292, "Vila Verde" },
                    { 293, "Vila Viçosa" },
                    { 294, "Vila de Rei" }
                });

            migrationBuilder.InsertData(
                table: "Concelho",
                columns: new[] { "IdConcelho", "NomeConcelho" },
                values: new object[,]
                {
                    { 295, "Vila do Bispo" },
                    { 296, "Vila do Conde" },
                    { 297, "Vila do Porto" },
                    { 298, "Vimioso" },
                    { 299, "Vinhais" },
                    { 300, "Viseu" },
                    { 301, "Vizela" },
                    { 302, "Vouzela" },
                    { 303, "Águeda" },
                    { 304, "Évora" },
                    { 305, "Ílhavo" },
                    { 306, "Óbidos" }
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
                name: "IX_CV_IdConcelho",
                table: "CV",
                column: "IdConcelho");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_IdConcelho",
                table: "Empresa",
                column: "IdConcelho");

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
                name: "IX_LogoEmpresa_empresaIdEmpresa",
                table: "LogoEmpresa",
                column: "empresaIdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_EmpresaIdEmpresa",
                table: "OfertaEmprego",
                column: "EmpresaIdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdConcelho",
                table: "OfertaEmprego",
                column: "IdConcelho");

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdTipoContrato",
                table: "OfertaEmprego",
                column: "IdTipoContrato");

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
                name: "TipoContrato");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Concelho");
        }
    }
}
