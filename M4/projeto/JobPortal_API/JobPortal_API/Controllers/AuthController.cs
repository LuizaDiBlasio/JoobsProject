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

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
            IUserHelper userHelper, IConfiguration configuration, IMailHelper mailHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _userHelper = userHelper;
            _configuration = configuration;
            _mailHelper = mailHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role); // "Admin", "Candidato", "Empresa"


                if (model.Role == "Candidato")
                {
                    var candidato = new Candidato
                    {
                        UserId = user.Id,
                        Nome = model.Name,
                        Email = model.Email
                    };
                    _context.Candidato.Add(candidato);
                }
                else if (model.Role == "Empresa")
                {
                    var empresa = new Empresa
                    {
                        UserId = user.Id,
                        Nome = model.Name,
                        Email = model.Email
                    };
                    _context.Empresa.Add(empresa);
                }

                await _context.SaveChangesAsync();

                return Ok("User created successfully");
            }


            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");

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
                    string tokenLink = $"http://localhost:5020/Auth/RecoverPassword?userId={user.Id}&token={Uri.EscapeDataString(myToken)}"; // garante que o token seja codificado corretamente mesmo com caracteres especiais

                    APIResponse response = _mailHelper.SendEmail(dto.Email, "Retrieve your password", $"<h1>Retrieve your password, token expires in one hour</h1>" +
                   $"<br><br><a href = \"{tokenLink}\">Click here to reset your password</a>"); //Contruir email e enviá-lo com o link

                    if (response.IsSuccess) //se conseguiu enviar o email
                    {
                        return StatusCode(200, new { Message = "A link to retrieve password has been sent to your email" });
                    }

                    //se não conseguiu enviar email:
                    return StatusCode(500, new { Message = "Unable to retrieve password, please contact admin" });

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
    