using System.Drawing.Text;
using System.Runtime.CompilerServices;
using AutoMapper;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using JobPortal_API.Utilities.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace JobPortal_API.Data
{
    //_____________NOVO FICHEIRO________
    public class SeedDB
    {
        private readonly IUserHelper _userHelper;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;

        public SeedDB(ApplicationDbContext context, IUserHelper userHelper, IMapper mapper, IHostEnvironment env)
        {
            _userHelper = userHelper;
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Aplicar migrações
                await _context.Database.MigrateAsync();

                // Criar roles
                await _userHelper.CreateRolesAsync();

                // Ver se role SysAdmin existe
                await _userHelper.CheckRoleAsync("SysAdmin");

                //CRIAR SYSADMIM
                await CreateSeedAdminAsync("sysAdmin@sysAdmin.com", "SysAdmin");

                if (_env.IsDevelopment())
                {
                    await SeedDevDataAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("SeedDb exception: " + ex);
                throw;
            }
        }


        private async Task SeedDevDataAsync()
        {
            await CreateEmpresaSeedAsync("empresa1@empresa1.com", "Esquadrias ltda", "Azeitão", "Esquadrias", 222222222);
            await CreateEmpresaSeedAsync("empresa2@empresa2.com", "Papel ltda", "Barreiro", "Papel", 222222222);
            await CreateCandidatoSeedAsync("candidato1@candidato1.com", "Julia Matias", "Barreiro", new DateTime(1995, 05, 25), 222222222);
            await CreateCandidatoSeedAsync("candidato2@candidato2.com", "Julia Bandeira", "Barreiro", new DateTime(1995, 04, 25), 222222222);
            await CreateSeedAdminAsync("Admin@Admin.com", "Admin");

        }

        // Método pode criar 2 roles de admins (SysAdmin e Admin)
        private async Task CreateSeedAdminAsync(string email, string adminRole)
        {

            // Ver se user admin existe
            var userAdmin = await _userHelper.GetUserByEmailAsync(email);
            if (userAdmin == null)
            {
                userAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };


                // Adicionar user
                var result = await _userHelper.AddUserAsync(userAdmin, "Abc123!");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the user in seeder.");
                }

                // Determinar role
                await _userHelper.AddUserToRoleAsync(userAdmin, adminRole);
            }
            else
            {
                // Checar de user está no role
                var isInRole = await _userHelper.IsUserInRoleAsync(userAdmin, adminRole);
                if (!isInRole)
                {
                    await _userHelper.AddUserToRoleAsync(userAdmin, adminRole);
                }
            }
        }

        private async Task CreateEmpresaSeedAsync(string email, string nome, string localidade, string zonaAtuacao, int telefone)
        {
            var empresaUser = await _userHelper.GetUserByEmailAsync(email);
            if (empresaUser == null)
            {
                empresaUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };


                // Adicionar user
                var result = await _userHelper.AddUserAsync(empresaUser, "Abc123!");

                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(empresaUser, "Empresa");

                    // Adicionar entidade Empresa
                    var empresa = new Empresa
                    {
                        UserId = empresaUser.Id,
                        Nome = nome,
                        User = empresaUser,
                        Localidade = localidade,
                        Email = email,
                        Telefone = 222222222,
                        NoFuncionarios = 50,
                        ZonaAtuacao = zonaAtuacao
                    };

                    _context.Empresa.Add(empresa);
                    await _context.SaveChangesAsync();
                }
            }    
        }

        private async Task CreateCandidatoSeedAsync(string email, string nome, string morada, DateTime dataNasc, int telefone)
        {
            var candidatoUser = await _userHelper.GetUserByEmailAsync(email);
            if (candidatoUser == null)
            {
                candidatoUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };


                // Adicionar user
                var result = await _userHelper.AddUserAsync(candidatoUser, "Abc123!");

                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(candidatoUser, "Empresa");

                    // Adicionar entidade Candidato
                    var candidato = new Candidato
                    {
                        UserId = candidatoUser.Id,
                        Nome = nome,
                        User = candidatoUser,
                        Morada = morada,
                        Email = email,
                        Telefone = 222222222,
                        DataNasc = dataNasc,
                    };

                    _context.Candidato.Add(candidato);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedListLocalidades()
        {
            List<string> Localidades = new List<string>
            {
                // AVEIRO
                "Águeda, Aveiro", "Albergaria-a-Velha, Aveiro", "Anadia, Aveiro", "Arouca, Aveiro", "Aveiro, Aveiro", "Castelo de Paiva, Aveiro", "Espinho, Aveiro", "Estarreja, Aveiro", "Ílhavo, Aveiro", "Mealhada, Aveiro", "Murtosa, Aveiro", "Oliveira de Azeméis, Aveiro", "Oliveira do Bairro, Aveiro", "Ovar, Aveiro", "Santa Maria da Feira, Aveiro", "São João da Madeira, Aveiro", "Sever do Vouga, Aveiro", "Vagos, Aveiro", "Vale de Cambra, Aveiro",

                // BEJA
                "Aljustrel, Beja", "Almodôvar, Beja", "Alvito, Beja", "Barrancos, Beja", "Beja, Beja", "Castro Verde, Beja", "Cuba, Beja", "Ferreira do Alentejo, Beja", "Mértola, Beja", "Moura, Beja", "Odemira, Beja", "Ourique, Beja", "Serpa, Beja", "Vidigueira, Beja",

                // BRAGA
                "Amares, Braga", "Barcelos, Braga", "Braga, Braga", "Cabeceiras de Basto, Braga", "Celorico de Basto, Braga", "Esposende, Braga", "Fafe, Braga", "Guimarães, Braga", "Póvoa de Lanhoso, Braga", "Terras de Bouro, Braga", "Vieira do Minho, Braga", "Vila Nova de Famalicão, Braga", "Vila Verde, Braga", "Vizela, Braga",

                // BRAGANÇA
                "Alfândega da Fé, Bragança", "Bragança, Bragança", "Carrazeda de Ansiães, Bragança", "Freixo de Espada à Cinta, Bragança", "Macedo de Cavaleiros, Bragança", "Miranda do Douro, Bragança", "Mirandela, Bragança", "Mogadouro, Bragança", "Torre de Moncorvo, Bragança", "Vila Flor, Bragança", "Vimioso, Bragança", "Vinhais, Bragança",

                // CASTELO BRANCO
                "Belmonte, Castelo Branco", "Castelo Branco, Castelo Branco", "Covilhã, Castelo Branco", "Fundão, Castelo Branco", "Idanha-a-Nova, Castelo Branco", "Oleiros, Castelo Branco", "Penamacor, Castelo Branco", "Proença-a-Nova, Castelo Branco", "Sertã, Castelo Branco", "Vila de Rei, Castelo Branco", "Vila Velha de Ródão, Castelo Branco",

                // COIMBRA
                "Arganil, Coimbra", "Cantanhede, Coimbra", "Coimbra, Coimbra", "Condeixa-a-Nova, Coimbra", "Figueira da Foz, Coimbra", "Góis, Coimbra", "Lousã, Coimbra", "Mira, Coimbra", "Miranda do Corvo, Coimbra", "Montemor-o-Velho, Coimbra", "Oliveira do Hospital, Coimbra", "Pampilhosa da Serra, Coimbra", "Penacova, Coimbra", "Penela, Coimbra", "Soure, Coimbra", "Tábua, Coimbra", "Vila Nova de Poiares, Coimbra",

                // ÉVORA
                "Alandroal, Évora", "Arraiolos, Évora", "Borba, Évora", "Estremoz, Évora", "Évora, Évora", "Montemor-o-Novo, Évora", "Mora, Évora", "Mourão, Évora", "Portel, Évora", "Redondo, Évora", "Reguengos de Monsaraz, Évora", "Vendas Novas, Évora", "Viana do Alentejo, Évora", "Vila Viçosa, Évora",

                // FARO
                "Albufeira, Faro", "Alcoutim, Faro", "Aljezur, Faro", "Castro Marim, Faro", "Faro, Faro", "Lagoa, Faro", "Lagos, Faro", "Loulé, Faro", "Monchique, Faro", "Olhão, Faro", "Portimão, Faro", "São Brás de Alportel, Faro", "Silves, Faro", "Tavira, Faro", "Vila do Bispo, Faro", "Vila Real de Santo António, Faro",

                // GUARDA
                "Aguiar da Beira, Guarda", "Almeida, Guarda", "Celorico da Beira, Guarda", "Figueira de Castelo Rodrigo, Guarda", "Fornos de Algodres, Guarda", "Gouveia, Guarda", "Guarda, Guarda", "Manteigas, Guarda", "Mêda, Guarda", "Pinhel, Guarda", "Sabugal, Guarda", "Seia, Guarda", "Trancoso, Guarda", "Vila Nova de Foz Côa, Guarda",

                // LEIRIA
                "Alcobaça, Leiria", "Alvaiázere, Leiria", "Ansião, Leiria", "Batalha, Leiria", "Bombarral, Leiria", "Caldas da Rainha, Leiria", "Castanheira de Pera, Leiria", "Figueiró dos Vinhos, Leiria", "Leiria, Leiria", "Marinha Grande, Leiria", "Nazaré, Leiria", "Óbidos, Leiria", "Pedrógão Grande, Leiria", "Peniche, Leiria", "Pombal, Leiria", "Porto de Mós, Leiria",

                // LISBOA
                "Alenquer, Lisboa", "Amadora, Lisboa", "Arruda dos Vinhos, Lisboa", "Azambuja, Lisboa", "Cadaval, Lisboa", "Cascais, Lisboa", "Lisboa, Lisboa", "Loures, Lisboa", "Lourinhã, Lisboa", "Mafra, Lisboa", "Odivelas, Lisboa", "Oeiras, Lisboa", "Sintra, Lisboa", "Sobral de Monte Agraço, Lisboa", "Torres Vedras, Lisboa", "Vila Franca de Xira, Lisboa",

                // PORTALEGRE
                "Alter do Chão, Portalegre", "Arronches, Portalegre", "Avis, Portalegre", "Campo Maior, Portalegre", "Castelo de Vide, Portalegre", "Crato, Portalegre", "Elvas, Portalegre", "Fronteira, Portalegre", "Gavião, Portalegre", "Marvão, Portalegre", "Monforte, Portalegre", "Nisa, Portalegre", "Ponte de Sor, Portalegre", "Portalegre, Portalegre", "Sousel, Portalegre",

                // PORTO
                "Amarante, Porto", "Baião, Porto", "Felgueiras, Porto", "Gondomar, Porto", "Lousada, Porto", "Maia, Porto", "Marco de Canaveses, Porto", "Matosinhos, Porto", "Paços de Ferreira, Porto", "Paredes, Porto", "Penafiel, Porto", "Porto, Porto", "Póvoa de Varzim, Porto", "Santo Tirso, Porto", "Trofa, Porto", "Valongo, Porto", "Vila do Conde, Porto", "Vila Nova de Gaia, Porto",

                // SANTARÉM
                "Abrantes, Santarém", "Alcanena, Santarém", "Almeirim, Santarém", "Alpiarça, Santarém", "Benavente, Santarém", "Cartaxo, Santarém", "Chamusca, Santarém", "Constância, Santarém", "Coruche, Santarém", "Entroncamento, Santarém", "Ferreira do Zêzere, Santarém", "Golegã, Santarém", "Mação, Santarém", "Ourém, Santarém", "Rio Maior, Santarém", "Salvaterra de Magos, Santarém", "Santarém, Santarém", "Sardoal, Santarém", "Tomar, Santarém", "Torres Novas, Santarém", "Vila Nova da Barquinha, Santarém",

                // SETÚBAL
                "Alcácer do Sal, Setúbal", "Alcochete, Setúbal", "Almada, Setúbal", "Barreiro, Setúbal", "Grândola, Setúbal", "Moita, Setúbal", "Montijo, Setúbal", "Palmela, Setúbal", "Santiago do Cacém, Setúbal", "Seixal, Setúbal", "Sesimbra, Setúbal", "Setúbal, Setúbal", "Sines, Setúbal",

                // VIANA DO CASTELO
                "Arcos de Valdevez, Viana do Castelo", "Caminha, Viana do Castelo", "Melgaço, Viana do Castelo", "Monção, Viana do Castelo", "Paredes de Coura, Viana do Castelo", "Ponte da Barca, Viana do Castelo", "Ponte de Lima, Viana do Castelo", "Valença, Viana do Castelo", "Viana do Castelo, Viana do Castelo", "Vila Nova de Cerveira, Viana do Castelo",

                // VILA REAL
                "Alijó, Vila Real", "Boticas, Vila Real", "Chaves, Vila Real", "Mesão Frio, Vila Real", "Mondim de Basto, Vila Real", "Montalegre, Vila Real", "Murça, Vila Real", "Peso da Régua, Vila Real", "Ribeira de Pena, Vila Real", "Sabrosa, Vila Real", "Santa Marta de Penaguião, Vila Real", "Valpaços, Vila Real", "Vila Pouca de Aguiar, Vila Real", "Vila Real, Vila Real",

                // VISEU
                "Armamar, Viseu", "Carregal do Sal, Viseu", "Castro Daire, Viseu", "Cinfães, Viseu", "Lamego, Viseu", "Mangualde, Viseu", "Moimenta da Beira, Viseu", "Mortágua, Viseu", "Nelas, Viseu", "Oliveira de Frades, Viseu", "Penalva do Castelo, Viseu", "Penedono, Viseu", "Resende, Viseu", "Santa Comba Dão, Viseu", "São João da Pesqueira, Viseu", "São Pedro do Sul, Viseu", "Sátão, Viseu", "Sernancelhe, Viseu", "Tabuaço, Viseu", "Tarouca, Viseu", "Tondela, Viseu", "Vila Nova de Paiva, Viseu", "Viseu, Viseu", "Vouzela, Viseu",

                // AÇORES
                "Angra do Heroísmo, Açores", "Calheta, Açores", "Corvo, Açores", "Horta, Açores", "Lagoa, Açores", "Lajes das Flores, Açores", "Lajes do Pico, Açores", "Madalena, Açores", "Nordeste, Açores", "Ponta Delgada, Açores", "Povoação, Açores", "Praia da Vitória, Açores", "Ribeira Grande, Açores", "Santa Cruz da Graciosa, Açores", "Santa Cruz das Flores, Açores", "São Roque do Pico, Açores", "Velas, Açores", "Vila do Porto, Açores", "Vila Franca do Campo, Açores",

                // MADEIRA
                "Calheta, Madeira", "Câmara de Lobos, Madeira", "Funchal, Madeira", "Machico, Madeira", "Ponta do Sol, Madeira", "Porto Moniz, Madeira", "Porto Santo, Madeira", "Ribeira Brava, Madeira", "Santa Cruz, Madeira", "Santana, Madeira", "São Vicente, Madeira"
            };
        }

    }
}
