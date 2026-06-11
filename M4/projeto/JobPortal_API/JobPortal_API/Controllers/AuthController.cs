using Google.Apis.Auth;
using JobPortal_API.Data;
using JobPortal_API.DTOs;
using JobPortal_API.Models;
using JobPortal_API.Utilities.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobPortal_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
            IUserHelper userHelper, IConfiguration configuration, IMailHelper mailHelper, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _userHelper = userHelper;
            _configuration = configuration;
            _mailHelper = mailHelper;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { mensagem = "Os dados do registo não podem estar vazios." });
            }

            // Iniciar uma transação para garantir consistência entre o Identity e o DbContext
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                // 1. Criar Utilizador no Identity
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    // Retorna os erros específicos do Identity (ex: password fraca, email duplicado)
                    return BadRequest(new { mensagem = "Erro na validação do utilizador.", erros = result.Errors });
                }

                // 2. Atribuir Role
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user); // Remove o user criado se a role falhar
                    await transaction.RollbackAsync();
                    return BadRequest(new { mensagem = "Erro ao atribuir a role.", erros = roleResult.Errors });
                }

                // 3. Criar o perfil específico (Candidato ou Empresa)
                if (model.Role != null && model.Role.Equals("Candidato", StringComparison.OrdinalIgnoreCase))
                {
                    var candidato = new Candidato
                    {
                        UserId = user.Id,
                        Nome = model.Name,
                        Email = model.Email
                    };
                    _context.Candidato.Add(candidato);
                }
                else if (model.Role != null && model.Role.Equals("Empresa", StringComparison.OrdinalIgnoreCase))
                {
                    var empresa = new Empresa
                    {
                        UserId = user.Id,
                        Nome = model.Name,
                        Email = model.Email
                    };
                    _context.Empresa.Add(empresa);
                }
                else
                {
                    // Se a role enviada não for válida, cancela tudo
                    await _userManager.DeleteAsync(user);
                    await transaction.RollbackAsync();
                    return BadRequest(new { mensagem = $"A role '{model.Role}' não é válida. Escolha 'Candidato' ou 'Empresa'." });
                }

                // 4. Gravar o Perfil na Base de Dados
                await _context.SaveChangesAsync();

                // Se tudo correu bem, confirma a transação no SQL Server
                await transaction.CommitAsync();

                return Ok(new { mensagem = "User created successfully" });
            }
            catch (Exception ex)
            {
                // Se houver qualquer falha (ex: erro no SQL), desfaz as alterações
                await transaction.RollbackAsync();

                // Captura a mensagem mais profunda (geralmente o erro real do SQL Server)
                var erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                Console.WriteLine($"ERRO NO REGISTO: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"INNER EXCEPTION: {erroReal}");

                // Retorna o erro detalhado para conseguires ver no Frontend (F12 -> Network)
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao processar registo.",
                    detalhe = ex.Message,
                    erroInterno = erroReal,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return Unauthorized(new { mensagem = "Invalid credentials" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized(new { mensagem = "Invalid credentials"});

            var roles = await _userManager.GetRolesAsync(user);

            string idCandidato = null;
            string idEmpresa = null;

            if (roles.Contains("Candidato"))
            {
                var candidato = await _context.Candidato.FirstOrDefaultAsync(c => c.UserId == user.Id);
                idCandidato = candidato?.IdCandidato.ToString();
            }

            if (roles.Contains("Empresa"))
            {
                var empresa = await _context.Empresa.FirstOrDefaultAsync(c => c.UserId == user.Id);
                idEmpresa = empresa?.IdEmpresa.ToString();
            }

            var jwtToken = GenerateJwtToken(user, roles, idCandidato, idEmpresa);

            return Ok(new APIResponse
            {
                IsSuccess = true,
                Result = new LoginResponseDTO
                {
                    User = new UserDTO
                    {
                        UserName = user.UserName,
                        Role = roles.FirstOrDefault()
                    },
                    Token = jwtToken
                }
            });
        }


        private string GenerateJwtToken(ApplicationUser user, IList<string> roles, string? idCandidato, string? idEmpresa)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (idCandidato != null)
            {
                claims.Add(new Claim("IdCandidato", idCandidato));
            }

            if (idEmpresa != null)
            {
                claims.Add(new Claim("IdEmpresa", idEmpresa));
            }

            // Adiciona os IDs de negócio aos Claims para que os filtros (ex: VerificaCandidatoFilter) de autorização consigam validar o proprietário dos dados.
            if (!string.IsNullOrEmpty(idCandidato)) claims.Add(new Claim("IdCandidato", idCandidato));
            if (!string.IsNullOrEmpty(idEmpresa)) claims.Add(new Claim("IdEmpresa", idEmpresa));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("minha-chave-jwt-supersecreta-32bytes!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "JobPortalAPI",
                audience: "JobPortalAPI",
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO request)
        {
            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded) return BadRequest("Error creating user.");

                    await _userManager.AddToRoleAsync(user, "Candidato");

                    var candidato = new Candidato
                    {
                        UserId = user.Id,
                        Nome = payload.Name,
                        Email = payload.Email
                    };
                    _context.Candidato.Add(candidato);
                    await _context.SaveChangesAsync();
                }

                var roles = await _userManager.GetRolesAsync(user);

                string idCandidato = null;
                string idEmpresa = null;

                if (roles.Contains("Candidato"))
                {
                    var candidato = await _context.Candidato.FirstOrDefaultAsync(c => c.UserId == user.Id);
                    idCandidato = candidato?.IdCandidato.ToString();
                }

                if (roles.Contains("Empresa"))
                {
                    var empresa = await _context.Empresa.FirstOrDefaultAsync(c => c.UserId == user.Id);
                    idEmpresa = empresa?.IdEmpresa.ToString();
                }

                var jwtToken = GenerateJwtToken(user, roles, idCandidato, idEmpresa);

                return Ok(new APIResponse
                {
                    IsSuccess = true,
                    Result = new LoginResponseDTO
                    {
                        User = new UserDTO
                        {
                            UserName = user.UserName,
                            Role = roles.FirstOrDefault() ?? "Candidato"
                        },
                        Token = jwtToken
                    }
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized("Invalid Google token.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal server error occurred during authentication.");
            }
        }

        //___________ADIÇÃO DE CÓDIGO___________(Recuperação de password) 
        [HttpPost("GenerateForgotPasswordTokenAndEmail")]
                public async Task<IActionResult> GenerateForgotPasswordTokenAndEmail(ForgotPasswordDTO dto)
                {
                    var user = await _userHelper.GetUserByEmailAsync(dto.Email);

                    if (user == null)
                    {
                        return StatusCode(404, new { Message = "User not found", IsSuccess = false });
                    }

                    string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user); //gerar o token

                    // gera um link de confirmação para o email
                    string tokenLink = _config["WebAppSettings:BaseUrl"] + $"Auth/RecoverPassword?userId={user.Id}&token={Uri.EscapeDataString(myToken)}"; // garante que o token seja codificado corretamente mesmo com caracteres especiais

                    APIResponse response = _mailHelper.SendEmail(dto.Email, "Recuperação de password", $"<h1>Recupere sua password, token expira em uma hora</h1>" +
                   $"<br><br><a href = \"{tokenLink}\">Clique aqui para criar uma nova password</a>"); //Contruir email e enviá-lo com o link

                    if (response.IsSuccess) //se conseguiu enviar o email
                    {
                        return StatusCode(200, new { Message = "Foi enviado para o seu email um link de recuperação de password" });
                    }

                    //se não conseguiu enviar email:
                    return StatusCode(500, new { Message = "Não foi possível recuperar senha, favor contactar admin" });

                }




                //___________ADIÇÃO DE CÓDIGO___________(Recuperação de password) 
                [Microsoft.AspNetCore.Mvc.HttpPost("ResetPassword")]
                public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
                {
                    var user = await _userHelper.GetUserByIdAsync(resetPasswordDto.UserId); //verificar user

                    if (user == null)
                    {
                        return StatusCode(404, new APIResponse { Message = "User not found", IsSuccess = false });
                    }


                    var resetPassword = await _userHelper.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

                    if (resetPassword.Succeeded)
                    {
                        return StatusCode(200, new APIResponse { Message = "Password reset successfully, you can login now", IsSuccess = true });
                    }
                    else
                    {
                        return StatusCode(400, new APIResponse { Message = "An unexpected error occurred while resetting password, please try again", IsSuccess = false });
                    }
                }
    }
}
    