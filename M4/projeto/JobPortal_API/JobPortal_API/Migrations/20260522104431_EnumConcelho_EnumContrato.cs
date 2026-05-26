using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class EnumConcelho_EnumContrato : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CV_Concelho_IdConcelho",
                table: "CV");

            migrationBuilder.DropForeignKey(
                name: "FK_Empresa_Concelho_IdConcelho",
                table: "Empresa");

            migrationBuilder.DropForeignKey(
                name: "FK_OfertaEmprego_Concelho_IdConcelho",
                table: "OfertaEmprego");

            migrationBuilder.DropForeignKey(
                name: "FK_OfertaEmprego_TipoContrato_IdTipoContrato",
                table: "OfertaEmprego");

            migrationBuilder.DropTable(
                name: "Concelho");

            migrationBuilder.DropTable(
                name: "TipoContrato");

            migrationBuilder.DropIndex(
                name: "IX_OfertaEmprego_IdConcelho",
                table: "OfertaEmprego");

            migrationBuilder.DropIndex(
                name: "IX_OfertaEmprego_IdTipoContrato",
                table: "OfertaEmprego");

            migrationBuilder.DropIndex(
                name: "IX_Empresa_IdConcelho",
                table: "Empresa");

            migrationBuilder.DropIndex(
                name: "IX_CV_IdConcelho",
                table: "CV");

            migrationBuilder.DropColumn(
                name: "IdConcelho",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "IdTipoContrato",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "IdConcelho",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "IdConcelho",
                table: "CV");

            migrationBuilder.AddColumn<string>(
                name: "Concelho",
                table: "OfertaEmprego",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoContrato",
                table: "OfertaEmprego",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Concelho",
                table: "Empresa",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Concelho",
                table: "CV",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concelho",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "TipoContrato",
                table: "OfertaEmprego");

            migrationBuilder.DropColumn(
                name: "Concelho",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "Concelho",
                table: "CV");

            migrationBuilder.AddColumn<int>(
                name: "IdConcelho",
                table: "OfertaEmprego",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoContrato",
                table: "OfertaEmprego",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdConcelho",
                table: "Empresa",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdConcelho",
                table: "CV",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                    Tipo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoContrato", x => x.IdTipoContrato);
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

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdConcelho",
                table: "OfertaEmprego",
                column: "IdConcelho");

            migrationBuilder.CreateIndex(
                name: "IX_OfertaEmprego_IdTipoContrato",
                table: "OfertaEmprego",
                column: "IdTipoContrato");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_IdConcelho",
                table: "Empresa",
                column: "IdConcelho");

            migrationBuilder.CreateIndex(
                name: "IX_CV_IdConcelho",
                table: "CV",
                column: "IdConcelho");

            migrationBuilder.AddForeignKey(
                name: "FK_CV_Concelho_IdConcelho",
                table: "CV",
                column: "IdConcelho",
                principalTable: "Concelho",
                principalColumn: "IdConcelho",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Empresa_Concelho_IdConcelho",
                table: "Empresa",
                column: "IdConcelho",
                principalTable: "Concelho",
                principalColumn: "IdConcelho",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OfertaEmprego_Concelho_IdConcelho",
                table: "OfertaEmprego",
                column: "IdConcelho",
                principalTable: "Concelho",
                principalColumn: "IdConcelho");

            migrationBuilder.AddForeignKey(
                name: "FK_OfertaEmprego_TipoContrato_IdTipoContrato",
                table: "OfertaEmprego",
                column: "IdTipoContrato",
                principalTable: "TipoContrato",
                principalColumn: "IdTipoContrato");
        }
    }
}
