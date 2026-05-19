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
            await CreateEmpresaSeedAsync("empresa1@empresa1.com", "Esquadrias ltda", 1, "Esquadrias", 222222222);
            await CreateEmpresaSeedAsync("empresa2@empresa2.com", "Papel ltda", 2, "Papel", 222222222);
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

        private async Task CreateEmpresaSeedAsync(string email, string nome, int idConcelho, string zonaAtuacao, int telefone)
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
                        IdConcelho = idConcelho, 
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



    }
}
