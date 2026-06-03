using System.Drawing.Text;
using System.Runtime.CompilerServices;
using AutoMapper;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using JobPortal_API.Models.Enums;
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

        public SeedDB(ApplicationDbContext context, IUserHelper userHelper, IMapper mapper/*, IHostEnvironment env*/)
        {
            _userHelper = userHelper;
            _context = context;
            _mapper = mapper;
            //_env = env;
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

                //if (_env.IsDevelopment()) // Decidimos manter dados também após a publicação
                //{
                    await SeedDevDataAsync();
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine("SeedDb exception: " + ex);
                throw;
            }
        }


        private async Task SeedDevDataAsync()
        {
            //  Criar Empresas
            await CreateEmpresaSeedAsync("empresa1@empresa1.com", "Esquadrias ltda", ConcelhoEnum.Abrantes, "Esquadrias", 222222222);
            await CreateEmpresaSeedAsync("empresa2@empresa2.com", "Papel ltda", ConcelhoEnum.Agueda, "Papel", 222222222);

            //  Criar Candidatos
            await CreateCandidatoSeedAsync("candidato1@candidato1.com", "Julia Matias", "Barreiro", new DateTime(1995, 05, 25), 222222222);
            await CreateCandidatoSeedAsync("candidato2@candidato2.com", "Julia Bandeira", "Barreiro", new DateTime(1995, 04, 25), 222222222);

            //Criar Cv para candidato 1
            var candidato1 = await _context.Candidato.FirstOrDefaultAsync(c => c.Email == "candidato1@candidato1.com");
            if (candidato1 != null)
            {
                await CreateCvSeedAsync(
                    idCandidato: candidato1.IdCandidato,
                    nomeCv: "CV - Julia Matias - Engenharia",
                    concelho: ConcelhoEnum.Abrantes, // Substitui pelo Enums corretos se necessário
                    escolaridade: EscolaridadeEnum.Licenciatura,
                    expProfissional: "• Desenvolvedora Backend Estagiária (1 ano)\n• Projetos Académicos em C# e ASP.NET Core",
                    competencias: "C#, ASP.NET Core, Entity Framework, SQL Server, Git",
                    interesses: "Desenvolvimento de APIs, Cloud Computing, Arquitetura de Software"
                );
            }

            //  Criar Admin Geral
            await CreateSeedAdminAsync("Admin@Admin.com", "Admin");

            //  Criar Dados Relacionados (Ofertas, Reviews e Candidaturas)
            await SeedOfertasReviewsEAlplicacoesAsync();

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

        private async Task CreateEmpresaSeedAsync(string email, string nome, ConcelhoEnum concelho, string zonaAtuacao, int telefone)
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
                        Concelho = concelho, 
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
                    await _userHelper.AddUserToRoleAsync(candidatoUser, "Candidato");

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

        private async Task SeedOfertasReviewsEAlplicacoesAsync()
        {
            var empresa1 = await _context.Empresa.FirstOrDefaultAsync(e => e.Email == "empresa1@empresa1.com");
            var empresa2 = await _context.Empresa.FirstOrDefaultAsync(e => e.Email == "empresa2@empresa2.com");
            var candidato1 = await _context.Candidato.FirstOrDefaultAsync(c => c.Email == "candidato1@candidato1.com");

            if (empresa1 == null || empresa2 == null || candidato1 == null) return;

            // Criar Ofertas de Emprego 
            if (!await _context.OfertaEmprego.AnyAsync())
            {
                var oferta1 = new OfertaEmprego
                {
                    IdEmpresa = empresa1.IdEmpresa,
                    Titulo = "Desenvolvedor Backend .NET",
                    Salario = 1500,
                    Concelho = ConcelhoEnum.Abrantes,
                    TipoContrato = TipoContratoEnum.SemTermo, 
                    RegimeTrabalho = RegimeTrabalhoEnum.Remoto, 
                    Jornada = JornadaEnum.Flexivel, 
                    Requisitos = "C#, Entity Framework, SQL Server",
                    VagaDisponivel = true,
                    Descricao = "Venha trabalhar na melhor empresa de Esquadrias como Dev!",
                    Contagem = 0
                };

                var oferta2 = new OfertaEmprego
                {
                    IdEmpresa = empresa2.IdEmpresa,
                    Titulo = "Assistente de Produção",
                    Salario = 900,
                    Concelho = ConcelhoEnum.Agueda,
                    TipoContrato = TipoContratoEnum.SemTermo,
                    RegimeTrabalho = RegimeTrabalhoEnum.Presencial,
                    Jornada = JornadaEnum.FullTime,
                    Requisitos = "Experiência em ambiente fabril.",
                    VagaDisponivel = true,
                    Descricao = "Operação de máquinas de corte de papel.",
                    Contagem = 0
                };

                _context.OfertaEmprego.AddRange(oferta1, oferta2);
                await _context.SaveChangesAsync();

                if (!await _context.AplicacaoTrabalho.AnyAsync())
                {
                    var aplicacao = new AplicacaoTrabalho
                    {
                        IdOferta = oferta1.IdOferta,
                        IdCandidato = candidato1.IdCandidato,
                        DataAplicacao = DateTime.Now
                    };
                    _context.AplicacaoTrabalho.Add(aplicacao);
                }
            }

            // Criar Reviews para as Empresas 
            if (!await _context.Review.AnyAsync())
            {
                var review = new Review
                {
                    IdEmpresa = empresa1.IdEmpresa,
                    Titulo = "Excelente ambiente",
                    Descricao = "Empresa muito organizada e profissionais acolhedores.",
                    Rating = 5,
                    DataCriacao = DateTime.Now
                };

                _context.Review.Add(review);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CreateCvSeedAsync(
            int idCandidato,
            string nomeCv,
            ConcelhoEnum concelho,
            EscolaridadeEnum escolaridade,
            string expProfissional,
            string competencias,
            string interesses)
        {
            // Garante que não criamos CVs duplicados para o mesmo candidato
            var cvExistente = await _context.CV.AnyAsync(c => c.IdCandidatoCv == idCandidato);
            if (!cvExistente)
            {
                var novoCv = new CV
                {
                    Nome = nomeCv,
                    Concelho = concelho,
                    Escolaridade = escolaridade,
                    ExpProfissional = expProfissional,
                    Competencias = competencias,
                    Interesses = interesses,
                    IdCandidatoCv = idCandidato // Relacionamento estabelecido via ID recebido por parâmetro
                };

                _context.CV.Add(novoCv);
                await _context.SaveChangesAsync();
            }
        }



    }
}
